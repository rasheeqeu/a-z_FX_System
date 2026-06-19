using System.Windows;
using System.Windows.Controls;
using ForexTradingWorkspace.Models.Screenshots;
using ForexTradingWorkspace.Services;
using ForexTradingWorkspace.Services.Browser;
using ForexTradingWorkspace.ViewModels;

namespace ForexTradingWorkspace.Views;

public partial class MainWindow : Window
{
    private readonly ShellViewModel _viewModel;
    private readonly IScreenshotService _screenshotService;

    public MainWindow(ShellViewModel viewModel, IScreenshotService screenshotService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _screenshotService = screenshotService;
        DataContext = viewModel;
        Loaded += OnWindowLoaded;
    }

    private async void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        EnsureWindowIsVisible();
        await BrokerWebView.EnsureCoreWebView2Async();
        await ChartsWebView.EnsureCoreWebView2Async();
        HookFeedback();
    }

    private void NavDrawerPopup_Closed(object sender, EventArgs e)
    {
        _viewModel.IsNavOpen = false;
    }

    private void EnsureWindowIsVisible()
    {
        var workArea = SystemParameters.WorkArea;
        var safeTop = workArea.Top + 12;
        var safeLeft = workArea.Left + 12;
        var safeRight = workArea.Right - 12;
        var safeBottom = workArea.Bottom - 12;

        if (Width > workArea.Width)
        {
            Width = Math.Max(MinWidth, workArea.Width - 24);
        }

        if (Height > workArea.Height)
        {
            Height = Math.Max(MinHeight, workArea.Height - 24);
        }

        if (Top < safeTop || Top + 48 > safeBottom)
        {
            Top = safeTop;
        }

        if (Left < safeLeft || Left + 120 > safeRight)
        {
            Left = safeLeft;
        }
    }

    private void BrowserBack_Click(object sender, RoutedEventArgs e)
    {
        if (BrokerWebView.CanGoBack) BrokerWebView.GoBack();
    }

    private void BrowserForward_Click(object sender, RoutedEventArgs e)
    {
        if (BrokerWebView.CanGoForward) BrokerWebView.GoForward();
    }

    private void BrowserRefresh_Click(object sender, RoutedEventArgs e)
    {
        BrokerWebView.Reload();
    }

    private async void CaptureBefore_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var file = await _screenshotService.CaptureAsync(BrokerWebView, _viewModel.Planner.Plan.Pair);
            await _viewModel.Planner.AttachScreenshotAsync(file, ScreenshotCaptureType.BeforeTrade);
            ShowNotification(new NotificationEventArgs
            {
                Title = "Screenshot Captured",
                Message = "Before-trade screenshot attached to the current plan.",
                Type = NotificationType.Success
            });
        }
        catch (Exception ex)
        {
            ShowNotification(new NotificationEventArgs
            {
                Title = "Screenshot Failed",
                Message = ex.Message,
                Type = NotificationType.Error
            });
        }
    }

    private void HookFeedback()
    {
        var app = (App)Application.Current;
        var feedbackService = app.Services?.GetService(typeof(IFeedbackService)) as IFeedbackService;
        if (feedbackService == null) return;

        feedbackService.ProgressRequested += (_, args) =>
        {
            Dispatcher.Invoke(() =>
            {
                ProgressOverlay.Visibility = args.Percentage < 0 ? Visibility.Collapsed : Visibility.Visible;
                ProgressTitle.Text = args.Title;
                ProgressBar.Value = Math.Clamp(args.Percentage, 0, 100);
                ProgressPercentage.Text = $"{Math.Clamp(args.Percentage, 0, 100)}%";
            });
        };

        feedbackService.NotificationRequested += (_, args) =>
        {
            Dispatcher.Invoke(() => ShowNotification(args));
        };
    }

    private void ShowNotification(NotificationEventArgs args)
    {
        var toast = new Border
        {
            Width = 300,
            Background = (System.Windows.Media.Brush)FindResource("DS_PanelSurfaceBrush"),
            BorderBrush = args.Type switch
            {
                NotificationType.Success => (System.Windows.Media.Brush)FindResource("DS_SuccessBrush"),
                NotificationType.Error => (System.Windows.Media.Brush)FindResource("DS_DangerBrush"),
                NotificationType.Warning => (System.Windows.Media.Brush)FindResource("DS_WarningBrush"),
                _ => (System.Windows.Media.Brush)FindResource("DS_AccentBrush")
            },
            BorderThickness = new Thickness(2, 0, 0, 0),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(12),
            Margin = new Thickness(0, 0, 0, 8)
        };

        toast.Child = new StackPanel
        {
            Children =
            {
                new TextBlock { Text = args.Title, FontWeight = FontWeights.SemiBold },
                new TextBlock { Text = args.Message, TextWrapping = TextWrapping.Wrap, Foreground = (System.Windows.Media.Brush)FindResource("DS_SecondaryTextBrush") }
            }
        };

        ToastContainer.Children.Insert(0, toast);
        var timer = new System.Timers.Timer(Math.Max(500, args.DurationMs));
        timer.Elapsed += (_, _) =>
        {
            Dispatcher.Invoke(() => ToastContainer.Children.Remove(toast));
            timer.Dispose();
        };
        timer.Start();
    }

    private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
