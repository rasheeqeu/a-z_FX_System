namespace ForexTradingWorkspace.Models;

public class SessionStatus
{
    public string Name { get; set; } = "";
    public TimeSpan UtcOpen { get; set; }
    public TimeSpan UtcClose { get; set; }
    public DateTime LocalOpen { get; set; }
    public DateTime LocalClose { get; set; }
    public bool IsCurrentlyOpen { get; set; }
    public TimeSpan TimeUntilChange { get; set; }
    public string TimeUntilChangeDisplay => FormatTimeSpan(TimeUntilChange);

    private static string FormatTimeSpan(TimeSpan ts)
    {
        if (ts.TotalHours >= 1)
            return $"{(int)ts.TotalHours}h {ts.Minutes}m";
        if (ts.TotalMinutes >= 1)
            return $"{ts.Minutes}m {ts.Seconds}s";
        return $"{ts.Seconds}s";
    }
}
