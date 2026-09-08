namespace MiniHub.Api.Services;

/// <summary>
/// Mini Hub serves one city, so workshop times are entered as Melbourne wall-clock time
/// and stored in UTC (Postgres timestamptz). Everything that writes a StartsAtUtc must
/// go through here.
/// </summary>
public static class MelbourneTime
{
    public const string ZoneId = "Australia/Melbourne";

    /// <summary>Convert a Melbourne wall-clock time (Kind = Unspecified) to UTC.</summary>
    public static DateTime ToUtc(DateTime melbourneLocal)
    {
        // Postgres wants UTC-kind DateTimes for timestamptz, so make sure the kind is set.
        return DateTime.SpecifyKind(melbourneLocal, DateTimeKind.Utc);
    }
}
