using IntraDotNet.CleanArchitecture.Application.Common.Persistence;

namespace KeyZee.Application.Common.Persistence;

public interface IKeyZeeUnitOfWork: IUnitOfWork
{
    IAppRepository AppRepository { get; }
    IKeyValuePairRepository KeyValuePairRepository { get; }
}