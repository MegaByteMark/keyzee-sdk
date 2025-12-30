using IntraDotNet.CleanArchitecture.Application.Common.Services;
using IntraDotNet.CleanArchitecture.Application.Results;
using KeyZee.Domain.Models;

namespace KeyZee.Application.Common.Services;

/// <summary>
/// Service interface for managing App entities.
/// </summary>
public interface IAppService : IGuidValidatableDataService<App>
{
    Task<ValueResult<App?>> GetByNameAsync(string appName, CancellationToken cancellationToken = default);
    Task<Result> DeleteByNameAsync(string appName, CancellationToken cancellationToken = default);
}