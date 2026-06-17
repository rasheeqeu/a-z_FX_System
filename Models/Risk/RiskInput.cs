namespace ForexTradingWorkspace.Models.Risk;

public sealed class RiskInput
{
    public decimal AccountBalance { get; set; } = 10000m;
    public decimal RiskPercent { get; set; } = 1m;
    public string Instrument { get; set; } = "EURUSD";
    public string Direction { get; set; } = "Buy";
    public decimal Entry { get; set; } = 1.1000m;
    public decimal StopLoss { get; set; } = 1.0980m;
    public decimal TakeProfit { get; set; } = 1.1040m;
    public decimal PipSize { get; set; } = 0.0001m;
    public decimal PipValuePerLot { get; set; } = 10m;
    public decimal LotStep { get; set; } = 0.01m;
    public decimal MinimumLot { get; set; } = 0.01m;
    public decimal DailyRiskLimitPercent { get; set; } = 2m;
}
