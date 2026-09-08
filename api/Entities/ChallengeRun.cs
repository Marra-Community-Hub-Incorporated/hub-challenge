namespace MiniHub.Api.Entities;

/// <summary>
/// One row per seed run. Not tenant-owned. The seed tool prints a token derived from
/// this row so a candidate can show, in their walkthrough call, that the stack really
/// ran on their machine. See tools/Seed/Program.cs.
/// </summary>
public class ChallengeRun
{
    public Guid Id { get; set; }
    public required string ChallengeId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
