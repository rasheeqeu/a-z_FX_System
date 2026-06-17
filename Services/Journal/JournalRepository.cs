using System.Text.Json;
using ForexTradingWorkspace.Models.Journal;

namespace ForexTradingWorkspace.Services.Journal;

public sealed class JournalRepository : IJournalRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly string _journalFile = Path.Combine(AppPaths.DataPath, "learning-journal.json");

    public async Task<IReadOnlyList<TradeJournalEntry>> LoadAsync()
    {
        if (!File.Exists(_journalFile))
        {
            return [];
        }

        var json = await File.ReadAllTextAsync(_journalFile);
        return JsonSerializer.Deserialize<List<TradeJournalEntry>>(json, JsonOptions) ?? [];
    }

    public async Task SaveAsync(IEnumerable<TradeJournalEntry> entries)
    {
        Directory.CreateDirectory(AppPaths.DataPath);
        var json = JsonSerializer.Serialize(entries.OrderByDescending(x => x.CreatedAtUtc).ToList(), JsonOptions);
        await File.WriteAllTextAsync(_journalFile, json);
    }

    public async Task UpsertAsync(TradeJournalEntry entry)
    {
        var entries = (await LoadAsync()).ToList();
        var index = entries.FindIndex(x => x.Id == entry.Id);
        if (index >= 0) entries[index] = entry;
        else entries.Add(entry);
        await SaveAsync(entries);
    }
}
