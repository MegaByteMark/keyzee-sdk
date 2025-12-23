using IntraDotNet.CleanArchitecture.Application.Common.Services;
using IntraDotNet.CleanArchitecture.Application.Results;
using KeyZee.Application.Dtos;

namespace KeyZee.Application.Common.Services;

/// <summary>
/// Service interface for managing KeyValuePair entities.
/// </summary>
public interface IKeyValuePairService: IGuidValidatableDataService<Domain.Models.KeyValuePair, KeyValuePairDto>
{
    Task<ValueResult<KeyValuePairDto?>> GetKeyValuePairByAppAndKeyAsync(string appName, string key, CancellationToken cancellationToken = default);
    Task<ValueResult<KeyValuePairDto?>> GetKeyValuePairByAppAndKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<ValueResult<IEnumerable<KeyValuePairDto>>> GetKeyValuePairsByAppAsync(string appName, CancellationToken cancellationToken = default);
    Task<ValueResult<IEnumerable<KeyValuePairDto>>> GetKeyValuePairsByAppAsync(CancellationToken cancellationToken = default);
    Task<Result> DeleteKeyValuePairByAppAndKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<Result> DeleteKeyValuePairByAppAndKeyAsync(string appName, string key, CancellationToken cancellationToken = default);
}