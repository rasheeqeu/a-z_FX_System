using System.Text.Json;

namespace ForexTradingWorkspace.Services;

public class StateService : IStateService
{
    private readonly string _statesPath;
    private readonly object _lockObject = new object();

    public StateService()
    {
        _statesPath = Path.Combine(AppPaths.RootPath, "states");
        EnsureDirectoryExists();
    }

    public async Task SaveStateAsync(string screenName, object stateData)
    {
        try
        {
            var json = JsonSerializer.Serialize(stateData, new JsonSerializerOptions { WriteIndented = true });
            var filePath = Path.Combine(_statesPath, $"{screenName}.json");

            await Task.Run(() =>
            {
                lock (_lockObject)
                {
                    File.WriteAllText(filePath, json);
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"State save failed for {screenName}: {ex.Message}");
        }
    }

    public async Task<T?> LoadStateAsync<T>(string screenName) where T : class
    {
        try
        {
            var filePath = Path.Combine(_statesPath, $"{screenName}.json");

            if (!File.Exists(filePath))
                return null;

            return await Task.Run(() =>
            {
                lock (_lockObject)
                {
                    var json = File.ReadAllText(filePath);
                    return JsonSerializer.Deserialize<T>(json);
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"State load failed for {screenName}: {ex.Message}");
            return null;
        }
    }

    public async Task ValidateStateAsync(string screenName)
    {
        try
        {
            var filePath = Path.Combine(_statesPath, $"{screenName}.json");

            if (!File.Exists(filePath))
                return;

            await Task.Run(() =>
            {
                lock (_lockObject)
                {
                    var json = File.ReadAllText(filePath);
                    using (JsonDocument.Parse(json)) { }
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"State validation failed for {screenName}: {ex.Message}");
        }
    }

    public async Task<Dictionary<string, bool>> ValidateAllStateFilesAsync()
    {
        var validationResults = new Dictionary<string, bool>();
        var screens = new[] { "Browser", "Calendar", "Journal", "Settings" };

        foreach (var screen in screens)
        {
            var isValid = false;
            try
            {
                var filePath = Path.Combine(_statesPath, $"{screen}.json");

                if (!File.Exists(filePath))
                {
                    validationResults[screen] = false;
                    continue;
                }

                await Task.Run(() =>
                {
                    lock (_lockObject)
                    {
                        var json = File.ReadAllText(filePath);
                        using (JsonDocument.Parse(json))
                        {
                            isValid = true;
                        }
                    }
                });

                validationResults[screen] = isValid;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"State validation failed for {screen}: {ex.Message}");
                validationResults[screen] = false;
            }
        }

        return validationResults;
    }

    private void EnsureDirectoryExists()
    {
        if (!Directory.Exists(_statesPath))
        {
            Directory.CreateDirectory(_statesPath);
        }
    }
}
