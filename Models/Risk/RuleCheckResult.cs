namespace ForexTradingWorkspace.Models.Risk;

public sealed class RuleCheckResult
{
    public string RuleName { get; set; } = "";
    public RuleSeverity Severity { get; set; }
    public string Message { get; set; } = "";
}
