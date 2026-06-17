namespace ForexTradingWorkspace.Services.Notifications;

public interface INotificationService
{
    Task NotifyAsync(string title, string message);
}
