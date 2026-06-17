using CommunityToolkit.Mvvm.ComponentModel;
using ForexTradingWorkspace.Models.Learning;

namespace ForexTradingWorkspace.ViewModels;

public partial class LessonItemViewModel(Lesson lesson, LessonProgress progress) : ObservableObject
{
    public Lesson Lesson { get; } = lesson;
    public string Id => Lesson.Id;
    public string Title => Lesson.Title;
    public string Duration => Lesson.Duration;
    public string DisplayTitle => $"{Lesson.Id} {Lesson.Title}";

    [ObservableProperty] private LessonState state = progress.State;

    public LessonProgress ToProgress() => new()
    {
        LessonId = Id,
        State = State,
        LastUpdatedUtc = DateTime.UtcNow
    };
}
