namespace ForexTradingWorkspace.Models.Learning;

public sealed class PracticeAttempt
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string LessonId { get; set; } = "";
    public string TaskId { get; set; } = "";
    public string Answer { get; set; } = "";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
