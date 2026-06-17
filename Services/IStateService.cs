namespace ForexTradingWorkspace.Services;

public interface IStateService
{
    Task SaveStateAsync(string screenName, object stateData);
    Task<T?> LoadStateAsync<T>(string screenName) where T : class;
    Task ValidateStateAsync(string screenName);
    Task<Dictionary<string, bool>> ValidateAllStateFilesAsync();
}
