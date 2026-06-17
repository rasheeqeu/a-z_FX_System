using System.Media;
using System.Windows;

namespace ForexTradingWorkspace.Services.Notifications;

public sealed class DesktopNotificationService : INotificationService
{
    public Task NotifyAsync(string title, string message)
    {
        SystemSounds.Asterisk.Play();
        System.Windows.Application.Current.Dispatcher.Invoke(() => System.Windows.MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information));
        return Task.CompletedTask;
    }
}
