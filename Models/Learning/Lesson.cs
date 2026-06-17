namespace ForexTradingWorkspace.Models.Learning;

public sealed class Lesson
{
    public string Id { get; set; } = "";
    public int SectionNumber { get; set; }
    public string SectionTitle { get; set; } = "";
    public string Title { get; set; } = "";
    public string Duration { get; set; } = "";
    public string Summary { get; set; } = "";
    public List<string> KeyTerms { get; set; } = [];
    public List<string> CommonMistakes { get; set; } = [];
    public List<PracticeTask> PracticeTasks { get; set; } = [];
    public string DemoApplication { get; set; } = "";
    public List<string> Checklist { get; set; } = [];
}
