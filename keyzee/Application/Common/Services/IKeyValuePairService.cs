using KeyZee.Application.Dtos;

namespace KeyZee.Application.Common.Services;

/// <summary>
/// Service interface for managing KeyValuePair entities.
/// </summary>
public interface IKeyValuePairService
{
    Task<IEnumerable<KeyValuePairDto>> GetAllKeyValuePairsAsync(CancellationToken cancellationToken = default);
    Task<KeyValuePairDto?> GetKeyValuePairByAppAndKeyAsync(string appName, string key, CancellationToken cancellationToken = default);
    Task<KeyValuePairDto?> GetKeyValuePairByAppAndKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<IEnumerable<KeyValuePairDto>> GetKeyValuePairsByAppAsync(string appName, CancellationToken cancellationToken = default);
    Task<IEnumerable<KeyValuePairDto>> GetKeyValuePairsByAppAsync(CancellationToken cancellationToken = default);
    Task SaveKeyValuePairAsync(KeyValuePairDto keyValuePairDto, CancellationToken cancellationToken = default);
    Task DeleteKeyValuePairAsync(string key, CancellationToken cancellationToken = default);
    Task DeleteKeyValuePairAsync(string appName, string key, CancellationToken cancellationToken = default);
}