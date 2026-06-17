using ForexTradingWorkspace.Models;

namespace ForexTradingWorkspace.Services;

public interface IAppSettingsService
{
    Task<AppSettings> LoadAsync();
    Task SaveAsync(AppSettings settings);
}
