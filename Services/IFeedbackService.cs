namespace ForexTradingWorkspace.Services;

public interface IFeedbackService
{
    void ShowNotification(string title, string message, NotificationType type = NotificationType.Info, int durationMs = 3000);
    void ShowProgress(string title, int percentage);
    void HideProgress();
    void SetStatus(string message);
    void ClearStatus();

    event EventHandler<NotificationEventArgs>? NotificationRequested;
    event EventHandler<ProgressEventArgs>? ProgressRequested;
    event EventHandler<string>? StatusChanged;
}

public enum NotificationType
{
    Info,
    Success,
    Warning,
    Error
}

public class NotificationEventArgs : EventArgs
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public int DurationMs { get; set; } = 3000;
}

public class ProgressEventArgs : EventArgs
{
    public string Title { get; set; } = string.Empty;
    public int Percentage { get; set; }
}
