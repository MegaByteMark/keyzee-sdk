using System.Linq.Expressions;
using FluentValidation;
using IntraDotNet.CleanArchitecture.Application.Results;
using IntraDotNet.CleanArchitecture.Application.Services;
using KeyZee.Application.Common.Encryption;
using KeyZee.Application.Common.Persistence;
using KeyZee.Application.Common.Services;
using KeyZee.Application.Dtos;
using KeyZee.Infrastructure.Options;

namespace KeyZee.Application.Services;

/// <summary>
/// Service implementation for managing KeyValuePair entities.
/// </summary>
public sealed class KeyValuePairService : GuidValidatableDataService<Domain.Models.KeyValuePair, KeyValuePairDto>, IKeyValuePairService
{
    /// <summary>
    /// The Unit of Work for database operations.
    /// </summary>
    private readonly IKeyZeeUnitOfWork _unitOfWork;
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
        IKeyZeeUnitOfWork unitOfWork,
        IAppService appService,
        KeyZeeOptions options,
        IValidator<KeyValuePairDto> validator,
        IEncryptionService encryptionService
    )
    {
        _unitOfWork = unitOfWork;
        _keyValuePairRepository = unitOfWork.KeyValuePairRepository;
        _appService = appService;
        _options = options;
        _validator = validator;
        _encryptionService = encryptionService;
    }

    /// <summary>
    /// Maps a Domain.Models.KeyValuePair to a KeyValuePairDto.
    /// </summary>
    /// <param name="keyValuePair">The KeyValuePair domain model to map.</param>
    /// <returns>A KeyValuePairDto representing the mapped data.</returns>
    protected override KeyValuePairDto MapToDto(Domain.Models.KeyValuePair keyValuePair)
    {
        return new KeyValuePairDto
        {
            Id = keyValuePair.Id,
            AppId = keyValuePair.AppId,
            AppName = keyValuePair.Application!.Name,
            Key = keyValuePair.Key,
            // Decrypt the value before returning it back in plain text
            Value = DecryptValue(keyValuePair.EncryptedValue)
        };
    }

    /// <summary>
    /// Maps a KeyValuePairDto to a Domain.Models.KeyValuePair.
    /// </summary>
    /// <param name="dto">The KeyValuePairDto to map.</param>
    /// <returns>A Domain.Models.KeyValuePair representing the mapped data.</returns>
    protected override Domain.Models.KeyValuePair MapToEntity(KeyValuePairDto dto)
    {
        return new Domain.Models.KeyValuePair
        {
            Id = dto.Id,
            AppId = dto.AppId,
            Key = dto.Key,
            // Encrypt the value before storing it
            EncryptedValue = EncryptValue(dto.Value)
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

    /// <summary>
    /// Creates a new KeyValuePair entity internally.
    /// </summary>
    /// <param name="entity">The KeyValuePairDto to create.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A Result indicating success or failure.</returns>
    protected override async Task<Result> CreateInternalAsync(KeyValuePairDto entity, CancellationToken cancellationToken = default)
    {
        var keyValuePair = MapToEntity(entity);

        return await _keyValuePairRepository.AddOrUpdateAsync(keyValuePair, cancellationToken).ContinueWith(async task =>
        {
            if (task.IsFaulted)
            {
                return Result.Failure([task.Exception?.InnerException!]);
            }

            try
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure([ex]);
            }
        }, cancellationToken).Unwrap();
    }

    /// <summary>
    /// Updates an existing KeyValuePair entity internally.
    /// </summary>
    /// <param name="entity">The KeyValuePairDto to update.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A Result indicating success or failure.</returns>
    protected override async Task<Result> UpdateInternalAsync(KeyValuePairDto entity, CancellationToken cancellationToken = default)
    {
        var keyValuePair = MapToEntity(entity);

        return await _keyValuePairRepository.AddOrUpdateAsync(keyValuePair, cancellationToken).ContinueWith(async task =>
        {
            if (task.IsFaulted)
            {
                return Result.Failure([task.Exception?.InnerException!]);
            }

            try
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure([ex]);
            }
        }, cancellationToken).Unwrap();
    }

    /// <summary>
    /// Validates a KeyValuePairDto entity.
    /// </summary>
    /// <param name="entity">The KeyValuePairDto to validate.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A ValueResult indicating success or failure.</returns>
    public override async Task<ValueResult<bool>> ValidateAsync(KeyValuePairDto entity, CancellationToken cancellationToken = default)
    {
        return await _validator.ValidateAsync(entity, cancellationToken)
            .ContinueWith(task =>
            {
                var validationResult = task.Result;

                if (validationResult.IsValid)
                {
                    return ValueResult<bool>.Success(true);
                }
                else
                {
                    var errors = validationResult.Errors.Select(e => e.ErrorMessage);

                    return ValueResult<bool>.Failure(errors);
                }
            }, cancellationToken);
    }

    /// <summary>
    /// Deletes a KeyValuePair entity.
    /// </summary>
    /// <param name="entity">The KeyValuePairDto to delete.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A Result indicating success or failure.</returns>
    public override async Task<Result> DeleteAsync(KeyValuePairDto entity, CancellationToken cancellationToken = default)
    {
        var keyValuePair = MapToEntity(entity);

        return await _keyValuePairRepository.DeleteAsync(keyValuePair, cancellationToken).ContinueWith(async task =>
        {
            if (task.IsFaulted)
            {
                return Result.Failure([task.Exception?.InnerException!]);
            }

            try
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure([ex]);
            }
        }, cancellationToken).Unwrap();
    }

    /// <summary>
    /// Gets all KeyValuePair entities.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A ValueResult containing a collection of KeyValuePairDto objects.</returns>
    public override async Task<ValueResult<IEnumerable<KeyValuePairDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _keyValuePairRepository.GetAllAsync(withIncludes: true, includeDeleted: false, cancellationToken: cancellationToken)
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    return ValueResult<IEnumerable<KeyValuePairDto>>.Failure([task.Exception?.InnerException!]);
                }

                var keyValuePairs = task.Result;

                try
                {
                    var dtos = keyValuePairs.Select(MapToDto);

                    return ValueResult<IEnumerable<KeyValuePairDto>>.Success(dtos);
                }
                catch (Exception ex)
                {
                    return ValueResult<IEnumerable<KeyValuePairDto>>.Failure([ex]);
                }
            }, cancellationToken);
    }

    /// <summary>
    /// Finds KeyValuePair entities that match the specified predicate.
    /// </summary>
    /// <param name="predicate">The predicate to filter KeyValuePair entities.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A ValueResult containing a collection of KeyValuePairDto objects that match the predicate.</returns>
    public override async Task<ValueResult<IEnumerable<KeyValuePairDto>>> FindAsync(Expression<Func<Domain.Models.KeyValuePair, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _keyValuePairRepository.FindAsync(predicate, withIncludes: true, includeDeleted: false, cancellationToken: cancellationToken)
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    return ValueResult<IEnumerable<KeyValuePairDto>>.Failure([task.Exception?.InnerException!]);
                }

                var keyValuePairs = task.Result;

                try
                {
                    var dtos = keyValuePairs.Select(MapToDto);

                    return ValueResult<IEnumerable<KeyValuePairDto>>.Success(dtos);
                }
                catch (Exception ex)
                {
                    return ValueResult<IEnumerable<KeyValuePairDto>>.Failure([ex]);
                }
            }, cancellationToken);
    }

    /// <summary>
    ///  Gets a KeyValuePair entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the KeyValuePair entity.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A ValueResult containing the KeyValuePairDto object.</returns>
    public override async Task<ValueResult<KeyValuePairDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _keyValuePairRepository.GetByIdAsync(id, withIncludes: true, includeDeleted: false, cancellationToken: cancellationToken)
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    return ValueResult<KeyValuePairDto>.Failure([task.Exception?.InnerException!]);
                }

                var keyValuePair = task.Result;

                if (keyValuePair == null)
                {
                    return ValueResult<KeyValuePairDto>.Failure(["KeyValuePair not found."]);
                }

                try
                {
                    var dto = MapToDto(keyValuePair);
                    return ValueResult<KeyValuePairDto>.Success(dto);
                }
                catch (Exception ex)
                {
                    return ValueResult<KeyValuePairDto>.Failure([ex]);
                }
            }, cancellationToken);
    }

    /// <summary>
    /// Gets a KeyValuePair by application name and key, including soft-deleted entries.
    /// </summary>
    /// <param name="appName">The name of the application.</param>
    /// <param name="key">The key of the KeyValuePair.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A ValueResult containing the KeyValuePairDto object, including soft-deleted entries.</returns>
    public async Task<ValueResult<KeyValuePairDto?>> GetKeyValuePairByAppAndKeyWithSoftDeletedAsync(string appName, string key, CancellationToken cancellationToken = default)
    {
        return await InternalGetKeyValuePairByAppAndKeyAsync(appName, key, includeDeleted: true, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Gets a KeyValuePair by application name and key.
    /// </summary>
    /// <param name="appName">The name of the application.</param>
    /// <param name="key">The key of the KeyValuePair.</param>
    /// <param name="includeDeleted">Whether to include soft-deleted entries.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A ValueResult containing the KeyValuePairDto object.</returns>
    private async Task<ValueResult<KeyValuePairDto?>> InternalGetKeyValuePairByAppAndKeyAsync(string appName, string key, bool includeDeleted = false, CancellationToken cancellationToken = default)
    {
        appName ??= _options.AppName;

        var appResult = await _appService.GetByNameAsync(appName, cancellationToken);

        if (!appResult.IsSuccess || appResult.Value == null)
        {
            return ValueResult<KeyValuePairDto?>.Failure(["App not found."]);
        }

        return await _keyValuePairRepository.FindAsync(kvp => kvp.AppId == appResult.Value.Id && kvp.Key == key, withIncludes: true, includeDeleted: includeDeleted, cancellationToken: cancellationToken)
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    return ValueResult<KeyValuePairDto?>.Failure([task.Exception?.InnerException!]);
                }

                var keyValuePair = task.Result.FirstOrDefault();

                if (keyValuePair == null)
                {
                    return ValueResult<KeyValuePairDto?>.Success(null);
                }

                try
                {
                    var dto = MapToDto(keyValuePair);

                    return ValueResult<KeyValuePairDto?>.Success(dto);
                }
                catch (Exception ex)
                {
                    return ValueResult<KeyValuePairDto?>.Failure([ex]);
                }
            }, cancellationToken);
    }

    /// <summary>
    /// Gets a KeyValuePair by application name and key.
    /// </summary>
    /// <param name="appName">The name of the application.</param>
    /// <param name="key">The key of the KeyValuePair.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A ValueResult containing the KeyValuePairDto object.</returns>
    public async Task<ValueResult<KeyValuePairDto?>> GetKeyValuePairByAppAndKeyAsync(string appName, string key, CancellationToken cancellationToken = default)
    {
        return await InternalGetKeyValuePairByAppAndKeyAsync(appName, key, includeDeleted: false, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Gets a KeyValuePair by key.
    /// </summary>
    /// <param name="key">The key of the KeyValuePair.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A ValueResult containing the KeyValuePairDto object.</returns>
    public async Task<ValueResult<KeyValuePairDto?>> GetKeyValuePairByAppAndKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await GetKeyValuePairByAppAndKeyAsync(_options.AppName, key, cancellationToken);
    }

    /// <summary>
    ///  Gets all KeyValuePair entities for a specific application.
    /// </summary>
    /// <param name="appName">The name of the application.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A ValueResult containing a collection of KeyValuePairDto objects.</returns>
    public async Task<ValueResult<IEnumerable<KeyValuePairDto>>> GetKeyValuePairsByAppAsync(string appName, CancellationToken cancellationToken = default)
    {
        appName ??= _options.AppName;

        var appResult = await _appService.GetByNameAsync(appName, cancellationToken);

        if (!appResult.IsSuccess || appResult.Value == null)
        {
            return ValueResult<IEnumerable<KeyValuePairDto>>.Failure(["App not found."]);
        }

        return await _keyValuePairRepository.FindAsync(kvp => kvp.AppId == appResult.Value.Id, withIncludes: true, includeDeleted: false, cancellationToken: cancellationToken)
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    return ValueResult<IEnumerable<KeyValuePairDto>>.Failure([task.Exception?.InnerException!]);
                }

                var keyValuePairs = task.Result;

                if (keyValuePairs == null)
                {
                    return ValueResult<IEnumerable<KeyValuePairDto>>.Success([]);
                }

                try
                {
                    var dtos = keyValuePairs.Select(MapToDto);

                    return ValueResult<IEnumerable<KeyValuePairDto>>.Success(dtos);
                }
                catch (Exception ex)
                {
                    return ValueResult<IEnumerable<KeyValuePairDto>>.Failure([ex]);
                }
            }, cancellationToken);
    }

    /// <summary>
    /// Gets all KeyValuePair entities for the default application.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns></returns>
    public async Task<ValueResult<IEnumerable<KeyValuePairDto>>> GetKeyValuePairsByAppAsync(CancellationToken cancellationToken = default)
    {
        return await GetKeyValuePairsByAppAsync(_options.AppName, cancellationToken);
    }

    /// <summary>
    /// Deletes a KeyValuePair by key.
    /// </summary>
    /// <param name="key">The key of the KeyValuePair.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A Result indicating the success or failure of the delete operation.</returns>
    public async Task<Result> DeleteKeyValuePairByAppAndKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await DeleteKeyValuePairByAppAndKeyAsync(_options.AppName, key, cancellationToken);
    }

    /// <summary>
    /// Deletes a KeyValuePair by application name and key.
    /// </summary>
    /// <param name="appName">The name of the application.</param>
    /// <param name="key">The key of the KeyValuePair.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A Result indicating the success or failure of the delete operation.</returns>
    public async Task<Result> DeleteKeyValuePairByAppAndKeyAsync(string appName, string key, CancellationToken cancellationToken = default)
    {
        appName ??= _options.AppName;

        return await GetKeyValuePairByAppAndKeyAsync(appName, key, cancellationToken)
            .ContinueWith(async task =>
            {
                if (task.IsFaulted)
                {
                    return Result.Failure([task.Exception?.InnerException!]);
                }

                var keyValuePairDto = task.Result.Value;

                if (keyValuePairDto == null)
                {
                    return Result.Failure(["KeyValuePair not found."]);
                }

                try
                {
                    var deleteResult = await DeleteAsync(keyValuePairDto, cancellationToken);

                    if (!deleteResult.IsSuccess)
                    {
                        return deleteResult;
                    }

                    return Result.Success();
                }
                catch (Exception ex)
                {
                    return Result.Failure([ex]);
                }
            }, cancellationToken).Unwrap();
    }

    /// <summary>
    /// Migrates all KeyValuePair entries for a specific application to use a new encryption key and secret.
    /// </summary>
    /// <param name="appName">The name of the application.</param>
    /// <param name="newKey">The new encryption key.</param>
    /// <param name="newSecret">The new encryption secret.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A Result indicating the success or failure of the migration operation.</returns>
    public async Task<Result> MigrateByAppAsync(string appName, string newKey, string newSecret, CancellationToken cancellationToken = default)
    {
        string plainValue;

        appName ??= _options.AppName;

        var appResult = await _appService.GetByNameAsync(appName, cancellationToken);

        if (!appResult.IsSuccess || appResult.Value == null)
        {
            return Result.Failure(["App not found."]);
        }

        try
        {
            var kvps = await _keyValuePairRepository.FindAsync(kvp => kvp.AppId == appResult.Value.Id, withIncludes: true, asNoTracking: false, includeDeleted: true, cancellationToken: cancellationToken);

            if (kvps == null || !kvps.Any())
            {
                return Result.Success();
            }

            foreach (var kvp in kvps)
            {
                // Decrypt existing value using the current encryption key and secret from options
                plainValue = DecryptValue(kvp.EncryptedValue);

                // Re-encrypt value with the new key and secret and update the entity
                kvp.EncryptedValue = _encryptionService.Encrypt(plainValue, newKey, newSecret);

                await _keyValuePairRepository.AddOrUpdateAsync(kvp, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure([ex]);
        }
    }

    /// <summary>
    ///  Migrates a specific KeyValuePair entry to use a new encryption key and secret.
    /// </summary>
    /// <param name="appName">The name of the application.</param>
    /// <param name="key">The key of the KeyValuePair.</param>
    /// <param name="newKey">The new encryption key.</param>
    /// <param name="newSecret">The new encryption secret.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A Result indicating the success or failure of the migration operation.</returns>
    public async Task<Result> MigrateByAppAndKeyAsync(string appName, string key, string newKey, string newSecret, CancellationToken cancellationToken = default)
    {
        string plainValue;
        IEnumerable<Domain.Models.KeyValuePair> kvps;

        appName ??= _options.AppName;

        var appResult = await _appService.GetByNameAsync(appName, cancellationToken);

        if (!appResult.IsSuccess || appResult.Value == null)
        {
            return Result.Failure(["App not found."]);
        }

        try
        {
            kvps = await _keyValuePairRepository.FindAsync(kvp => kvp.AppId == appResult.Value.Id && kvp.Key == key, withIncludes: true, asNoTracking: false, includeDeleted: true, cancellationToken: cancellationToken);

            if (kvps == null || !kvps.Any())
            {
                return Result.Failure($"Key not found.");
            }
        }
        catch (Exception ex)
        {
            return Result.Failure([ex]);
        }

        try
        {
            var kvp = kvps.First();

            // Decrypt existing value using the current encryption key and secret from options
            plainValue = DecryptValue(kvp.EncryptedValue);

            // Re-encrypt value with the new key and secret and update the entity
            kvp.EncryptedValue = _encryptionService.Encrypt(plainValue, newKey, newSecret);

            await _keyValuePairRepository.AddOrUpdateAsync(kvp, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure([ex]);
        }
    }
}