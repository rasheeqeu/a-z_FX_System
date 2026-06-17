namespace ForexTradingWorkspace.Services;

public interface IAuditService
{
    Task LogActionAsync(
        string actionName,
        Dictionary<string, object>? parameters = null,
        bool success = true,
        string? error = null);

    Task<bool> ValidateAuditLogAsync();
    bool IsAuditLogValid { get; }
}
