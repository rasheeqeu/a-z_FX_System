using ForexTradingWorkspace.Models.Risk;

namespace ForexTradingWorkspace.Services.Risk;

public sealed class RiskCalculationService : IRiskCalculationService
{
    public RiskResult Calculate(RiskInput input)
    {
        var pipSize = input.PipSize <= 0 ? 0.0001m : input.PipSize;
        var pipValue = input.PipValuePerLot <= 0 ? 10m : input.PipValuePerLot;
        var riskPercent = Math.Max(0, input.RiskPercent);
        var stopPips = Math.Abs(input.Entry - input.StopLoss) / pipSize;
        var rewardPips = Math.Abs(input.TakeProfit - input.Entry) / pipSize;
        var moneyRisk = input.AccountBalance * riskPercent / 100m;
        var rawLot = stopPips <= 0 ? 0 : moneyRisk / (stopPips * pipValue);
        var lot = RoundDown(Math.Max(input.MinimumLot, rawLot), input.LotStep);
        var potentialReward = rewardPips * pipValue * lot;
        var rr = moneyRisk <= 0 ? 0 : potentialReward / moneyRisk;
        var dailyLimit = input.AccountBalance * input.DailyRiskLimitPercent / 100m;
        var dailyRemaining = dailyLimit - moneyRisk;

        return new RiskResult
        {
            PipDistance = Math.Round(stopPips, 1),
            RewardPips = Math.Round(rewardPips, 1),
            MoneyRisk = Math.Round(moneyRisk, 2),
            LotSize = Math.Round(lot, 2),
            PotentialReward = Math.Round(potentialReward, 2),
            RewardRiskRatio = Math.Round(rr, 2),
            DailyRiskRemaining = Math.Round(dailyRemaining, 2),
            BeginnerExplanation = $"If this trade hits stop loss, you risk about {moneyRisk:C}. If it reaches take profit, the estimated reward is {potentialReward:C}."
        };
    }

    private static decimal RoundDown(decimal value, decimal step)
    {
        if (step <= 0) return value;
        return Math.Floor(value / step) * step;
    }
}
