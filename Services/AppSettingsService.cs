using System.Text.Json;
using ForexTradingWorkspace.Models;
using ForexTradingWorkspace.Services.Security;
using Serilog;

namespace ForexTradingWorkspace.Services;

public sealed class AppSettingsService(IEncryptionService encryption) : IAppSettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public async Task<AppSettings> LoadAsync()
    {
        try
        {
            if (!File.Exists(AppPaths.SettingsFile))
            {
                var defaults = new AppSettings();
                await SaveAsync(defaults);
                return defaults;
            }

            var cipher = await File.ReadAllTextAsync(AppPaths.SettingsFile);
            var json = encryption.Unprotect(cipher);
            return JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load settings; using defaults.");
            return new AppSettings();
        }
    }

    public async Task SaveAsync(AppSettings settings)
    {
        Directory.CreateDirectory(AppPaths.DataPath);
        var json = JsonSerializer.Serialize(settings, JsonOptions);
        await File.WriteAllTextAsync(AppPaths.SettingsFile, encryption.Protect(json));
    }
}
