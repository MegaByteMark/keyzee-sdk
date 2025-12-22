using FluentValidation;
using KeyZee.Application.Common.Persistence;
using KeyZee.Application.Common.Services;
using KeyZee.Application.Dtos;

namespace KeyZee.Application.Services;

/// <summary>
/// Service for managing App entities.
/// </summary>
public sealed class AppService : IAppService
{
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
    public AppService(IAppRepository appRepository, IValidator<AppDto> validator)
    {
        _appRepository = appRepository;
        _validator = validator;
    }

    /// <summary>
    /// Gets an App by its name.
    /// </summary>
    /// <param name="appName">The name of the App.</param>
    /// <returns>The AppDto if found; otherwise, null.</returns>
    public async Task<AppDto?> GetByNameAsync(string appName, CancellationToken cancellationToken = default)
    {
        var apps = await _appRepository.FindAsync(a => a.Name == appName, cancellationToken: cancellationToken);

        return apps.Select(MapToDto).FirstOrDefault();
    }

    /// <summary>
    /// Gets all Apps.
    /// </summary>
    /// <returns>A collection of AppDto representing all Apps.</returns>
    public async Task<IEnumerable<AppDto>> GetAllAppsAsync(CancellationToken cancellationToken = default)
    {
        var applications = await _appRepository.GetAllAsync(cancellationToken: cancellationToken);

        return applications.Select(MapToDto);
    }

    /// <summary>
    /// Saves an App.
    /// </summary>
    /// <param name="appDto">The AppDto to save.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ValidationException">Thrown when the AppDto is invalid.</exception>
    public async Task SaveAppAsync(AppDto appDto, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(appDto, cancellationToken);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var apps = await _appRepository.FindAsync(a => a.Name == appDto.Name);
        var application = apps.FirstOrDefault();

        if (application != null)
        {
            //Already exists
            return;
        }
        else
        {
            application = new Domain.Models.App
            {
                Name = appDto.Name
            };
        }

        await _appRepository.AddOrUpdateAsync(application, a => a.Id == application.Id);
    }

    /// <summary>
    /// Deletes an App by its name.
    /// </summary>
    /// <param name="appName">The name of the App to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DeleteAppAsync(string appName, CancellationToken cancellationToken = default)
    {
        var apps = await _appRepository.FindAsync(a => a.Name == appName, cancellationToken: cancellationToken);
        var application = apps.FirstOrDefault();

        if (application != null)
        {
            application.DeletedOn = DateTime.UtcNow;
            application.DeletedBy =
            Environment.UserDomainName + "\\" + Environment.UserName ?? "unknown";
            await _appRepository.AddOrUpdateAsync(application, a => a.Id == application.Id);
        }
    }

    /// <summary>
    /// Maps a Domain.Models.App to an AppDto.
    /// </summary>
    /// <param name="app">The App domain model.</param>
    /// <returns>The corresponding AppDto.</returns>
    private static AppDto MapToDto(Domain.Models.App app)
    {
        return new AppDto
        {
            Id = app.Id,
            Name = app.Name
        };
    }
}