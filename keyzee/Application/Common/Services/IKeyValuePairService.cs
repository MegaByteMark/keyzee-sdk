using IntraDotNet.CleanArchitecture.Application.Common.Services;
using IntraDotNet.CleanArchitecture.Application.Results;

namespace KeyZee.Application.Common.Services;

/// <summary>
/// Service interface for managing KeyValuePair entities.
/// </summary>
public interface IKeyValuePairService: IGuidValidatableDataService<Domain.Models.KeyValuePair>
{
    Task<ValueResult<Domain.Models.KeyValuePair?>> GetKeyValuePairByAppAndKeyWithSoftDeletedAsync(string appName, string key, CancellationToken cancellationToken = default);
    Task<ValueResult<Domain.Models.KeyValuePair?>> GetKeyValuePairByAppAndKeyAsync(string appName, string key, CancellationToken cancellationToken = default);
    Task<ValueResult<Domain.Models.KeyValuePair?>> GetKeyValuePairByAppAndKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<ValueResult<IEnumerable<Domain.Models.KeyValuePair>>> GetKeyValuePairsByAppAsync(string appName, CancellationToken cancellationToken = default);
    Task<ValueResult<IEnumerable<Domain.Models.KeyValuePair>>> GetKeyValuePairsByAppAsync(CancellationToken cancellationToken = default);
    Task<Result> DeleteKeyValuePairByAppAndKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<Result> DeleteKeyValuePairByAppAndKeyAsync(string appName, string key, CancellationToken cancellationToken = default);
    Task<Result> MigrateByAppAndKeyAsync(string appName, string key, string newKey, string newSecret, CancellationToken cancellationToken = default);
    Task<Result> MigrateByAppAsync(string appName, string newKey, string newSecret, CancellationToken cancellationToken = default);
}