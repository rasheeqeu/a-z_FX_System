using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ForexTradingWorkspace.Models.Journal;
using ForexTradingWorkspace.Services.Journal;

namespace ForexTradingWorkspace.ViewModels;

public partial class JournalViewModel(IJournalRepository journalRepository) : ObservableObject
{
    public event Func<Task>? EntrySaved;

    [ObservableProperty] private TradeJournalEntry? selectedEntry;
    [ObservableProperty] private string status = "No journal entry selected.";
    [ObservableProperty] private string mistakeTagsText = "";

    public ObservableCollection<TradeJournalEntry> Entries { get; } = [];
    public IReadOnlyList<string> MistakeTags { get; } =
    [
        "FOMO", "Revenge trade", "No plan", "Bad entry", "Moved stop loss",
        "Exited early", "Ignored news", "Risk too high", "Overtrading"
    ];

    public async Task InitializeAsync()
    {
        Entries.Clear();
        var loaded = await journalRepository.LoadAsync();
        var cleaned = loaded
            .GroupBy(x => string.IsNullOrWhiteSpace(x.Plan.Id) ? x.Id : x.Plan.Id)
            .Select(x => x.OrderByDescending(entry => entry.CreatedAtUtc).First())
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToList();

        foreach (var entry in cleaned)
        {
            Entries.Add(entry);
        }

        if (cleaned.Count != loaded.Count)
        {
            await journalRepository.SaveAsync(cleaned);
        }

        SelectedEntry = Entries.FirstOrDefault();
    }

    partial void OnSelectedEntryChanged(TradeJournalEntry? value)
    {
        MistakeTagsText = value is null ? "" : string.Join(", ", value.MistakeTags);
    }

    [RelayCommand]
    private async Task SaveSelectedAsync()
    {
        if (SelectedEntry is null) return;
        SelectedEntry.MistakeTags = ParseMistakes(MistakeTagsText);
        await journalRepository.UpsertAsync(SelectedEntry);
        Status = "Journal entry saved.";
        if (EntrySaved is not null) await EntrySaved.Invoke();
    }

    [RelayCommand]
    private async Task MarkReviewedAsync()
    {
        if (SelectedEntry is null) return;
        SelectedEntry.ReviewNotes = string.IsNullOrWhiteSpace(SelectedEntry.ReviewNotes)
            ? "Reviewed."
            : SelectedEntry.ReviewNotes;
        await journalRepository.UpsertAsync(SelectedEntry);
        Status = "Trade reviewed.";
    }

    private static List<string> ParseMistakes(string value)
    {
        return value.Split([',', ';', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
