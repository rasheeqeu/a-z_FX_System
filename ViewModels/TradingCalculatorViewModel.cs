using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Text;
using System.Windows;

namespace ForexTradingWorkspace.ViewModels;

public partial class TradingCalculatorViewModel : ObservableObject
{
    private bool _isCalculating;

    public string[] Directions { get; } = ["Buy", "Sell"];
    public string[] Assets { get; } = ["Forex", "Gold (XAUUSD)", "Silver", "Indices", "Crypto", "Stocks"];
    public string[] AccountCurrencies { get; } = ["USD", "EUR", "GBP", "AED", "INR"];

    [ObservableProperty] private string direction = "Buy";
    [ObservableProperty] private string assetClass = "Forex";
    [ObservableProperty] private string accountCurrency = "USD";
    [ObservableProperty] private decimal accountBalance = 10000;
    [ObservableProperty] private decimal peakBalance = 10000;
    [ObservableProperty] private decimal riskPercent = 1;
    [ObservableProperty] private decimal fixedRiskAmount;
    [ObservableProperty] private bool useFixedRisk;
    [ObservableProperty] private decimal stopLossPips = 20;
    [ObservableProperty] private decimal desiredRr = 2;
    [ObservableProperty] private decimal pipValuePerLot = 10;
    [ObservableProperty] private decimal tickSize = 0.0001m;
    [ObservableProperty] private decimal entryPrice = 1.1000m;
    [ObservableProperty] private decimal stopLossPrice = 1.0980m;
    [ObservableProperty] private decimal takeProfitPrice = 1.1040m;
    [ObservableProperty] private decimal spreadPips = 1.5m;
    [ObservableProperty] private decimal commissionPerLot = 7;
    [ObservableProperty] private decimal leverage = 30;
    [ObservableProperty] private decimal contractSize = 100000;
    [ObservableProperty] private decimal minLot = 0.01m;
    [ObservableProperty] private decimal lotStep = 0.01m;
    [ObservableProperty] private decimal dailyMaxLossPercent = 5;
    [ObservableProperty] private decimal maxDrawdownPercent = 10;
    [ObservableProperty] private decimal partialClosePercent = 50;
    [ObservableProperty] private decimal partialCloseRr = 1;
    [ObservableProperty] private decimal breakevenPips = 10;

    [ObservableProperty] private decimal lotSize;
    [ObservableProperty] private decimal tradableLotSize;
    [ObservableProperty] private decimal moneyRisk;
    [ObservableProperty] private decimal potentialProfit;
    [ObservableProperty] private decimal potentialLoss;
    [ObservableProperty] private decimal grossProfit;
    [ObservableProperty] private decimal commission;
    [ObservableProperty] private decimal netProfit;
    [ObservableProperty] private decimal rewardRiskRatio;
    [ObservableProperty] private decimal marginRequired;
    [ObservableProperty] private decimal freeMarginAfterTrade;
    [ObservableProperty] private decimal marginLevelPercent;
    [ObservableProperty] private decimal maxDailyRisk;
    [ObservableProperty] private decimal remainingDailyLoss;
    [ObservableProperty] private decimal remainingTotalLoss;
    [ObservableProperty] private decimal drawdownPercent;
    [ObservableProperty] private decimal partialCloseProfit;
    [ObservableProperty] private string validationMessage = "Ready";
    [ObservableProperty] private string tradeSummary = "";
    [ObservableProperty] private string priceDiagram = "";

    public TradingCalculatorViewModel()
    {
        ApplyAssetPreset();
        Calculate();
    }

    partial void OnDirectionChanged(string value) => Calculate();
    partial void OnAccountBalanceChanged(decimal value) => Calculate();
    partial void OnRiskPercentChanged(decimal value) => Calculate();
    partial void OnFixedRiskAmountChanged(decimal value) => Calculate();
    partial void OnUseFixedRiskChanged(bool value) => Calculate();
    partial void OnStopLossPipsChanged(decimal value) => Calculate();
    partial void OnPipValuePerLotChanged(decimal value) => Calculate();
    partial void OnTickSizeChanged(decimal value) => Calculate();
    partial void OnEntryPriceChanged(decimal value) => Calculate();
    partial void OnStopLossPriceChanged(decimal value) => Calculate();
    partial void OnTakeProfitPriceChanged(decimal value) => Calculate();
    partial void OnSpreadPipsChanged(decimal value) => Calculate();
    partial void OnCommissionPerLotChanged(decimal value) => Calculate();
    partial void OnLeverageChanged(decimal value) => Calculate();
    partial void OnContractSizeChanged(decimal value) => Calculate();
    partial void OnMinLotChanged(decimal value) => Calculate();
    partial void OnLotStepChanged(decimal value) => Calculate();
    partial void OnDailyMaxLossPercentChanged(decimal value) => Calculate();
    partial void OnMaxDrawdownPercentChanged(decimal value) => Calculate();
    partial void OnPartialClosePercentChanged(decimal value) => Calculate();
    partial void OnPartialCloseRrChanged(decimal value) => Calculate();
    partial void OnPeakBalanceChanged(decimal value) => Calculate();

    partial void OnAssetClassChanged(string value)
    {
        ApplyAssetPreset();
        Calculate();
    }

    [RelayCommand]
    public void CalculateTpFromRr()
    {
        var riskDistance = Math.Abs(EntryPrice - StopLossPrice);
        if (riskDistance <= 0 || DesiredRr <= 0) return;
        TakeProfitPrice = Direction == "Buy"
            ? EntryPrice + riskDistance * DesiredRr
            : EntryPrice - riskDistance * DesiredRr;
        Calculate();
    }

    [RelayCommand]
    public void CopySummary()
    {
        Clipboard.SetText(TradeSummary);
    }

    [RelayCommand]
    public void Calculate()
    {
        if (_isCalculating) return;

        try
        {
            _isCalculating = true;
            if (TickSize <= 0) TickSize = 0.0001m;
            StopLossPips = Math.Abs(EntryPrice - StopLossPrice) / TickSize;
            var rewardPips = Math.Abs(TakeProfitPrice - EntryPrice) / TickSize;
            RewardRiskRatio = StopLossPips <= 0 ? 0 : Math.Round(rewardPips / StopLossPips, 2);

            MoneyRisk = UseFixedRisk && FixedRiskAmount > 0
                ? FixedRiskAmount
                : AccountBalance * RiskPercent / 100m;

            var effectiveRiskPips = StopLossPips + Math.Max(0, SpreadPips);
            LotSize = effectiveRiskPips <= 0 || PipValuePerLot <= 0 ? 0 : MoneyRisk / (effectiveRiskPips * PipValuePerLot);
            TradableLotSize = RoundLot(LotSize);

            PotentialLoss = effectiveRiskPips * PipValuePerLot * TradableLotSize;
            GrossProfit = rewardPips * PipValuePerLot * TradableLotSize;
            Commission = CommissionPerLot * TradableLotSize;
            PotentialProfit = GrossProfit - Commission;
            NetProfit = PotentialProfit;

            MarginRequired = Leverage <= 0 ? 0 : ContractSize * TradableLotSize * EntryPrice / Leverage;
            FreeMarginAfterTrade = AccountBalance - MarginRequired;
            MarginLevelPercent = MarginRequired <= 0 ? 0 : AccountBalance / MarginRequired * 100m;

            MaxDailyRisk = AccountBalance * DailyMaxLossPercent / 100m;
            RemainingDailyLoss = MaxDailyRisk - PotentialLoss;
            var maxTotalLoss = PeakBalance * MaxDrawdownPercent / 100m;
            DrawdownPercent = PeakBalance <= 0 ? 0 : Math.Max(0, (PeakBalance - AccountBalance) / PeakBalance * 100m);
            RemainingTotalLoss = maxTotalLoss - Math.Max(0, PeakBalance - AccountBalance);

            PartialCloseProfit = GrossProfit * (PartialClosePercent / 100m) * (PartialCloseRr / Math.Max(RewardRiskRatio, 0.01m));
            ValidatePrices();
            BuildSummary();
        }
        finally
        {
            _isCalculating = false;
        }
    }

    private void ApplyAssetPreset()
    {
        switch (AssetClass)
        {
            case "Gold (XAUUSD)":
                PipValuePerLot = 10; ContractSize = 100; TickSize = 0.01m; Leverage = 20;
                break;
            case "Silver":
                PipValuePerLot = 50; ContractSize = 5000; TickSize = 0.001m; Leverage = 20;
                break;
            case "Indices":
                PipValuePerLot = 1; ContractSize = 1; TickSize = 1; Leverage = 20;
                break;
            case "Crypto":
                PipValuePerLot = 1; ContractSize = 1; TickSize = 1; Leverage = 5;
                break;
            case "Stocks":
                PipValuePerLot = 1; ContractSize = 1; TickSize = 0.01m; Leverage = 5;
                break;
            default:
                PipValuePerLot = 10; ContractSize = 100000; TickSize = 0.0001m; Leverage = 30;
                break;
        }
    }

    private decimal RoundLot(decimal value)
    {
        if (LotStep <= 0) return value;
        var rounded = Math.Floor(value / LotStep) * LotStep;
        return Math.Max(MinLot, rounded);
    }

    private void ValidatePrices()
    {
        var errors = new List<string>();
        if (Direction == "Buy")
        {
            if (EntryPrice <= StopLossPrice) errors.Add("Buy requires Entry > SL");
            if (TakeProfitPrice <= EntryPrice) errors.Add("Buy requires TP > Entry");
        }
        else
        {
            if (EntryPrice >= StopLossPrice) errors.Add("Sell requires Entry < SL");
            if (TakeProfitPrice >= EntryPrice) errors.Add("Sell requires TP < Entry");
        }

        ValidationMessage = errors.Count == 0 ? "Valid setup" : string.Join("; ", errors);
    }

    private void BuildSummary()
    {
        var side = Direction.ToUpperInvariant();
        TradeSummary = $"""
            {side} {AssetClass}
            Account: {AccountBalance:N2} {AccountCurrency}
            Entry: {EntryPrice}
            SL: {StopLossPrice}
            TP: {TakeProfitPrice}
            Risk: {PotentialLoss:N2}
            Reward: {PotentialProfit:N2}
            RR: 1:{RewardRiskRatio:N2}
            Lot Size: {TradableLotSize:N2}
            Margin: {MarginRequired:N2}
            Validation: {ValidationMessage}
            """;

        PriceDiagram = Direction == "Buy"
            ? $"TP {TakeProfitPrice}\n|\n|\nENTRY {EntryPrice}\n|\n|\nSL {StopLossPrice}"
            : $"SL {StopLossPrice}\n|\n|\nENTRY {EntryPrice}\n|\n|\nTP {TakeProfitPrice}";
    }
}
