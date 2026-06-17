namespace ForexTradingWorkspace.Models.Learning;

public sealed class PracticeTask
{
    public string Id { get; set; } = "";
    public string Prompt { get; set; } = "";
    public string ExpectedAction { get; set; } = "";
}
