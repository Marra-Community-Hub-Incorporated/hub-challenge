namespace MiniHub.Api.Multitenancy;

/// <summary>Scoped per invocation. Set by TenantMiddleware, read by AppDbContext.</summary>
public interface ITenantProvider
{
    Guid? TenantId { get; }
    void Set(Guid tenantId);
}

public sealed class TenantProvider : ITenantProvider
{
    public Guid? TenantId { get; private set; }
    public void Set(Guid tenantId) => TenantId = tenantId;
}
