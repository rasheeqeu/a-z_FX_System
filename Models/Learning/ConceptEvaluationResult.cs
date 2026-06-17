namespace ForexTradingWorkspace.Models.Learning;

public sealed class ConceptEvaluationResult
{
    public bool Passed { get; set; }
    public int Score { get; set; }
    public List<string> Missing { get; set; } = [];
    public string Feedback { get; set; } = "";
    public string NextAction { get; set; } = "";
    public bool UsedAi { get; set; }
}
