using System.Linq.Expressions;
using FluentValidation;
using IntraDotNet.CleanArchitecture.Application.Results;
using IntraDotNet.CleanArchitecture.Application.Services;
using KeyZee.Application.Common.Persistence;
using KeyZee.Application.Common.Services;
using KeyZee.Application.Dtos;
using KeyZee.Domain.Models;

namespace KeyZee.Application.Services;

/// <summary>
/// Service for managing App entities.
/// </summary>
public sealed class AppService : GuidValidatableDataService<App, AppDto>, IAppService
{
    /// <summary>
    /// The Unit of Work for database operations.
    /// </summary>
    private readonly IKeyZeeUnitOfWork _unitOfWork;
    /// <summary>
    /// The App repository.
    /// </summary>
    private readonly IAppRepository _appRepository;
    /// <summary>
    /// The App DTO validator.
    /// </summary>
    private readonly IValidator<AppDto> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppService"/> class.
    /// </summary>
    /// <param name="appRepository">The repository for App entities.</param>
    /// <param name="validator">The validator for AppDto.</param>
    public AppService(IKeyZeeUnitOfWork unitOfWork, IValidator<AppDto> validator)
    {
        _unitOfWork = unitOfWork;
        _appRepository = unitOfWork.AppRepository;
        _validator = validator;
    }

    /// <summary>
    /// Maps a Domain.Models.App to an AppDto.
    /// </summary>
    /// <param name="app">The App domain model.</param>
    /// <returns>The corresponding AppDto.</returns>
    protected override AppDto MapToDto(App app)
    {
        return new AppDto
        {
            Id = app.Id,
            Name = app.Name
        };
    }

    /// <summary>
    /// Maps an AppDto to a Domain.Models.App.
    /// </summary>
    /// <param name="dto">The AppDto object.</param>
    /// <returns>The corresponding Domain.Models.App entity.</returns>
    protected override App MapToEntity(AppDto dto)
    {
        return new App
        {
            Id = dto.Id,
            Name = dto.Name
        };
    }

    /// <summary>
    /// Deletes an App entity.
    /// </summary>
    /// <param name="entity">The AppDto object to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A Result indicating success or failure.</returns>
    public override async Task<Result> DeleteAsync(AppDto entity, CancellationToken cancellationToken = default)
    {
        var existing = await _appRepository.GetByIdAsync(entity.Id, cancellationToken: cancellationToken);

        if (existing is null)
        {
            return Result.Failure("App not found.");
        }

        try
        {
            await _appRepository.DeleteAsync(existing, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure([ex.Message]);
        }
    }

    /// <summary>
    /// Finds App entities based on a predicate.
    /// </summary>
    /// <param name="predicate">The predicate to filter App entities.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A ValueResult containing a collection of AppDto objects.</returns>
    public override async Task<ValueResult<IEnumerable<AppDto>>> FindAsync(Expression<Func<App, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _appRepository.FindAsync(predicate, cancellationToken: cancellationToken).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                return ValueResult<IEnumerable<AppDto>>.Failure([task.Exception?.InnerException!]);
            }

            var apps = task.Result;
            var appDtos = apps.Select(MapToDto);

            return ValueResult<IEnumerable<AppDto>>.Success(appDtos);
        }, cancellationToken);
    }

    /// <summary>
    /// Gets all App entities.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A ValueResult containing a collection of AppDto objects.</returns>
    public override async Task<ValueResult<IEnumerable<AppDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _appRepository.GetAllAsync(cancellationToken: cancellationToken).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                return ValueResult<IEnumerable<AppDto>>.Failure([task.Exception?.InnerException!]);
            }

            var apps = task.Result;
            var appDtos = apps.Select(MapToDto);

            return ValueResult<IEnumerable<AppDto>>.Success(appDtos);
        }, cancellationToken);
    }

    /// <summary>
    /// Gets an App entity by its ID.
    /// </summary>
    /// <param name="id">The ID of the App entity.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A ValueResult containing the AppDto object.</returns>
    public override async Task<ValueResult<AppDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _appRepository.GetByIdAsync(id, cancellationToken: cancellationToken)
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    return ValueResult<AppDto>.Failure([task.Exception?.InnerException!]);
                }

                var app = task.Result;

                if (app == null)
                {
                    return ValueResult<AppDto>.Failure("App not found.");
                }

                var appDto = MapToDto(app);

                return ValueResult<AppDto>.Success(appDto);
            }, cancellationToken);
    }

    /// <summary>
    /// Validates an AppDto entity.
    /// </summary>
    /// <param name="entity">The AppDto entity to validate.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A ValueResult indicating whether the entity is valid.</returns>
    public override async Task<ValueResult<bool>> ValidateAsync(AppDto entity, CancellationToken cancellationToken = default)
    {
        return await _validator.ValidateAsync(entity, cancellationToken)
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    return ValueResult<bool>.Failure([task.Exception?.InnerException!]);
                }

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
    /// Creates a new App entity.
    /// </summary>
    /// <param name="entity">The AppDto entity to create.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A Result indicating the success or failure of the operation.</returns>
    protected override async Task<Result> CreateInternalAsync(AppDto entity, CancellationToken cancellationToken = default)
    {
        var app = MapToEntity(entity);

        return await _appRepository.AddOrUpdateAsync(app, cancellationToken)
            .ContinueWith(async task =>
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
    /// Updates an existing App entity.
    /// </summary>
    /// <param name="entity">The AppDto entity to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A Result indicating the success or failure of the operation.</returns>
    protected override async Task<Result> UpdateInternalAsync(AppDto entity, CancellationToken cancellationToken = default)
    {
        var app = MapToEntity(entity);

        return await _appRepository.AddOrUpdateAsync(app, cancellationToken)
            .ContinueWith(async task =>
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
    /// Gets an App entity by its name.
    /// </summary>
    /// <param name="appName">The name of the App entity.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A ValueResult containing the AppDto if found, or null if not found.</returns>
    public async Task<ValueResult<AppDto?>> GetByNameAsync(string appName, CancellationToken cancellationToken = default)
    {
        return await _appRepository.FindAsync(a => a.Name == appName, cancellationToken: cancellationToken)
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    return ValueResult<AppDto?>.Failure([task.Exception?.InnerException!]);
                }

                var app = task.Result.FirstOrDefault();

                if (app == null)
                {
                    return ValueResult<AppDto?>.Success(null);
                }

                var appDto = MapToDto(app);

                return ValueResult<AppDto?>.Success(appDto);
            }, cancellationToken);
    }

    /// <summary>
    /// Deletes an App entity by its name.
    /// </summary>
    /// <param name="appName">The name of the App entity.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A Result indicating the success or failure of the operation.</returns>
    public async Task<Result> DeleteByNameAsync(string appName, CancellationToken cancellationToken = default)
    {
        var app = await GetByNameAsync(appName, cancellationToken);

        if (!app.IsSuccess)
        {
            return Result.Failure(app.Errors);
        }

        if (app.Value is null)
        {
            return Result.Failure("App not found.");
        }

        return await DeleteAsync(app.Value, cancellationToken);
    }

    /// <summary>
    /// Deletes an App entity by its ID.
    /// </summary>
    /// <param name="appId">The ID of the App entity.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A Result indicating the success or failure of the operation.</returns>
    public async Task<Result> DeleteByIdAsync(Guid appId, CancellationToken cancellationToken = default)
    {
        var app = await GetByIdAsync(appId, cancellationToken);

        if (!app.IsSuccess)
        {
            return Result.Failure(app.Errors);
        }

        if (app.Value is null)
        {
            return Result.Failure("App not found.");
        }

        return await DeleteAsync(app.Value, cancellationToken);
    }
}