using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ForexTradingWorkspace.Models;
using ForexTradingWorkspace.Models.Export;
using ForexTradingWorkspace.Models.Learning;
using ForexTradingWorkspace.Models.Shell;
using ForexTradingWorkspace.Services;
using ForexTradingWorkspace.Services.Export;
using ForexTradingWorkspace.Views;

namespace ForexTradingWorkspace.ViewModels;

public partial class ShellViewModel(
    LearningViewModel learning,
    BrokerPortalViewModel broker,
    TradePlannerViewModel planner,
    JournalViewModel journal,
    ReviewViewModel review,
    SettingsViewModel settings,
    IAiReviewExportService aiReviewExportService,
    Func<LessonEditorWindow> lessonEditorFactory,
    SessionClockService sessionClockService) : ObservableObject
{
    [ObservableProperty] private NavigationState activeSection = NavigationState.Learn;
    [ObservableProperty] private LayoutState layoutState = LayoutState.Normal;
    [ObservableProperty] private GuidedStep currentStep = GuidedStep.ChooseLesson;
    [ObservableProperty] private string localClock = "";
    [ObservableProperty] private string status = "Ready";
    [ObservableProperty] private string aiReviewQuestion = "Review this demo trade plan. What am I missing?";
    [ObservableProperty] private bool isSessionPanelOpen = false;
    [ObservableProperty] private ObservableCollection<SessionStatus> sessionsData = new();
    [ObservableProperty] private string selectedTimezone = "Local";
    [ObservableProperty] private ObservableCollection<string> availableTimezones = new();

    private readonly System.Windows.Threading.DispatcherTimer _clockTimer = new() { Interval = TimeSpan.FromSeconds(1) };

    public LearningViewModel Learning { get; } = learning;
    public BrokerPortalViewModel Broker { get; } = broker;
    public TradePlannerViewModel Planner { get; } = planner;
    public JournalViewModel Journal { get; } = journal;
    public ReviewViewModel Review { get; } = review;
    public SettingsViewModel Settings { get; } = settings;

    public string ActiveSectionTitle => ActiveSection.ToString();
    public int CurrentStepNumber => (int)CurrentStep + 1;
    public string ProgressText => $"{CurrentStepNumber} of 8";
    public string NextButtonText => CurrentStep == GuidedStep.JournalReview ? "Start Next Lesson" : "Next";
    public string CurrentStepTitle => CurrentStep switch
    {
        GuidedStep.ChooseLesson => "Step 1: Choose Lesson",
        GuidedStep.EvaluateUnderstanding => "Step 2: Evaluate Understanding",
        GuidedStep.PlanSetup => "Step 3: Trade Idea",
        GuidedStep.EnterPrices => "Step 4: Entry, Stop, Target",
        GuidedStep.CheckRisk => "Step 5: Risk Check",
        GuidedStep.CaptureScreenshot => "Step 6: Screenshot",
        GuidedStep.ManualExecution => "Step 7: Place Manually",
        GuidedStep.JournalReview => "Step 8: Journal Review",
        _ => "Guided Practice"
    };

    public string CurrentStepHelp => CurrentStep switch
    {
        GuidedStep.ChooseLesson => "Pick one concept to practice. Do not trade randomly.",
        GuidedStep.EvaluateUnderstanding => "Prove you understand the concept before creating a demo trade idea.",
        GuidedStep.PlanSetup => "Write the market idea in simple words before looking at lot size.",
        GuidedStep.EnterPrices => "Enter entry, stop loss, take profit, and risk percent.",
        GuidedStep.CheckRisk => "Calculate risk and fix any blocked rule before continuing.",
        GuidedStep.CaptureScreenshot => "Capture what you saw before manual XM demo execution.",
        GuidedStep.ManualExecution => "Place the trade manually in XM demo using the planned values.",
        GuidedStep.JournalReview => "This is the end of this practice cycle. Save what happened, then start the next lesson.",
        _ => ""
    };
    public string AssistantTitle => ActiveSection switch
    {
        NavigationState.Learn => "Learning assistant",
        NavigationState.Broker => "XM demo assistant",
        NavigationState.Plan => "Trade assistant",
        NavigationState.Journal => "Review assistant",
        NavigationState.Review => "Improvement assistant",
        _ => "Settings"
    };

    public async Task InitializeAsync()
    {
        await Learning.InitializeAsync();
        await Journal.InitializeAsync();
        await RefreshReviewAsync();

        // Initialize timezones
        AvailableTimezones.Add("Local");
        AvailableTimezones.Add("Dubai (GMT +4)");
        AvailableTimezones.Add("Kolkata (GMT +5:30)");
        AvailableTimezones.Add("London (GMT +0)");
        AvailableTimezones.Add("New York (GMT -5)");

        UpdateClock();
        _clockTimer.Tick += (_, _) => UpdateClock();
        _clockTimer.Start();

        // Auto-advance hooks
        Learning.UnderstandingSaved += async () =>
        {
            if (CurrentStep == GuidedStep.ChooseLesson)
                await GoToStep(GuidedStep.EvaluateUnderstanding);
        };
        Learning.EvaluationPassed += async () =>
        {
            if (CurrentStep == GuidedStep.EvaluateUnderstanding)
            {
                Planner.StartFromLesson(Learning.SelectedLesson?.Id);
                await GoToStep(GuidedStep.PlanSetup);
            }
        };
        Planner.RiskCalculated += async () =>
        {
            if (CurrentStep == GuidedStep.EnterPrices || CurrentStep == GuidedStep.PlanSetup)
                await GoToStep(GuidedStep.CheckRisk);
        };
        Planner.RulesAllPassed += async () =>
        {
            if (CurrentStep == GuidedStep.CheckRisk)
                await GoToStep(GuidedStep.CaptureScreenshot);
        };
        Planner.ScreenshotAttached += async () =>
        {
            if (CurrentStep == GuidedStep.CaptureScreenshot)
                await GoToStep(GuidedStep.ManualExecution);
        };
        Planner.TradeMarkedPlaced += async () =>
        {
            if (CurrentStep == GuidedStep.ManualExecution)
                await GoToStep(GuidedStep.JournalReview);
        };
        Journal.EntrySaved += async () =>
        {
            if (CurrentStep == GuidedStep.JournalReview)
            {
                await RefreshReviewAsync();
                Status = "Entry saved. Review your stats, then start the next lesson.";
            }
        };
    }

    [RelayCommand]
    private async Task Navigate(NavigationState section)
    {
        ActiveSection = section;
        OnPropertyChanged(nameof(ActiveSectionTitle));
        OnPropertyChanged(nameof(AssistantTitle));
        if (section == NavigationState.Review)
        {
            await RefreshReviewAsync();
        }
        else if (section == NavigationState.Journal)
        {
            await Journal.InitializeAsync();
        }
    }

    [RelayCommand]
    private async Task GoToStep(GuidedStep step)
    {
        CurrentStep = step;
        OnPropertyChanged(nameof(CurrentStepNumber));
        OnPropertyChanged(nameof(ProgressText));
        OnPropertyChanged(nameof(NextButtonText));
        OnPropertyChanged(nameof(CurrentStepTitle));
        OnPropertyChanged(nameof(CurrentStepHelp));

        ActiveSection = step switch
        {
            GuidedStep.ChooseLesson => NavigationState.Learn,
            GuidedStep.EvaluateUnderstanding => NavigationState.Learn,
            GuidedStep.ManualExecution => NavigationState.Broker,
            GuidedStep.JournalReview => NavigationState.Journal,
            _ => NavigationState.Plan
        };

        OnPropertyChanged(nameof(ActiveSectionTitle));
        OnPropertyChanged(nameof(AssistantTitle));

        if (ActiveSection == NavigationState.Journal)
        {
            await Journal.InitializeAsync();
        }
    }

    [RelayCommand]
    private async Task NextStep()
    {
        if (CurrentStep == GuidedStep.JournalReview)
        {
            Learning.NextLessonCommand.Execute(null);
            Planner.ResetForNewPractice();
            await GoToStep(GuidedStep.ChooseLesson);
            Status = "Ready for the next lesson.";
            return;
        }

        if (CurrentStep == GuidedStep.ChooseLesson && !Learning.CanEvaluateSelectedLesson)
        {
            Status = Learning.LearningGateMessage;
            return;
        }

        if (CurrentStep == GuidedStep.EvaluateUnderstanding && !Learning.CanApplySelectedLesson)
        {
            Status = "Pass the understanding evaluation before creating a trade idea.";
            return;
        }

        var next = CurrentStep == GuidedStep.JournalReview
            ? GuidedStep.JournalReview
            : CurrentStep + 1;

        if (next == GuidedStep.PlanSetup)
        {
            Planner.StartFromLesson(Learning.SelectedLesson?.Id);
        }
        else if (next == GuidedStep.CheckRisk)
        {
            Planner.CalculateRiskCommand.Execute(null);
        }

        await GoToStep(next);
    }

    [RelayCommand]
    private async Task PreviousStep()
    {
        var previous = CurrentStep == GuidedStep.ChooseLesson
            ? GuidedStep.ChooseLesson
            : CurrentStep - 1;

        await GoToStep(previous);
    }

    [RelayCommand]
    private async Task ContinueLearning()
    {
        await GoToStep(GuidedStep.ChooseLesson);
        if (Learning.SelectedLesson is not null)
        {
            await Learning.SetLessonStateCommand.ExecuteAsync(LessonState.Studying);
        }
    }

    [RelayCommand]
    private async Task OpenBroker()
    {
        await Navigate(NavigationState.Broker);
    }

    [RelayCommand]
    private async Task PlanTrade()
    {
        Planner.StartFromLesson(Learning.SelectedLesson?.Id);
        await GoToStep(GuidedStep.PlanSetup);
    }

    [RelayCommand]
    private void OpenLessonEditor()
    {
        var window = lessonEditorFactory();
        window.Show();
    }

    [RelayCommand]
    private void ToggleAssistant()
    {
        LayoutState = LayoutState == LayoutState.AssistantCollapsed ? LayoutState.Normal : LayoutState.AssistantCollapsed;
    }

    [RelayCommand]
    private void ToggleSessionPanel()
    {
        IsSessionPanelOpen = !IsSessionPanelOpen;
    }

    [RelayCommand]
    private async Task ExportAiReviewAsync()
    {
        if (Planner.RuleChecks.Count == 0)
        {
            Planner.CheckRulesCommand.Execute(null);
        }

        var package = new AiReviewPackage
        {
            CurrentLessonId = Learning.SelectedLesson?.Id,
            CurrentLessonTitle = Learning.SelectedLesson?.Title,
            Plan = Planner.Plan,
            Risk = Planner.Risk,
            RuleState = Planner.RuleChecks.ToList(),
            Screenshots = Planner.Screenshots.ToList(),
            JournalNotes = Journal.SelectedEntry?.ReviewNotes ?? Journal.SelectedEntry?.LessonLearned ?? "",
            MistakeTags = Journal.SelectedEntry?.MistakeTags ?? [],
            UserQuestion = AiReviewQuestion
        };

        await aiReviewExportService.ExportToClipboardAsync(package);
        var file = await aiReviewExportService.SaveToFileAsync(package);
        Status = $"Copied for AI. Saved copy: {file}";
    }

    private async Task RefreshReviewAsync()
    {
        await Review.RefreshAsync(Learning.Sections.SelectMany(x => x.Lessons));
    }

    private void UpdateClock()
    {
        LocalClock = DateTime.Now.ToString("ddd dd MMM HH:mm:ss");
        UpdateSessionStatus();
    }

    private void UpdateSessionStatus()
    {
        var utcNow = DateTime.UtcNow;
        var sessionsStatus = sessionClockService.GetAllSessionsStatus(utcNow);
        var localTimes = sessionClockService.GetAllSessionsLocalTimes();

        // Build updated sessions list
        var updated = new List<SessionStatus>();
        for (int i = 0; i < sessionClockService.Sessions.Count; i++)
        {
            var (name, openUtc, closeUtc, isOpen, timeUntil) = sessionsStatus[i];
            var (_, localOpen, localClose) = localTimes[i];

            updated.Add(new SessionStatus
            {
                Name = name,
                UtcOpen = openUtc,
                UtcClose = closeUtc,
                LocalOpen = localOpen,
                LocalClose = localClose,
                IsCurrentlyOpen = isOpen,
                TimeUntilChange = timeUntil
            });
        }

        // Update collection
        if (SessionsData.Count == 0)
        {
            foreach (var session in updated)
            {
                SessionsData.Add(session);
            }
        }
        else
        {
            for (int i = 0; i < updated.Count; i++)
            {
                SessionsData[i] = updated[i];
            }
        }
    }
}
