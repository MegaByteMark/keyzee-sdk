using IntraDotNet.EntityFrameworkCore.Infrastructure.Repositories;
using KeyZee.Application.Common.Persistence;
using KeyZee.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace KeyZee.Infrastructure.Repositories;

public sealed class KeyValuePairRepository(KeyZeeDbContext context) : BaseAuditableRepository<Domain.Models.KeyValuePair, KeyZeeDbContext>(context), IKeyValuePairRepository
{
    protected override IQueryable<Domain.Models.KeyValuePair> AddIncludes(IQueryable<Domain.Models.KeyValuePair> query)
    {
        return base.AddIncludes(query).Include(c => c.Application);
    }
}