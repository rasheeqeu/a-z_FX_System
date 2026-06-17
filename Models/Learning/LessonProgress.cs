namespace ForexTradingWorkspace.Models.Learning;

public sealed class LessonProgress
{
    public string LessonId { get; set; } = "";
    public LessonState State { get; set; } = LessonState.NotStarted;
    public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;
}
