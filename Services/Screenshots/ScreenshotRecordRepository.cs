using System.Text.Json;
using ForexTradingWorkspace.Models.Screenshots;

namespace ForexTradingWorkspace.Services.Screenshots;

public sealed class ScreenshotRecordRepository : IScreenshotRecordRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly string _file = Path.Combine(AppPaths.DataPath, "screenshot-records.json");

    public async Task<IReadOnlyList<ScreenshotRecord>> LoadAsync()
    {
        if (!File.Exists(_file))
        {
            return [];
        }

        var json = await File.ReadAllTextAsync(_file);
        return JsonSerializer.Deserialize<List<ScreenshotRecord>>(json, JsonOptions) ?? [];
    }

    public async Task AddAsync(ScreenshotRecord record)
    {
        Directory.CreateDirectory(AppPaths.DataPath);
        var records = (await LoadAsync()).ToList();
        records.Add(record);
        var json = JsonSerializer.Serialize(records.OrderByDescending(x => x.CapturedAtUtc).ToList(), JsonOptions);
        await File.WriteAllTextAsync(_file, json);
    }
}
