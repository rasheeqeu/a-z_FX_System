using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ForexTradingWorkspace.Models.Learning;
using ForexTradingWorkspace.Services.Learning;

namespace ForexTradingWorkspace.ViewModels;

public partial class LearningViewModel(
    ILessonRepository lessonRepository,
    IConceptEvaluationService conceptEvaluationService,
    SettingsViewModel settings) : ObservableObject
{
    public event Func<Task>? UnderstandingSaved;
    public event Func<Task>? EvaluationPassed;

    [ObservableProperty] private LessonItemViewModel? selectedLesson;
    [ObservableProperty] private string practiceAnswer = "";
    [ObservableProperty] private string evaluationAnswer = "";
    [ObservableProperty] private string evaluationPrompt = "";
    [ObservableProperty] private string evaluationJson = "";
    [ObservableProperty] private string evaluationFeedback = "After saving your understanding, explain the concept again with an example before demo practice.";
    [ObservableProperty] private int evaluationScore;
    [ObservableProperty] private string status = "Choose a lesson to begin.";
    [ObservableProperty] private string learningGateMessage = "Write and save your understanding before practicing in demo.";

    public ObservableCollection<LessonSectionViewModel> Sections { get; } = [];
    public List<LessonItemViewModel> AllLessons => Sections.SelectMany(x => x.Lessons).ToList();
    public string LessonPosition
    {
        get
        {
            var lessons = AllLessons;
            if (SelectedLesson is null || lessons.Count == 0) return "No lesson selected";
            var index = lessons.IndexOf(SelectedLesson);
            return index < 0 ? "Lesson" : $"Lesson {index + 1} of {lessons.Count}";
        }
    }

    public bool CanEvaluateSelectedLesson => SelectedLesson?.State >= LessonState.Practicing;
    public bool CanApplySelectedLesson => SelectedLesson?.State >= LessonState.DemoApplied;

    public async Task InitializeAsync()
    {
        var sections = await lessonRepository.LoadSectionsAsync();
        var progress = await lessonRepository.LoadProgressAsync();
        Sections.Clear();
        foreach (var section in sections)
        {
            Sections.Add(new LessonSectionViewModel(section, progress));
        }

        SelectedLesson ??= Sections.SelectMany(x => x.Lessons).FirstOrDefault();
        OnPropertyChanged(nameof(AllLessons));
        OnPropertyChanged(nameof(LessonPosition));
    }

    partial void OnSelectedLessonChanged(LessonItemViewModel? value)
    {
        OnPropertyChanged(nameof(LessonPosition));
        EvaluationAnswer = "";
        EvaluationPrompt = "";
        EvaluationJson = "";
        EvaluationFeedback = "After saving your understanding, explain the concept again with an example before demo practice.";
        RefreshLearningGate();
        foreach (var lesson in AllLessons)
            lesson.IsSelected = lesson == value;
    }

    partial void OnPracticeAnswerChanged(string value)
    {
        RefreshLearningGate();
    }

    partial void OnEvaluationAnswerChanged(string value)
    {
        RefreshEvaluationPrompt();
        RefreshLearningGate();
    }

    [RelayCommand]
    private void SelectLesson(LessonItemViewModel lesson)
    {
        SelectedLesson = lesson;
        Status = lesson.DisplayTitle;
        RefreshLearningGate();
    }

    [RelayCommand]
    private void PreviousLesson()
    {
        MoveLesson(-1);
    }

    [RelayCommand]
    private void NextLesson()
    {
        MoveLesson(1);
    }

    [RelayCommand]
    private async Task SetLessonState(LessonState state)
    {
        if (SelectedLesson is null) return;
        SelectedLesson.State = state;
        await SaveProgressAsync();
        Status = $"{SelectedLesson.DisplayTitle}: {state}";
    }

    [RelayCommand]
    private async Task SavePracticeAsync()
    {
        if (SelectedLesson is null)
        {
            LearningGateMessage = "Select a lesson first.";
            return;
        }

        if (PracticeAnswer.Trim().Length < 20)
        {
            LearningGateMessage = "Write at least one clear sentence before demo practice.";
            Status = LearningGateMessage;
            return;
        }

        var task = SelectedLesson.Lesson.PracticeTasks.FirstOrDefault();
        await lessonRepository.SavePracticeAttemptAsync(new PracticeAttempt
        {
            LessonId = SelectedLesson.Id,
            TaskId = task?.Id ?? $"{SelectedLesson.Id}-practice",
            Answer = PracticeAnswer.Trim()
        });

        SelectedLesson.State = LessonState.Practicing;
        PracticeAnswer = "";
        await SaveProgressAsync();
        Status = "Practice saved. You can now apply this concept in a demo plan.";
        RefreshLearningGate();
        if (UnderstandingSaved is not null) await UnderstandingSaved.Invoke();
    }

    [RelayCommand]
    private async Task EvaluateUnderstandingAsync()
    {
        if (SelectedLesson is null)
        {
            EvaluationFeedback = "Select a lesson first.";
            return;
        }

        if (!CanEvaluateSelectedLesson)
        {
            EvaluationFeedback = "Save your understanding first, then do the evaluation.";
            return;
        }

        var result = await conceptEvaluationService.EvaluateAsync(SelectedLesson.Lesson, EvaluationAnswer);
        await ApplyEvaluationResultAsync(result);
    }

    [RelayCommand]
    private void CopyEvaluationPrompt()
    {
        if (SelectedLesson is null)
        {
            EvaluationFeedback = "Select a lesson first.";
            return;
        }

        if (string.IsNullOrWhiteSpace(EvaluationAnswer))
        {
            EvaluationFeedback = "Write your explanation first, then copy the evaluation prompt.";
            return;
        }

        RefreshEvaluationPrompt();
        Clipboard.SetText(EvaluationPrompt);

        var url = settings.AiTarget.Equals("chatgpt", StringComparison.OrdinalIgnoreCase)
            ? "https://chatgpt.com"
            : "https://claude.ai/new";

        try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
        catch { /* browser unavailable, clipboard still has the prompt */ }

        EvaluationFeedback = "Prompt copied to clipboard. Browser opened — just paste and send. Then paste the JSON result below.";
    }

    [RelayCommand]
    private async Task ParseEvaluationResultAsync()
    {
        if (SelectedLesson is null)
        {
            EvaluationFeedback = "Select a lesson first.";
            return;
        }

        var result = conceptEvaluationService.ParseExpectedOutput(EvaluationJson);
        await ApplyEvaluationResultAsync(result);
    }

    public async Task SaveProgressAsync()
    {
        var items = Sections.SelectMany(x => x.Lessons).Select(x => x.ToProgress()).ToList();
        await lessonRepository.SaveProgressAsync(items);
    }

    private void MoveLesson(int delta)
    {
        var lessons = AllLessons;
        if (lessons.Count == 0) return;

        var index = SelectedLesson is null ? 0 : lessons.IndexOf(SelectedLesson);
        if (index < 0) index = 0;

        var next = Math.Clamp(index + delta, 0, lessons.Count - 1);
        SelectedLesson = lessons[next];
        Status = SelectedLesson.DisplayTitle;
        RefreshLearningGate();
    }

    private void RefreshLearningGate()
    {
        OnPropertyChanged(nameof(CanApplySelectedLesson));
        OnPropertyChanged(nameof(CanEvaluateSelectedLesson));

        LearningGateMessage = SelectedLesson switch
        {
            null => "Select a lesson first.",
            { State: >= LessonState.DemoApplied } => "Eligible: evaluation passed. You can practice this concept in demo.",
            { State: >= LessonState.Practicing } => "Understanding saved. Next: pass the evaluation step.",
            _ when PracticeAnswer.Trim().Length < 20 => "Not eligible yet: write one clear sentence, then save understanding.",
            _ => "Ready to save: click Save Understanding to unlock demo practice."
        };
    }

    private async Task ApplyEvaluationResultAsync(ConceptEvaluationResult result)
    {
        EvaluationScore = result.Score;

        if (!result.Passed)
        {
            var missing = result.Missing.Count == 0 ? "" : $" Missing: {string.Join("; ", result.Missing)}";
            EvaluationFeedback = $"{result.Feedback}{missing}";
            Status = EvaluationFeedback;
            RefreshLearningGate();
            return;
        }

        if (SelectedLesson is not null)
        {
            SelectedLesson.State = LessonState.DemoApplied;
            await SaveProgressAsync();
        }

        EvaluationFeedback = $"{result.Feedback} Score: {result.Score}/100. Evaluator: {(result.UsedAi ? "manual AI JSON" : "local parser")}.";
        Status = EvaluationFeedback;
        RefreshLearningGate();
        if (EvaluationPassed is not null) await EvaluationPassed.Invoke();
    }

    private void RefreshEvaluationPrompt()
    {
        EvaluationPrompt = SelectedLesson is null
            ? ""
            : conceptEvaluationService.BuildPrompt(SelectedLesson.Lesson, EvaluationAnswer);
    }
}
