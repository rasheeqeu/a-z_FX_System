using CommunityToolkit.Mvvm.ComponentModel;
using ForexTradingWorkspace.Models.Journal;
using ForexTradingWorkspace.Models.Learning;
using ForexTradingWorkspace.Services.Journal;

namespace ForexTradingWorkspace.ViewModels;

public partial class ReviewViewModel(IJournalRepository journalRepository) : ObservableObject
{
    [ObservableProperty] private int lessonsCompleted;
    [ObservableProperty] private int journalCount;
    [ObservableProperty] private decimal netProfitLoss;
    [ObservableProperty] private decimal winRate;
    [ObservableProperty] private string topMistakes = "No mistakes recorded yet.";
    [ObservableProperty] private string nextImprovement = "Create one planned demo trade and review it.";

    public async Task RefreshAsync(IEnumerable<LessonItemViewModel> lessons)
    {
        var entries = (await journalRepository.LoadAsync()).ToList();
        JournalCount = entries.Count;
        LessonsCompleted = lessons.Count(x => x.State == LessonState.Completed || x.State == LessonState.Reviewed);
        NetProfitLoss = entries.Sum(x => x.ProfitLoss);
        WinRate = entries.Count == 0 ? 0 : Math.Round((decimal)entries.Count(x => x.ProfitLoss > 0) / entries.Count * 100m, 2);
        TopMistakes = BuildTopMistakes(entries);
        NextImprovement = entries.Count == 0
            ? "Finish the first plan -> screenshot -> journal loop."
            : "Review the most repeated mistake before the next demo trade.";
    }

    private static string BuildTopMistakes(IEnumerable<TradeJournalEntry> entries)
    {
        var mistakes = entries
            .SelectMany(x => x.MistakeTags)
            .GroupBy(x => x)
            .OrderByDescending(x => x.Count())
            .Take(3)
            .Select(x => $"{x.Key} ({x.Count()})")
            .ToList();

        return mistakes.Count == 0 ? "No mistakes recorded yet." : string.Join(", ", mistakes);
    }
}
