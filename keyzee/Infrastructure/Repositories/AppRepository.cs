using IntraDotNet.CleanArchitecture.Infrastructure.EFCore.Persistence;
using KeyZee.Application.Common.Persistence;
using KeyZee.Domain.Models;
using KeyZee.Infrastructure.DbContext;

namespace KeyZee.Infrastructure.Repositories;

public sealed class AppRepository(KeyZeeDbContext context) : GuidRepository<App, KeyZeeDbContext>(context), IAppRepository
{
}