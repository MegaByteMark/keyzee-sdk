using IntraDotNet.Application.Core.Interfaces;

namespace KeyZee.Application.Common.Persistence;

/// <summary>
/// Repository interface for managing KeyValuePair entities.
/// </summary>
public interface IKeyValuePairRepository: IBaseRepository<Domain.Models.KeyValuePair>
{
}
