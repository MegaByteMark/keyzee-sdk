using System.Linq.Expressions;
using System.Security;
using FluentValidation;
using IntraDotNet.CleanArchitecture.Application.Results;
using IntraDotNet.CleanArchitecture.Application.Services;
using KeyZee.Application.Common.Encryption;
using KeyZee.Application.Common.Persistence;
using KeyZee.Application.Common.Services;
using KeyZee.Application.Dtos;
using KeyZee.Infrastructure.Options;
using Microsoft.Extensions.Options;

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

    protected override async Task<Result> CreateInternalAsync(KeyValuePairDto entity, CancellationToken cancellationToken = default)
    {
        var keyValuePair = MapToEntity(entity);

        return await _keyValuePairRepository.AddOrUpdateAsync(keyValuePair, cancellationToken).ContinueWith(async task =>
        {
            if (task.IsFaulted)
            {
                return Result.Failure([task.Exception?.InnerException!]);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }, cancellationToken).Unwrap();
    }

    protected override async Task<Result> UpdateInternalAsync(KeyValuePairDto entity, CancellationToken cancellationToken = default)
    {
        var keyValuePair = MapToEntity(entity);

        return await _keyValuePairRepository.AddOrUpdateAsync(keyValuePair, cancellationToken).ContinueWith(async task =>
        {
            if (task.IsFaulted)
            {
                return Result.Failure([task.Exception?.InnerException!]);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }, cancellationToken).Unwrap();
    }

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

    public override async Task<Result> DeleteAsync(KeyValuePairDto entity, CancellationToken cancellationToken = default)
    {
        var keyValuePair = MapToEntity(entity);

        return await _keyValuePairRepository.DeleteAsync(keyValuePair, cancellationToken).ContinueWith(async task =>
        {
            if (task.IsFaulted)
            {
                return Result.Failure([task.Exception?.InnerException!]);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }, cancellationToken).Unwrap();
    }

    public override async Task<ValueResult<IEnumerable<KeyValuePairDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _keyValuePairRepository.GetAllAsync(cancellationToken: cancellationToken)
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    return ValueResult<IEnumerable<KeyValuePairDto>>.Failure([task.Exception?.InnerException!]);
                }

                var keyValuePairs = task.Result;
                var dtos = keyValuePairs.Select(MapToDto);

                return ValueResult<IEnumerable<KeyValuePairDto>>.Success(dtos);
            }, cancellationToken);
    }

    public override async Task<ValueResult<IEnumerable<KeyValuePairDto>>> FindAsync(Expression<Func<Domain.Models.KeyValuePair, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _keyValuePairRepository.FindAsync(predicate, cancellationToken: cancellationToken)
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    return ValueResult<IEnumerable<KeyValuePairDto>>.Failure([task.Exception?.InnerException!]);
                }

                var keyValuePairs = task.Result;
                var dtos = keyValuePairs.Select(MapToDto);

                return ValueResult<IEnumerable<KeyValuePairDto>>.Success(dtos);
            }, cancellationToken);
    }

    public override async Task<ValueResult<KeyValuePairDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _keyValuePairRepository.GetByIdAsync(id, cancellationToken: cancellationToken)
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

                var dto = MapToDto(keyValuePair);
                return ValueResult<KeyValuePairDto>.Success(dto);
            }, cancellationToken);
    }

    public async Task<ValueResult<KeyValuePairDto?>> GetKeyValuePairByAppAndKeyAsync(string appName, string key, CancellationToken cancellationToken = default)
    {
        return await _keyValuePairRepository.FindAsync(kvp => kvp.Application!.Name == appName && kvp.Key == key, cancellationToken: cancellationToken)
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

                var dto = MapToDto(keyValuePair);

                return ValueResult<KeyValuePairDto?>.Success(dto);
            }, cancellationToken);
    }

    public async Task<ValueResult<KeyValuePairDto?>> GetKeyValuePairByAppAndKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await GetKeyValuePairByAppAndKeyAsync(_options.AppName, key, cancellationToken);
    }

    public async Task<ValueResult<IEnumerable<KeyValuePairDto>>> GetKeyValuePairsByAppAsync(string appName, CancellationToken cancellationToken = default)
    {
        var appResult = await _appService.GetByNameAsync(_options.AppName, cancellationToken);

        if (!appResult.IsSuccess || appResult.Value == null)
        {
            return ValueResult<IEnumerable<KeyValuePairDto>>.Failure(["App not found."]);
        }

        return await _keyValuePairRepository.FindAsync(kvp => kvp.AppId == appResult.Value.Id, cancellationToken: cancellationToken)
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

                var dtos = keyValuePairs.Select(MapToDto);

                return ValueResult<IEnumerable<KeyValuePairDto>>.Success(dtos);
            }, cancellationToken);
    }

    public async Task<ValueResult<IEnumerable<KeyValuePairDto>>> GetKeyValuePairsByAppAsync(CancellationToken cancellationToken = default)
    {
        return await GetKeyValuePairsByAppAsync(_options.AppName, cancellationToken);
    }

    public async Task<Result> DeleteKeyValuePairByAppAndKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        return await DeleteKeyValuePairByAppAndKeyAsync(_options.AppName, key, cancellationToken);
    }

    public async Task<Result> DeleteKeyValuePairByAppAndKeyAsync(string appName, string key, CancellationToken cancellationToken = default)
    {
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

                var deleteResult = await DeleteAsync(keyValuePairDto, cancellationToken);

                if (!deleteResult.IsSuccess)
                {
                    return deleteResult;
                }

                return Result.Success();
            }, cancellationToken).Unwrap();
    }
}