using IntraDotNet.CleanArchitecture.Application.Common.Services;
using IntraDotNet.CleanArchitecture.Application.Results;
using KeyZee.Application.Dtos;
using KeyZee.Domain.Models;

namespace KeyZee.Application.Common.Services;

/// <summary>
/// Service interface for managing App entities.
/// </summary>
public interface IAppService : IGuidValidatableDataService<App, AppDto>
{
    Task<ValueResult<AppDto?>> GetByNameAsync(string appName, CancellationToken cancellationToken = default);
    Task<Result> DeleteByNameAsync(string appName, CancellationToken cancellationToken = default);
    Task<Result> DeleteByIdAsync(Guid appId, CancellationToken cancellationToken = default);
}