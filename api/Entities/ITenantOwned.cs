namespace MiniHub.Api.Entities;

/// <summary>
/// Marker for rows that belong to a tenant. AppDbContext filters every query on these
/// entities to the current tenant and stamps TenantId on insert, so functions never
/// have to remember to do either. This is the invariant the whole product rests on.
/// </summary>
public interface ITenantOwned
{
    Guid TenantId { get; set; }
}
