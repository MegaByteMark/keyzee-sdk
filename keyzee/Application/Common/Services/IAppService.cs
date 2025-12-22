using KeyZee.Application.Dtos;

namespace KeyZee.Application.Common.Services;

/// <summary>
/// Service interface for managing App entities.
/// </summary>
public interface IAppService
{
    Task<IEnumerable<AppDto>> GetAllAppsAsync(CancellationToken cancellationToken = default);
    Task<AppDto?> GetAppByNameAsync(string appName, CancellationToken cancellationToken = default);
    Task<AppDto?> GetAppByIdAsync(Guid appId, CancellationToken cancellationToken = default);
    Task SaveAppAsync(AppDto appDto, CancellationToken cancellationToken = default);
    Task DeleteAppByNameAsync(string appName, CancellationToken cancellationToken = default);
    Task DeleteAppByIdAsync(Guid appId, CancellationToken cancellationToken = default);
}