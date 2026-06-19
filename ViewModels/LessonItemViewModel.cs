using CommunityToolkit.Mvvm.ComponentModel;
using ForexTradingWorkspace.Models.Learning;
using System.IO;

namespace ForexTradingWorkspace.ViewModels;

public partial class LessonItemViewModel(Lesson lesson, LessonProgress progress) : ObservableObject
{
    public Lesson Lesson { get; } = lesson;
    public string Id => Lesson.Id;
    public string Title => Lesson.Title;
    public string Duration => Lesson.Duration;
    public string DisplayTitle => $"{Lesson.Id} {Lesson.Title}";
    public bool HasVideo => !string.IsNullOrEmpty(Lesson.VideoPath) && File.Exists(Lesson.VideoPath);
    public Uri? VideoUri => HasVideo ? new Uri(Lesson.VideoPath!) : null;

    [ObservableProperty] private LessonState state = progress.State;
    [ObservableProperty] private bool isSelected = false;

    public LessonProgress ToProgress() => new()
    {
        LessonId = Id,
        State = State,
        LastUpdatedUtc = DateTime.UtcNow
    };
}
