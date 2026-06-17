namespace ForexTradingWorkspace.Models;

public sealed class Trade
{
    public long Id { get; set; }
    public DateTime OpenedAt { get; set; } = DateTime.Now;
    public string Pair { get; set; } = "EURUSD";
    public string Direction { get; set; } = "Long";
    public decimal Entry { get; set; }
    public decimal StopLoss { get; set; }
    public decimal TakeProfit { get; set; }
    public decimal Risk { get; set; }
    public decimal ProfitLoss { get; set; }
    public string Notes { get; set; } = "";
    public string BeforeScreenshotPath { get; set; } = "";
    public string AfterScreenshotPath { get; set; } = "";
}
