namespace ForexTradingWorkspace.Models.Risk;

public sealed class RiskResult
{
    public decimal PipDistance { get; set; }
    public decimal RewardPips { get; set; }
    public decimal MoneyRisk { get; set; }
    public decimal LotSize { get; set; }
    public decimal PotentialReward { get; set; }
    public decimal RewardRiskRatio { get; set; }
    public decimal DailyRiskRemaining { get; set; }
    public string BeginnerExplanation { get; set; } = "";
}
