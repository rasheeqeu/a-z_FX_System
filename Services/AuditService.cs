using System.Text.Json;

namespace ForexTradingWorkspace.Services;

public class AuditService : IAuditService
{
    private readonly string _auditLogPath;
    private readonly object _lockObject = new object();
    private bool _isAuditLogValid = true;

    public AuditService()
    {
        _auditLogPath = Path.Combine(AppPaths.LogsPath, "audit.json");
        EnsureDirectoryExists();
    }

    public async Task<bool> ValidateAuditLogAsync()
    {
        try
        {
            if (!File.Exists(_auditLogPath))
            {
                _isAuditLogValid = true;
                return true;
            }

            return await Task.Run(() =>
            {
                lock (_lockObject)
                {
                    try
                    {
                        var json = File.ReadAllText(_auditLogPath);
                        var lines = json.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                        foreach (var line in lines)
                        {
                            if (string.IsNullOrWhiteSpace(line))
                                continue;

                            using (JsonDocument.Parse(line)) { }
                        }

                        _isAuditLogValid = true;
                        return true;
                    }
                    catch
                    {
                        _isAuditLogValid = false;
                        return false;
                    }
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Audit log validation failed: {ex.Message}");
            _isAuditLogValid = false;
            return false;
        }
    }

    public bool IsAuditLogValid => _isAuditLogValid;

    public async Task LogActionAsync(
        string actionName,
        Dictionary<string, object>? parameters = null,
        bool success = true,
        string? error = null)
    {
        try
        {
            var auditEntry = new
            {
                timestamp = DateTime.UtcNow.ToString("O"),
                actionName = actionName,
                parameters = parameters,
                success = success,
                error = error
            };

            var json = JsonSerializer.Serialize(auditEntry);

            await Task.Run(() =>
            {
                lock (_lockObject)
                {
                    File.AppendAllText(_auditLogPath, json + Environment.NewLine);
                }
            });
        }
        catch (Exception ex)
        {
            // Fail silently - audit logging failures should not crash the app
            System.Diagnostics.Debug.WriteLine($"Audit logging failed: {ex.Message}");
        }
    }

    private void EnsureDirectoryExists()
    {
        var directory = Path.GetDirectoryName(_auditLogPath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}
