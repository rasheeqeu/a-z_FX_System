namespace ForexTradingWorkspace.Models;

public class MarketData
{
    public string Symbol { get; set; } = "";
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public decimal Bid { get; set; }
    public decimal Ask { get; set; }
    public long Volume { get; set; }
    public decimal Change { get; set; }
    public decimal ChangePercent { get; set; }
    public DateTime LastUpdate { get; set; } = DateTime.Now;
}
