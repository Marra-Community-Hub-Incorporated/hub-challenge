using Microsoft.EntityFrameworkCore;
using MiniHub.Api.Data;
using MiniHub.Api.Entities;
using MiniHub.Api.Multitenancy;

namespace MiniHub.Api.Tests;

/// <summary>
/// One in-memory database shared by every context created from the same TestDb, so a
/// test can write as tenant A and read as tenant B. Mirrors how the API behaves: each
/// request gets a fresh AppDbContext bound to exactly one tenant.
/// </summary>
public sealed class TestDb
{
    private readonly string _name = Guid.NewGuid().ToString();
    public Tenant TenantA { get; } = new() { Id = Guid.NewGuid(), Slug = "a", Name = "Tenant A" };
    public Tenant TenantB { get; } = new() { Id = Guid.NewGuid(), Slug = "b", Name = "Tenant B" };

    public TestDb()
    {
        using var db = For(null);
        db.Tenants.AddRange(TenantA, TenantB);
        db.SaveChanges();
    }

    public AppDbContext For(Tenant? tenant)
    {
        var provider = new TenantProvider();
        if (tenant is not null) provider.Set(tenant.Id);
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(_name)
            .Options;
        return new AppDbContext(options, provider);
    }
}
