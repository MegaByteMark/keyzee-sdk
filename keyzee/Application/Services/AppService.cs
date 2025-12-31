using System.Linq.Expressions;
using FluentValidation;
using IntraDotNet.CleanArchitecture.Application.Results;
using IntraDotNet.CleanArchitecture.Application.Services;
using KeyZee.Application.Common.Persistence;
using KeyZee.Application.Common.Services;
using KeyZee.Domain.Models;

namespace KeyZee.Application.Services;

/// <summary>
/// Service for managing App entities.
/// </summary>
public sealed class AppService : GuidValidatableDataService<App>, IAppService
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
    private readonly IValidator<App> _validator;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppService"/> class.
    /// </summary>
    /// <param name="unitOfWork">The Unit of Work for database operations.</param>
    /// <param name="validator">The validator for App.</param>
    public AppService(IKeyZeeUnitOfWork unitOfWork, IValidator<App> validator)
    {
        _unitOfWork = unitOfWork;
        _appRepository = unitOfWork.AppRepository;
        _validator = validator;
    }

    /// <summary>
    /// Deletes an App entity.
    /// </summary>
    /// <param name="entity">The App entity to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A Result indicating success or failure.</returns>
    public override async Task<Result> DeleteAsync(App entity, CancellationToken cancellationToken = default)
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
    /// <returns>A ValueResult containing a collection of App objects.</returns>
    public override async Task<ValueResult<IEnumerable<App>>> FindAsync(Expression<Func<App, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _appRepository.FindAsync(predicate, withIncludes: true, includeDeleted: false, cancellationToken: cancellationToken).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                return ValueResult<IEnumerable<App>>.Failure([task.Exception?.InnerException!]);
            }

            var apps = task.Result;

            return ValueResult<IEnumerable<App>>.Success(apps);
        }, cancellationToken);
    }

    /// <summary>
    /// Gets all App entities.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A ValueResult containing a collection of App objects.</returns>
    public override async Task<ValueResult<IEnumerable<App>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _appRepository.GetAllAsync(withIncludes: true, includeDeleted: false, cancellationToken: cancellationToken).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                return ValueResult<IEnumerable<App>>.Failure([task.Exception?.InnerException!]);
            }

            var apps = task.Result;

            return ValueResult<IEnumerable<App>>.Success(apps);
        }, cancellationToken);
    }

    /// <summary>
    /// Gets an App entity by its ID.
    /// </summary>
    /// <param name="id">The ID of the App entity.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A ValueResult containing the App object.</returns>
    public override async Task<ValueResult<App>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _appRepository.GetByIdAsync(id, withIncludes: true, includeDeleted: false, cancellationToken: cancellationToken)
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    return ValueResult<App>.Failure([task.Exception?.InnerException!]);
                }

                var app = task.Result;

                if (app == null)
                {
                    return ValueResult<App>.Failure("App not found.");
                }

                return ValueResult<App>.Success(app);
            }, cancellationToken);
    }

    /// <summary>
    /// Validates an AppDto entity.
    /// </summary>
    /// <param name="entity">The AppDto entity to validate.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A ValueResult indicating whether the entity is valid.</returns>
    public override async Task<ValueResult<bool>> ValidateAsync(App entity, CancellationToken cancellationToken = default)
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
    protected override async Task<ValueResult<App>> CreateInternalAsync(App entity, CancellationToken cancellationToken = default)
    {
        return await UpdateInternalAsync(entity, cancellationToken);
    }

    /// <summary>
    /// Updates an existing App entity.
    /// </summary>
    /// <param name="entity">The AppDto entity to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A Result indicating the success or failure of the operation.</returns>
    protected override async Task<ValueResult<App>> UpdateInternalAsync(App entity, CancellationToken cancellationToken = default)
    {
        //Check to see if another app with this name exists, this maintains our unique constraint on name.
        var existingAppResult = await GetByNameWithSoftDeletedAsync(entity.Name, cancellationToken);

        if (!existingAppResult.IsSuccess)
        {
            return ValueResult<App>.Failure(existingAppResult.Errors);
        }

        var existingApp = existingAppResult.Value;

        //If no id came in, check the DB to see if this app name exists already and if so update the id.
        if (entity.Id == Guid.Empty)
        {
            if (existingApp is not null)
            {
                //found an existing app use its ID
                entity.Id = existingApp.Id;
            }
            else
            {
                //new app, assign new ID
                entity.Id = Guid.NewGuid();
            }
        }
        else
        {
            if (existingApp is not null && existingApp.Id != entity.Id)
            {
                return ValueResult<App>.Failure($"An application with the name '{entity.Name}' already exists.");
            }
        }

        return await _appRepository.AddOrUpdateAsync(entity, cancellationToken)
            .ContinueWith(async task =>
            {
                if (task.IsFaulted)
                {
                    return ValueResult<App>.Failure([task.Exception?.InnerException!]);
                }

                try
                {
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    return ValueResult<App>.Success(entity);
                }
                catch (Exception ex)
                {
                    return ValueResult<App>.Failure([ex]);
                }
            }, cancellationToken).Unwrap();
    }

    /// <summary>
    /// Gets an App entity by its name.
    /// </summary>
    /// <param name="appName">The name of the App entity.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A ValueResult containing the App if found, or null if not found.</returns>
    public async Task<ValueResult<App?>> GetByNameAsync(string appName, CancellationToken cancellationToken = default)
    {
        return await _appRepository.FindAsync(a => a.Name == appName, withIncludes: true, includeDeleted: false, cancellationToken: cancellationToken)
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    return ValueResult<App?>.Failure([task.Exception?.InnerException!]);
                }

                var app = task.Result.FirstOrDefault();

                if (app == null)
                {
                    return ValueResult<App?>.Success(null);
                }

                return ValueResult<App?>.Success(app);
            }, cancellationToken);
    }

    public async Task<ValueResult<App?>> GetByNameWithSoftDeletedAsync(string appName, CancellationToken cancellationToken = default)
    {
        return await _appRepository.FindAsync(a => a.Name == appName, withIncludes: true, includeDeleted: true, cancellationToken: cancellationToken)
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    return ValueResult<App?>.Failure([task.Exception?.InnerException!]);
                }

                var app = task.Result.FirstOrDefault();

                if (app == null)
                {
                    return ValueResult<App?>.Success(null);
                }

                return ValueResult<App?>.Success(app);
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
    public override async Task<Result> DeleteByIdAsync(Guid appId, CancellationToken cancellationToken = default)
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