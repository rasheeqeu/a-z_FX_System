using ForexTradingWorkspace.Models;

namespace ForexTradingWorkspace.Services.Repositories;

public interface ITradeRepository
{
    Task<IReadOnlyList<Trade>> SearchAsync(string? pair = null, DateTime? from = null, DateTime? to = null);
    Task<Trade> AddAsync(Trade trade);
    Task UpdateAsync(Trade trade);
    Task DeleteAsync(long id);
}
