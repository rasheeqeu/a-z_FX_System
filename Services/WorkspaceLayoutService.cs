using System.Text.Json;
using ForexTradingWorkspace.Models;
using Serilog;

namespace ForexTradingWorkspace.Services;

public sealed class WorkspaceLayoutService : IWorkspaceLayoutService
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public async Task<WorkspaceLayout> LoadAsync()
    {
        try
        {
            if (!File.Exists(AppPaths.LayoutFile))
            {
                var layout = new WorkspaceLayout();
                await SaveAsync(layout);
                return layout;
            }

            var json = await File.ReadAllTextAsync(AppPaths.LayoutFile);
            return JsonSerializer.Deserialize<WorkspaceLayout>(json, Options) ?? new WorkspaceLayout();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load workspace layout.");
            return new WorkspaceLayout();
        }
    }

    public async Task SaveAsync(WorkspaceLayout layout)
    {
        Directory.CreateDirectory(AppPaths.DataPath);
        await File.WriteAllTextAsync(AppPaths.LayoutFile, JsonSerializer.Serialize(layout, Options));
    }
}
