using ForexTradingWorkspace.Models.Screenshots;

namespace ForexTradingWorkspace.Services.Screenshots;

public interface IScreenshotRecordRepository
{
    Task<IReadOnlyList<ScreenshotRecord>> LoadAsync();
    Task AddAsync(ScreenshotRecord record);
}
