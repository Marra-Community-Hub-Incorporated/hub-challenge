using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using MiniHub.Api.Multitenancy;

namespace MiniHub.Api.Data;

/// <summary>Lets `dotnet ef migrations add` build the context without the Functions host.</summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionStrings.Resolve(null))
            .Options;
        return new AppDbContext(options, new TenantProvider());
    }
}

public static class ConnectionStrings
{
    /// <summary>Matches compose.yaml. Host port 5433 so it never collides with a local Postgres.</summary>
    public const string LocalDocker = "Host=localhost;Port=5433;Username=minihub;Password=localdev;Database=minihub";

    public static string Resolve(string? configured) =>
        string.IsNullOrWhiteSpace(configured) ? LocalDocker : configured;
}
