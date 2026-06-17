using ForexTradingWorkspace.Models.Learning;

namespace ForexTradingWorkspace.Services.Learning;

public interface ILessonRepository
{
    Task<IReadOnlyList<LessonSection>> LoadSectionsAsync();
    Task<Dictionary<string, LessonProgress>> LoadProgressAsync();
    Task SaveProgressAsync(IEnumerable<LessonProgress> progress);
    Task<IReadOnlyList<PracticeAttempt>> LoadPracticeAttemptsAsync();
    Task SavePracticeAttemptAsync(PracticeAttempt attempt);
}
