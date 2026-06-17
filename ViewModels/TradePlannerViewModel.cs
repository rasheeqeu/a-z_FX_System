using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ForexTradingWorkspace.Models.Journal;
using ForexTradingWorkspace.Models.Risk;
using ForexTradingWorkspace.Models.Screenshots;
using ForexTradingWorkspace.Models.Trading;
using ForexTradingWorkspace.Services.Journal;
using ForexTradingWorkspace.Services.Risk;
using ForexTradingWorkspace.Services.Screenshots;

namespace ForexTradingWorkspace.ViewModels;

public partial class TradePlannerViewModel(
    IRiskCalculationService riskCalculationService,
    IRuleGuardService ruleGuardService,
    IJournalRepository journalRepository,
    IScreenshotRecordRepository screenshotRepository) : ObservableObject
{
    public event Func<Task>? RiskCalculated;
    public event Func<Task>? RulesAllPassed;
    public event Func<Task>? ScreenshotAttached;
    public event Func<Task>? TradeMarkedPlaced;

    [ObservableProperty] private TradeWorkflowState workflowState = TradeWorkflowState.NoTrade;
    [ObservableProperty] private TradePlan plan = new();
    [ObservableProperty] private RiskResult? risk;
    [ObservableProperty] private bool checklistComplete;
    [ObservableProperty] private bool beforeScreenshotCaptured;
    [ObservableProperty] private string status = "Start a plan, calculate risk, complete rules, then execute manually in XM demo.";
    private string? _journalDraftId;

    public ObservableCollection<RuleCheckResult> RuleChecks { get; } = [];
    public ObservableCollection<ScreenshotRecord> Screenshots { get; } = [];

    public void StartFromLesson(string? lessonId)
    {
        if (WorkflowState == TradeWorkflowState.Completed)
        {
            ResetForNewPractice();
        }

        Plan.LinkedLessonId = lessonId;
        WorkflowState = string.IsNullOrWhiteSpace(lessonId) ? TradeWorkflowState.Planning : TradeWorkflowState.LessonSelected;
        Status = lessonId is null ? "Planning without linked lesson." : $"Planning from lesson {lessonId}.";
        OnPropertyChanged(nameof(Plan));
    }

    public void ResetForNewPractice()
    {
        Plan = new TradePlan();
        Risk = null;
        ChecklistComplete = false;
        BeforeScreenshotCaptured = false;
        WorkflowState = TradeWorkflowState.NoTrade;
        Status = "Choose a lesson, then create a fresh demo practice plan.";
        RuleChecks.Clear();
        Screenshots.Clear();
        _journalDraftId = null;
    }

    [RelayCommand]
    private void CalculateRisk()
    {
        Risk = riskCalculationService.Calculate(new RiskInput
        {
            RiskPercent = Plan.RiskPercent,
            Instrument = Plan.Pair,
            Direction = Plan.Direction,
            Entry = Plan.Entry,
            StopLoss = Plan.StopLoss,
            TakeProfit = Plan.TakeProfit
        });
        WorkflowState = TradeWorkflowState.RiskCalculated;
        CheckRulesInternal();
        Status = "Risk calculated. Complete checklist and capture before screenshot.";
        if (RiskCalculated is not null) _ = RiskCalculated.Invoke();
    }

    [RelayCommand]
    private void CheckRules() => CheckRulesInternal();

    private void CheckRulesInternal()
    {
        RuleChecks.Clear();
        foreach (var result in ruleGuardService.Check(Plan, Risk, ChecklistComplete, BeforeScreenshotCaptured))
        {
            RuleChecks.Add(result);
        }

        if (RuleChecks.Any(x => x.Severity == RuleSeverity.Blocked))
        {
            Status = "Trade is not ready. Fix blocked rules.";
            return;
        }

        WorkflowState = TradeWorkflowState.ReadyForManualExecution;
        Status = "Trade is ready for manual XM demo execution.";
        if (RulesAllPassed is not null) _ = RulesAllPassed.Invoke();
    }

    [RelayCommand]
    private void CompleteChecklist()
    {
        ChecklistComplete = true;
        WorkflowState = TradeWorkflowState.ChecklistComplete;
        CheckRulesInternal();
    }

    public async Task AttachScreenshotAsync(string filePath, ScreenshotCaptureType type)
    {
        var record = new ScreenshotRecord
        {
            LessonId = Plan.LinkedLessonId,
            TradePlanId = Plan.Id,
            CaptureType = type,
            Instrument = Plan.Pair,
            FilePath = filePath
        };

        Screenshots.Add(record);
        await screenshotRepository.AddAsync(record);
        if (type == ScreenshotCaptureType.BeforeTrade)
        {
            BeforeScreenshotCaptured = true;
            WorkflowState = TradeWorkflowState.ScreenshotCaptured;
            if (ScreenshotAttached is not null) _ = ScreenshotAttached.Invoke();
        }
        CheckRulesInternal();
    }

    [RelayCommand]
    private async Task SaveJournalDraftAsync()
    {
        _journalDraftId ??= Guid.NewGuid().ToString("N");
        var entry = new TradeJournalEntry
        {
            Id = _journalDraftId,
            Plan = Plan,
            Risk = Risk,
            Screenshots = Screenshots.ToList(),
            FollowedPlan = true,
            LessonLearned = "Draft created before manual execution."
        };

        await journalRepository.UpsertAsync(entry);
        Status = "Journal draft saved.";
    }

    [RelayCommand]
    private void MarkManuallyPlaced()
    {
        WorkflowState = TradeWorkflowState.ManuallyPlaced;
        Status = "Manual demo trade marked as placed. Record result after close.";
        if (TradeMarkedPlaced is not null) _ = TradeMarkedPlaced.Invoke();
    }
}
