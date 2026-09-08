namespace MiniHub.Api.Entities;

/// <summary>An organisation using Mini Hub. Every other row belongs to exactly one tenant.</summary>
public class Tenant
{
    public Guid Id { get; set; }
    public required string Slug { get; set; }
    public required string Name { get; set; }
}
