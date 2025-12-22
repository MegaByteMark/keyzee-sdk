using IntraDotNet.EntityFrameworkCore.Infrastructure.Repositories;
using KeyZee.Application.Common.Persistence;
using KeyZee.Domain.Models;
using KeyZee.Infrastructure.DbContext;

namespace KeyZee.Infrastructure.Repositories;

public sealed class AppRepository(KeyZeeDbContext context) : BaseAuditableRepository<App, KeyZeeDbContext>(context), IAppRepository
{
}