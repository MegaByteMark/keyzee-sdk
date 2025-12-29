using IntraDotNet.CleanArchitecture.Infrastructure.Extensions;

namespace KeyZee.Infrastructure.DbContext;

/// <summary>
/// The KeyZee database context.
/// </summary>
public sealed class KeyZeeDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    /// <summary>
    /// The KeyValuePairs DbSet.
    /// </summary>
    internal Microsoft.EntityFrameworkCore.DbSet<Domain.Models.KeyValuePair> KeyValuePairs { get; set; }

    /// <summary>
    /// The Apps DbSet.
    /// </summary>
    internal Microsoft.EntityFrameworkCore.DbSet<Domain.Models.App> Apps { get; set; }

    public KeyZeeDbContext(Microsoft.EntityFrameworkCore.DbContextOptions options)
       : base(options)
    {
    }

    protected override void OnModelCreating(Microsoft.EntityFrameworkCore.ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Domain.Models.KeyValuePair>().HasKey(kvp => kvp.Id);
        modelBuilder.Entity<Domain.Models.App>().HasKey(app => app.Id);

        // Configure auditable entities for soft deletion and auditing
        modelBuilder.UseAuditable<Domain.Models.KeyValuePair>();
        modelBuilder.UseAuditable<Domain.Models.App>();

        //Configure optimistic concurrency for entities that have RowVersion
        modelBuilder.UseOptimisticConcurrency<Domain.Models.KeyValuePair>();
        modelBuilder.UseOptimisticConcurrency<Domain.Models.App>();

        // Configure unique index for the app name because we can't have two apps with the same name
        modelBuilder.Entity<Domain.Models.App>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Configure unique index for the combination of AppId and Key in KeyValuePair as this will mnost likely be the most common lookup
        modelBuilder.Entity<Domain.Models.KeyValuePair>(entity =>
        {
            entity.HasIndex(e => new { e.AppId, e.Key }).IsUnique();
        });

        //configure relationships
        modelBuilder.Entity<Domain.Models.KeyValuePair>()
            .HasOne(kvp => kvp.Application)
            .WithMany()
            .HasForeignKey(kvp => kvp.AppId)
            .OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.Cascade);
    }
}