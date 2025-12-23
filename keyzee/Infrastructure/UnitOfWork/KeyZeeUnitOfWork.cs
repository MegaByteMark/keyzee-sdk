using IntraDotNet.CleanArchitecture.Infrastructure;
using KeyZee.Application.Common.Persistence;
using KeyZee.Infrastructure.DbContext;
using KeyZee.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KeyZee.Infrastructure.UnitOfWork;

public sealed class KeyZeeUnitOfWork : UnitOfWork<KeyZeeDbContext>, IKeyZeeUnitOfWork
{
    public IAppRepository AppRepository => GetRepository<IAppRepository>();
    public IKeyValuePairRepository KeyValuePairRepository => GetRepository<IKeyValuePairRepository>();

    public KeyZeeUnitOfWork(IDbContextFactory<KeyZeeDbContext> dbContextFactory) : base(dbContextFactory)
    {
    }

    protected override TRepository CreateRepository<TRepository>()
    {
        return typeof(TRepository) switch
        {
            var t when t == typeof(IAppRepository) => (TRepository)(object)new AppRepository(Context),
            var t when t == typeof(IKeyValuePairRepository) => (TRepository)(object)new KeyValuePairRepository(Context),
            _ => throw new NotSupportedException($"Repository of type {typeof(TRepository).Name} is not supported.")
        };
    }
}