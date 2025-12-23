using IntraDotNet.CleanArchitecture.Application.Common.Persistence;

namespace KeyZee.Application.Common.Persistence;

/// <summary>
/// Repository interface for managing KeyValuePair entities.
/// </summary>
public interface IKeyValuePairRepository: IGuidRepository<Domain.Models.KeyValuePair>
{
}