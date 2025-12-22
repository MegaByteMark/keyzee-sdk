using KeyZee.DependencyInjection;
using dotenv.net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
internal class Program
{
    private static void Main(string[] args)
    {
        DotEnv.Load();

        using IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.AddKeyZee(options =>
                {
                    options.WithDbContextOptions(builder =>
                    {
                        builder.UseSqlServer(Environment.GetEnvironmentVariable("CONNECTION_STRING"));
                        builder.EnableDetailedErrors();
                        builder.EnableSensitiveDataLogging();
                    });

                    options.WithAppName("Seeder");
                    options.WithEncryptionKey(Environment.GetEnvironmentVariable("KEY")!);
                    options.WithEncryptionSecret(Environment.GetEnvironmentVariable("SECRET")!);
                });
            })
            .Build();

        host.Run();
    }
}