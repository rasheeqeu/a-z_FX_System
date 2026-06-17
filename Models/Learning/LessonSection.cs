namespace ForexTradingWorkspace.Models.Learning;

public sealed class LessonSection
{
    public int SectionNumber { get; set; }
    public string Title { get; set; } = "";
    public List<Lesson> Lessons { get; set; } = [];
}
