using ForexTradingWorkspace.Models.Journal;

namespace ForexTradingWorkspace.Services.Journal;

public interface IJournalRepository
{
    Task<IReadOnlyList<TradeJournalEntry>> LoadAsync();
    Task SaveAsync(IEnumerable<TradeJournalEntry> entries);
    Task UpsertAsync(TradeJournalEntry entry);
}
