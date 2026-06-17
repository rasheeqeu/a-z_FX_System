using CommunityToolkit.Mvvm.ComponentModel;
using ForexTradingWorkspace.Models;

namespace ForexTradingWorkspace.ViewModels;

public partial class PerformanceAnalyticsViewModel : ObservableObject
{
    [ObservableProperty] private decimal winRate;
    [ObservableProperty] private decimal profitFactor;
    [ObservableProperty] private decimal drawdown;
    [ObservableProperty] private decimal averageRr;
    [ObservableProperty] private decimal netProfit;

    public void Refresh(IEnumerable<Trade> trades)
    {
        var list = trades.ToList();
        if (list.Count == 0)
        {
            WinRate = ProfitFactor = Drawdown = AverageRr = NetProfit = 0;
            return;
        }

        var wins = list.Where(t => t.ProfitLoss > 0).ToList();
        var losses = list.Where(t => t.ProfitLoss < 0).ToList();
        WinRate = Math.Round((decimal)wins.Count / list.Count * 100m, 2);
        var grossWin = wins.Sum(t => t.ProfitLoss);
        var grossLoss = Math.Abs(losses.Sum(t => t.ProfitLoss));
        ProfitFactor = grossLoss == 0 ? grossWin : Math.Round(grossWin / grossLoss, 2);
        NetProfit = list.Sum(t => t.ProfitLoss);
        AverageRr = Math.Round(list.Where(t => t.Risk > 0).Select(t => t.ProfitLoss / t.Risk).DefaultIfEmpty().Average(), 2);
        Drawdown = CalculateDrawdown(list.OrderBy(t => t.OpenedAt));
    }

    private static decimal CalculateDrawdown(IEnumerable<Trade> trades)
    {
        decimal equity = 0;
        decimal peak = 0;
        decimal maxDrawdown = 0;
        foreach (var trade in trades)
        {
            equity += trade.ProfitLoss;
            peak = Math.Max(peak, equity);
            maxDrawdown = Math.Min(maxDrawdown, equity - peak);
        }
        return Math.Abs(maxDrawdown);
    }
}
