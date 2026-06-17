namespace ForexTradingWorkspace.Models;

public class JournalScreenState : ScreenState
{
    public Dictionary<string, string>? FilterCriteria { get; set; } = new();
    public string? SortColumn { get; set; }
    public long? SelectedTradeId { get; set; }

    public override bool IsValid => true;
}
