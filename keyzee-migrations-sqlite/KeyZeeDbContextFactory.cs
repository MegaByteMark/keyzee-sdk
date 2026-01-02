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
        optionsBuilder.UseSqlite("Data Source=KeyZee.db;", b => 
        {
            b.MigrationsAssembly("KeyZee.Migrations.Sqlite");
        });
        
        return new KeyZeeDbContext(optionsBuilder.Options);
    }
}