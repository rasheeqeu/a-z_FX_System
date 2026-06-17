using System.Collections.ObjectModel;
using ForexTradingWorkspace.Models.Learning;

namespace ForexTradingWorkspace.ViewModels;

public sealed class LessonSectionViewModel
{
    public LessonSectionViewModel(LessonSection section, Dictionary<string, LessonProgress> progress)
    {
        Title = $"{section.SectionNumber}. {section.Title}";
        Lessons = new ObservableCollection<LessonItemViewModel>(
            section.Lessons.Select(lesson => new LessonItemViewModel(
                lesson,
                progress.TryGetValue(lesson.Id, out var item)
                    ? item
                    : new LessonProgress { LessonId = lesson.Id })));
    }

    public string Title { get; }
    public ObservableCollection<LessonItemViewModel> Lessons { get; }
}
