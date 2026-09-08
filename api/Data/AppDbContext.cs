using Microsoft.EntityFrameworkCore;
using MiniHub.Api.Entities;
using MiniHub.Api.Multitenancy;

namespace MiniHub.Api.Data;

public class AppDbContext : DbContext
{
    private readonly ITenantProvider _tenant;

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantProvider tenant) : base(options)
    {
        _tenant = tenant;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Workshop> Workshops => Set<Workshop>();
    public DbSet<Registration> Registrations => Set<Registration>();
    public DbSet<ChallengeRun> ChallengeRuns => Set<ChallengeRun>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Tenant>().HasIndex(t => t.Slug).IsUnique();

        b.Entity<Workshop>().HasIndex(w => new { w.TenantId, w.StartsAtUtc });
        b.Entity<Workshop>()
            .HasMany(w => w.Registrations)
            .WithOne(r => r.Workshop)
            .HasForeignKey(r => r.WorkshopId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<Registration>().HasIndex(r => new { r.WorkshopId, r.Email }).IsUnique();

        // Global tenant filter: every query on a tenant-owned entity is scoped to the
        // current tenant. A request with no tenant resolved matches nothing.
        b.Entity<Workshop>().HasQueryFilter(w => w.TenantId == _tenant.TenantId);
        b.Entity<Registration>().HasQueryFilter(r => r.TenantId == _tenant.TenantId);
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        StampTenant();
        return base.SaveChangesAsync(ct);
    }

    public override int SaveChanges()
    {
        StampTenant();
        return base.SaveChanges();
    }

    private void StampTenant()
    {
        foreach (var entry in ChangeTracker.Entries<ITenantOwned>())
        {
            if (entry.State != EntityState.Added) continue;
            if (entry.Entity.TenantId != Guid.Empty) continue;
            entry.Entity.TenantId = _tenant.TenantId
                ?? throw new InvalidOperationException("No tenant resolved; refusing to insert an unowned row.");
        }
    }
}
