using System.Collections.ObjectModel;

namespace ForexTradingWorkspace.Services;

public sealed class SessionClockService
{
    public IReadOnlyList<(string Name, TimeSpan OpenUtc, TimeSpan CloseUtc)> Sessions { get; } =
    [
        ("Sydney", new TimeSpan(22, 0, 0), new TimeSpan(7, 0, 0)),
        ("Tokyo", new TimeSpan(0, 0, 0), new TimeSpan(9, 0, 0)),
        ("London", new TimeSpan(8, 0, 0), new TimeSpan(17, 0, 0)),
        ("New York", new TimeSpan(13, 0, 0), new TimeSpan(22, 0, 0))
    ];

    public (bool IsOpen, TimeSpan UntilChange) GetStatus(TimeSpan openUtc, TimeSpan closeUtc, DateTime utcNow)
    {
        var now = utcNow.TimeOfDay;
        var crossesMidnight = closeUtc <= openUtc;
        var isOpen = crossesMidnight
            ? now >= openUtc || now < closeUtc
            : now >= openUtc && now < closeUtc;

        var target = isOpen ? closeUtc : openUtc;
        var until = target - now;
        if (until < TimeSpan.Zero) until += TimeSpan.FromDays(1);
        return (isOpen, until);
    }

    public List<(string Name, TimeSpan OpenUtc, TimeSpan CloseUtc, bool IsOpen, TimeSpan TimeUntilChange)> GetAllSessionsStatus(DateTime utcNow)
    {
        var result = new List<(string, TimeSpan, TimeSpan, bool, TimeSpan)>();
        foreach (var (name, openUtc, closeUtc) in Sessions)
        {
            var (isOpen, timeUntil) = GetStatus(openUtc, closeUtc, utcNow);
            result.Add((name, openUtc, closeUtc, isOpen, timeUntil));
        }
        return result;
    }

    public List<(string SessionName, DateTime LocalOpen, DateTime LocalClose)> GetAllSessionsLocalTimes()
    {
        var localOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);
        var result = new List<(string, DateTime, DateTime)>();

        foreach (var (name, openUtc, closeUtc) in Sessions)
        {
            var baseDate = DateTime.Now.Date;
            var openDateTime = baseDate.Add(openUtc);
            var closeDateTime = baseDate.Add(closeUtc);

            // Handle midnight-crossing sessions
            if (closeUtc < openUtc)
            {
                closeDateTime = closeDateTime.AddDays(1);
            }

            var localOpen = openDateTime.Add(localOffset);
            var localClose = closeDateTime.Add(localOffset);

            result.Add((name, localOpen, localClose));
        }
        return result;
    }

    public List<(string Session1, string Session2, TimeSpan OverlapStartUtc, TimeSpan OverlapEndUtc)> GetOverlappingSessions()
    {
        var overlaps = new List<(string, string, TimeSpan, TimeSpan)>();

        // London (08:00-17:00) and New York (13:00-22:00) overlap from 13:00-17:00
        overlaps.Add(("London", "New York", new TimeSpan(13, 0, 0), new TimeSpan(17, 0, 0)));

        return overlaps;
    }

    public bool IsOverlapActive(DateTime utcNow, string session1, string session2)
    {
        var overlaps = GetOverlappingSessions();
        var overlap = overlaps.FirstOrDefault(o =>
            (o.Session1 == session1 && o.Session2 == session2) ||
            (o.Session1 == session2 && o.Session2 == session1));

        if (overlap == default) return false;

        var now = utcNow.TimeOfDay;
        return now >= overlap.OverlapStartUtc && now < overlap.OverlapEndUtc;
    }
}
