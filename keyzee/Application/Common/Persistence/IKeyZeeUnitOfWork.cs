using IntraDotNet.Application.Core.Interfaces;

namespace KeyZee.Application.Common.Persistence;

public interface IKeyZeeUnitOfWork: IUnitOfWork
{
    IAppRepository AppRepository { get; }
    IKeyValuePairRepository KeyValuePairRepository { get; }
}