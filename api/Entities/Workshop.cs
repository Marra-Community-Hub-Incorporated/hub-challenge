namespace MiniHub.Api.Entities;

public class Workshop : ITenantOwned
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public required string Title { get; set; }
    public string Description { get; set; } = "";
    public string Location { get; set; } = "";

    /// <summary>Stored as timestamptz. Always UTC; the frontend renders Melbourne time.</summary>
    public DateTime StartsAtUtc { get; set; }

    /// <summary>Maximum number of registrations. 0 means unlimited.</summary>
    public int Capacity { get; set; }

    public List<Registration> Registrations { get; set; } = new();
}
