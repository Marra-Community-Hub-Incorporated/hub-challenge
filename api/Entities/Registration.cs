namespace MiniHub.Api.Entities;

public class Registration : ITenantOwned
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid WorkshopId { get; set; }
    public Workshop? Workshop { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
