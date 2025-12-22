using FluentValidation;
using KeyZee.Application.Common.Encryption;
using KeyZee.Application.Common.Exceptions;
using KeyZee.Application.Common.Persistence;
using KeyZee.Application.Common.Services;
using KeyZee.Application.Dtos;
using KeyZee.Infrastructure.Options;

namespace KeyZee.Application.Services;

/// <summary>
/// Service implementation for managing KeyValuePair entities.
/// </summary>
public sealed class KeyValuePairService : IKeyValuePairService
{
    /// <summary>
    /// The KeyValuePair repository.
    /// </summary>
    private readonly IKeyValuePairRepository _keyValuePairRepository;
    /// <summary>
    /// The App service.
    /// </summary>
    private readonly IAppService _appService;
    /// <summary>
    /// The KeyZee options.
    /// </summary>
    private readonly KeyZeeOptions _options;

    private readonly IEncryptionService _encryptionService;

    /// <summary>
    /// The KeyValuePair DTO validator.
    /// </summary>
    private readonly IValidator<KeyValuePairDto> _validator;

    public KeyValuePairService(
        IKeyValuePairRepository keyValuePairRepository,
        IAppService appService,
        KeyZeeOptions options,
        IValidator<KeyValuePairDto> validator,
        IEncryptionService encryptionService
    )
    {
        _keyValuePairRepository = keyValuePairRepository;
        _appService = appService;
        _options = options;
        _validator = validator;
        _encryptionService = encryptionService;
    }

    /// <summary>
    /// Gets all KeyValuePairs.
    /// </summary>
    /// <returns>A collection of KeyValuePairDto representing all KeyValuePairs.</returns>
    public async Task<IEnumerable<KeyValuePairDto>> GetAllKeyValuePairsAsync(CancellationToken cancellationToken = default)
    {
        var keyValuePairs = await _keyValuePairRepository.GetAllAsync(cancellationToken: cancellationToken);

        return [.. keyValuePairs.Select(MapToDto)];
    }

    /// <summary>
    /// Gets a KeyValuePair by application name and key.
    /// </summary>
    /// <param name="key">The key of the KeyValuePair.</param>
    /// <returns>The KeyValuePairDto if found; otherwise, null.</returns>
    public async Task<KeyValuePairDto?> GetKeyValuePairByAppAndKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await GetKeyValuePairByAppAndKeyAsync(_options.AppName, key, cancellationToken);
    }

    /// <summary>
    /// Gets a KeyValuePair by application name and key.
    /// </summary>
    /// <param name="appName">The name of the application.</param>
    /// <param name="key">The key of the KeyValuePair.</param>
    /// <returns>The KeyValuePairDto if found; otherwise, null.</returns>
    public async Task<KeyValuePairDto?> GetKeyValuePairByAppAndKeyAsync(string appName, string key, CancellationToken cancellationToken = default)
    {
        var keyValuePairs = await _keyValuePairRepository.FindAsync(c => c.Application!.Name == appName && c.Key == key, cancellationToken: cancellationToken);
        var keyValuePair = keyValuePairs.FirstOrDefault();

        if (keyValuePair == null)
        {
            return null;
        }

        return MapToDto(keyValuePair);
    }

    /// <summary>
    /// Gets KeyValuePairs by application name.
    /// </summary>
    /// <returns>A collection of KeyValuePairDto representing KeyValuePairs for the specified application.</returns>
    public async Task<IEnumerable<KeyValuePairDto>> GetKeyValuePairsByAppAsync(CancellationToken cancellationToken = default)
    {
        return await GetKeyValuePairsByAppAsync(_options.AppName, cancellationToken);
    }

    /// <summary>
    /// Gets KeyValuePairs by application name.
    /// </summary>
    /// <param name="appName">The name of the application.</param>
    /// <returns>A collection of KeyValuePairDto representing KeyValuePairs for the specified application.</returns>
    public async Task<IEnumerable<KeyValuePairDto>> GetKeyValuePairsByAppAsync(string appName, CancellationToken cancellationToken = default)
    {
        var keyValuePairs = await _keyValuePairRepository.FindAsync(c => c.Application!.Name == appName, cancellationToken: cancellationToken);

        return [.. keyValuePairs.Select(MapToDto)];
    }

    /// <summary>
    /// Gets a KeyValuePair by its ID.
    /// </summary>
    /// <param name="keyValuePairId">The ID of the KeyValuePair.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The KeyValuePairDto if found; otherwise, null.</returns>
    public async Task<KeyValuePairDto?> GetKeyValuePairByIdAsync(Guid keyValuePairId, CancellationToken cancellationToken = default)
    {
        var keyValuePair = await _keyValuePairRepository.GetAsync(kvp => kvp.Id == keyValuePairId, cancellationToken: cancellationToken);

        return keyValuePair == null ? null : MapToDto(keyValuePair);
    }

    /// <summary>
    /// Saves a KeyValuePair.
    /// </summary>
    /// <param name="keyValuePairDto">The KeyValuePairDto to save.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ValidationException">Thrown when the KeyValuePairDto fails validation.</exception>
    public async Task SaveKeyValuePairAsync(KeyValuePairDto keyValuePairDto, CancellationToken cancellationToken = default)
    {
        Domain.Models.KeyValuePair keyValuePair;

        //Ensure application name is set
        if (string.IsNullOrEmpty(keyValuePairDto.AppName))
        {
            keyValuePairDto.AppName = _options.AppName;
        }

        var validationResult = await _validator.ValidateAsync(keyValuePairDto, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var application = await _appService.GetAppByNameAsync(keyValuePairDto.AppName, cancellationToken);

        if (application == null)
        {
            //Ensure application is registered before we move onto the key value pair
            application = new AppDto { Name = keyValuePairDto.AppName };
            await _appService.SaveAppAsync(application, cancellationToken);
        }

        // Encrypt the value before saving it to the persistence layer
        var encryptedValue = EncryptValue(keyValuePairDto.Value);
        var existingKeyValuePairs = await _keyValuePairRepository.FindAsync(c => c.Application!.Name == keyValuePairDto.AppName && c.Key == keyValuePairDto.Key, cancellationToken: cancellationToken);

        if (existingKeyValuePairs != null && existingKeyValuePairs.Any())
        {
            keyValuePair = existingKeyValuePairs.First();
            keyValuePair.EncryptedValue = encryptedValue;
        }
        else
        {
            keyValuePair = new Domain.Models.KeyValuePair
            {
                Id = Guid.NewGuid(),
                AppId = application.Id,
                Key = keyValuePairDto.Key,
                EncryptedValue = encryptedValue
            };
        }

        // Save or update the KeyValuePair
        await _keyValuePairRepository.AddOrUpdateAsync(keyValuePair, c => c.Id == keyValuePair.Id, cancellationToken);
    }

    /// <summary>
    /// Deletes a KeyValuePair by key for the default application.
    /// </summary>
    /// <param name="key">The key of the KeyValuePair to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DeleteKeyValuePairByAppAndKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        await DeleteKeyValuePairByAppAndKeyAsync(_options.AppName, key, cancellationToken);
    }

    /// <summary>
    /// Deletes a KeyValuePair by application name and key.
    /// </summary>
    /// <param name="appName">The name of the application.</param>
    /// <param name="key">The key of the KeyValuePair to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DeleteKeyValuePairByAppAndKeyAsync(string appName, string key, CancellationToken cancellationToken = default)
    {
        var keyValuePairs = await _keyValuePairRepository.FindAsync(c => c.Application!.Name == appName && c.Key == key, cancellationToken: cancellationToken);

        if(keyValuePairs == null || !keyValuePairs.Any())
        {
            throw new NotFoundException($"KeyValuePair with key '{key}' for application '{appName}' not found.");
        }

        var keyValuePair = keyValuePairs.First();
        keyValuePair.DeletedOn = DateTime.UtcNow;
        keyValuePair.DeletedBy =
        Environment.UserDomainName + "\\" + Environment.UserName ?? "unknown";
        await _keyValuePairRepository.AddOrUpdateAsync(keyValuePair, c => c.Id == keyValuePair.Id, cancellationToken);
    }

    /// <summary>
    /// Deletes a KeyValuePair by its ID.
    /// </summary>
    /// <param name="keyValuePairId">The ID of the KeyValuePair to delete.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DeleteKeyValuePairByIdAsync(Guid keyValuePairId, CancellationToken cancellationToken = default)
    {
        var keyValuePair = await _keyValuePairRepository.GetAsync(kvp => kvp.Id == keyValuePairId, cancellationToken: cancellationToken) ?? throw new NotFoundException($"KeyValuePair with ID '{keyValuePairId}' not found.");

        keyValuePair.DeletedOn = DateTime.UtcNow;
        keyValuePair.DeletedBy =
        Environment.UserDomainName + "\\" + Environment.UserName ?? "unknown";
        await _keyValuePairRepository.AddOrUpdateAsync(keyValuePair, c => c.Id == keyValuePair.Id, cancellationToken);
    }

    /// <summary>
    /// Maps a Domain.Models.KeyValuePair to a KeyValuePairDto.
    /// </summary>
    /// <param name="keyValuePair">The KeyValuePair domain model to map.</param>
    /// <returns>A KeyValuePairDto representing the mapped data.</returns>
    private KeyValuePairDto MapToDto(Domain.Models.KeyValuePair keyValuePair)
    {
        return new KeyValuePairDto
        {
            AppName = keyValuePair.Application!.Name,
            Key = keyValuePair.Key,
            // Decrypt the value before returning it back in plain text
            Value = DecryptValue(keyValuePair.EncryptedValue)
        };
    }

    /// <summary>
    /// Decrypts an encrypted value.
    /// </summary>
    /// <param name="encryptedValue">The encrypted value to decrypt.</param>
    /// <returns>The decrypted plain text value.</returns>
    private string DecryptValue(string encryptedValue)
    {
        return _encryptionService.Decrypt(encryptedValue);
    }

    /// <summary>
    /// Encrypts a plain text value.
    /// </summary>
    /// <param name="plainValue">The plain text value to encrypt.</param>
    /// <returns>The encrypted value as a base64 string.</returns>
    private string EncryptValue(string plainValue)
    {
        return _encryptionService.Encrypt(plainValue);
    }
}