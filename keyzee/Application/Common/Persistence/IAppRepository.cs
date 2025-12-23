using IntraDotNet.CleanArchitecture.Application.Common.Persistence;
using KeyZee.Domain.Models;

namespace KeyZee.Application.Common.Persistence;

/// <summary>
/// Repository interface for managing App entities.
/// </summary>
public interface IAppRepository: IGuidRepository<App>
{
}