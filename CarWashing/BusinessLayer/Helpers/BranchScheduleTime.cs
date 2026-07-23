namespace BusinessLayer.Helpers;

/// <summary>Converts the branch's civil schedule (Vietnam time) to persisted UTC instants.</summary>
public static class BranchScheduleTime
{
    private const string IanaId = "Asia/Bangkok";
    private const string WindowsId = "SE Asia Standard Time";

    public static TimeZoneInfo TimeZone { get; } = ResolveTimeZone();

    public static DateTime ToUtc(DateOnly date, TimeSpan time)
    {
        var local = DateTime.SpecifyKind(date.ToDateTime(TimeOnly.FromTimeSpan(time)), DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(local, TimeZone);
    }

    public static DateTime ToUtc(DateTimeOffset instant) => instant.UtcDateTime;

    public static DateTime ToLocal(DateTime utc) => TimeZoneInfo.ConvertTimeFromUtc(EnsureUtc(utc), TimeZone);

    public static DateOnly LocalDate(DateTime utc) => DateOnly.FromDateTime(ToLocal(utc));

    private static DateTime EnsureUtc(DateTime value) => value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc);

    private static TimeZoneInfo ResolveTimeZone()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById(IanaId); }
        catch (TimeZoneNotFoundException) { return TimeZoneInfo.FindSystemTimeZoneById(WindowsId); }
    }
}
