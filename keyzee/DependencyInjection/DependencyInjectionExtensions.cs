using FluentValidation;
using KeyZee.Application.Common.Encryption;
using KeyZee.Application.Common.Persistence;
using KeyZee.Application.Common.Services;
using KeyZee.Application.Dtos;
using KeyZee.Application.Services;
using KeyZee.Application.Validation;
using KeyZee.Infrastructure.DbContext;
using KeyZee.Infrastructure.Encryption;
using KeyZee.Infrastructure.Options;
using KeyZee.Infrastructure.Repositories;
using KeyZee.Infrastructure.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;

namespace KeyZee.DependencyInjection;

public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Adds KeyZee services to the IServiceCollection.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <param name="options">An action to configure KeyZeeOptionsBuilder.</param>
    /// <returns>The updated IServiceCollection.</returns>
    /// <exception cref="InvalidOperationException">Thrown when encryption key or secret is not provided.</exception>
    public static IServiceCollection AddKeyZee(this IServiceCollection services, Action<KeyZeeOptionsBuilder> options)
    {
        var optBuilder = new KeyZeeOptionsBuilder();
        options(optBuilder);

        var kzOptions = optBuilder.Build();

        if (string.IsNullOrEmpty(kzOptions.EncryptionKey) || string.IsNullOrEmpty(kzOptions.EncryptionSecret))
        {
            throw new InvalidOperationException("Encryption key and secret must be provided from environment variables.");
        }

        services.AddDbContextFactory<KeyZeeDbContext>(kzOptions.DbContextOptionsBuilder);

        services.AddValidators();
        services.AddRepositories();
        services.AddUnitOfWork();
        services.AddServices();

        // Register the configuration options as a singleton
        services.AddSingleton(kzOptions);

        return services;
    }

    private static IServiceCollection AddUnitOfWork(this IServiceCollection services)
    {
        services.AddScoped<IKeyZeeUnitOfWork, KeyZeeUnitOfWork>();

        return services;
    }

    /// <summary>
    /// Adds validators to the IServiceCollection.
    /// </summary>
    /// <param name="services">The IServiceCollection to add validators to.</param>
    /// <returns>The updated IServiceCollection.</returns>
    private static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddScoped<IValidator<AppDto>, AppValidator>();
        services.AddScoped<IValidator<KeyValuePairDto>, KeyValuePairValidator>();

        return services;
    }

    /// <summary>
    /// Adds repositories to the IServiceCollection.
    /// </summary>
    /// <param name="services">The IServiceCollection to add repositories to.</param>
    /// <returns>The updated IServiceCollection.</returns>
    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IKeyValuePairRepository, KeyValuePairRepository>();
        services.AddScoped<IAppRepository, AppRepository>();

        return services;
    }

    /// <summary>
    /// Adds services to the IServiceCollection.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <returns>The updated IServiceCollection.</returns>
    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IEncryptionService, AesEncryptionService>();
        services.AddScoped<IKeyValuePairService, KeyValuePairService>();
        services.AddScoped<IAppService, AppService>();

        return services;
    }
}