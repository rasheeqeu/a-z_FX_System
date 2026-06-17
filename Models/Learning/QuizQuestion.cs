namespace ForexTradingWorkspace.Models.Learning;

public sealed class QuizQuestion
{
    public string Prompt { get; set; } = "";
    public List<string> Options { get; set; } = [];
    public int CorrectOptionIndex { get; set; }
}
