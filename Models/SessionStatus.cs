namespace ForexTradingWorkspace.Models;

public class SessionStatus
{
    public string Name { get; set; } = "";
    public string SessionColor { get; set; } = "#000000";
    public TimeSpan UtcOpen { get; set; }
    public TimeSpan UtcClose { get; set; }
    public DateTime LocalOpen { get; set; }
    public DateTime LocalClose { get; set; }
    public bool IsCurrentlyOpen { get; set; }
    public TimeSpan TimeUntilChange { get; set; }
    public string TimeUntilChangeDisplay => FormatTimeSpan(TimeUntilChange);

    public DateTime GetDisplayOpen(int tzOffsetHours, int tzOffsetMinutes = 0)
    {
        var baseDate = DateTime.Now.Date;
        var utcTime = baseDate.Add(UtcOpen);
        var offset = TimeSpan.FromHours(tzOffsetHours) + TimeSpan.FromMinutes(tzOffsetMinutes);
        return utcTime.Add(offset);
    }

    public DateTime GetDisplayClose(int tzOffsetHours, int tzOffsetMinutes = 0)
    {
        var baseDate = DateTime.Now.Date;
        var utcTime = baseDate.Add(UtcClose);
        if (UtcClose < UtcOpen) utcTime = utcTime.AddDays(1);
        var offset = TimeSpan.FromHours(tzOffsetHours) + TimeSpan.FromMinutes(tzOffsetMinutes);
        return utcTime.Add(offset);
    }

    private static string FormatTimeSpan(TimeSpan ts)
    {
        if (ts.TotalHours >= 1)
            return $"{(int)ts.TotalHours}h {ts.Minutes}m";
        if (ts.TotalMinutes >= 1)
            return $"{ts.Minutes}m {ts.Seconds}s";
        return $"{ts.Seconds}s";
    }
}
