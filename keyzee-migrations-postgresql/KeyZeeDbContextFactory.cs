using KeyZee.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace KeyZee.Migrations.Sqlite;

// This is a design-time factory for EF Core migrations
public class KeyZeeDbContextFactory : IDesignTimeDbContextFactory<KeyZeeDbContext>
{
    public KeyZeeDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<KeyZeeDbContext>();
        
        // Use a dummy connection string for design-time operations
        // The actual connection string will be provided when running the bundle
        optionsBuilder.UseNpgsql("Host=localhost;Database=KeyZee;Username=postgres;Password=password", b => 
        {
            b.MigrationsAssembly("KeyZee.Migrations.Postgresql");
        });
        
        return new KeyZeeDbContext(optionsBuilder.Options);
    }
}