namespace ForexTradingWorkspace.Services;

public class FeedbackService : IFeedbackService
{
    public event EventHandler<NotificationEventArgs>? NotificationRequested;
    public event EventHandler<ProgressEventArgs>? ProgressRequested;
    public event EventHandler<string>? StatusChanged;

    public void ShowNotification(string title, string message, NotificationType type = NotificationType.Info, int durationMs = 3000)
    {
        try
        {
            var args = new NotificationEventArgs
            {
                Title = title,
                Message = message,
                Type = type,
                DurationMs = durationMs
            };
            NotificationRequested?.Invoke(this, args);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Notification failed: {ex.Message}");
        }
    }

    public void ShowProgress(string title, int percentage)
    {
        try
        {
            var args = new ProgressEventArgs
            {
                Title = title,
                Percentage = Math.Clamp(percentage, 0, 100)
            };
            ProgressRequested?.Invoke(this, args);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Progress failed: {ex.Message}");
        }
    }

    public void HideProgress()
    {
        try
        {
            var args = new ProgressEventArgs { Percentage = -1 };
            ProgressRequested?.Invoke(this, args);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Hide progress failed: {ex.Message}");
        }
    }

    public void SetStatus(string message)
    {
        try
        {
            StatusChanged?.Invoke(this, message);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Status failed: {ex.Message}");
        }
    }

    public void ClearStatus()
    {
        SetStatus(string.Empty);
    }
}
