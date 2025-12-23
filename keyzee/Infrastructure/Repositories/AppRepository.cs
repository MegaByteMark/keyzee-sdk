using KeyZee.Application.Common.Persistence;
using KeyZee.Domain.Models;
using KeyZee.Infrastructure.DbContext;
using IntraDotNet.CleanArchitecture.Infrastructure;

namespace KeyZee.Infrastructure.Repositories;

public sealed class AppRepository(KeyZeeDbContext context) : GuidRepository<App, KeyZeeDbContext>(context), IAppRepository
{
}