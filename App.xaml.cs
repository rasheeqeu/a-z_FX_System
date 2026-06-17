using ForexTradingWorkspace.Services;
using ForexTradingWorkspace.Services.Browser;
using ForexTradingWorkspace.Services.Export;
using ForexTradingWorkspace.Services.Journal;
using ForexTradingWorkspace.Services.Learning;
using ForexTradingWorkspace.Services.Notifications;
using ForexTradingWorkspace.Services.Repositories;
using ForexTradingWorkspace.Services.Risk;
using ForexTradingWorkspace.Services.Screenshots;
using ForexTradingWorkspace.Services.Security;
using ForexTradingWorkspace.ViewModels;
using ForexTradingWorkspace.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Windows;
using System.Windows.Threading;

namespace ForexTradingWorkspace;

public partial class App : System.Windows.Application
{
    private IHost? _host;

    public IServiceProvider? Services => _host?.Services;

    private async void OnStartup(object sender, StartupEventArgs e)
    {
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        Directory.CreateDirectory(AppPaths.DataPath);
        Directory.CreateDirectory(AppPaths.ScreenshotsPath);
        Directory.CreateDirectory(AppPaths.LogsPath);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(Path.Combine(AppPaths.LogsPath, "workspace-.log"), rollingInterval: RollingInterval.Day)
            .CreateLogger();

        try
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IAppSettingsService, AppSettingsService>();
                    services.AddSingleton<IEncryptionService, DpapiEncryptionService>();
                    services.AddSingleton<IDatabaseInitializer, SqliteDatabaseInitializer>();
                    services.AddSingleton<ITradeRepository, SqliteTradeRepository>();
                    services.AddSingleton<IJournalExportService, JournalExportService>();
                    services.AddSingleton<IWorkspaceLayoutService, WorkspaceLayoutService>();
                    services.AddSingleton<IScreenshotService, ScreenshotService>();
                    services.AddSingleton<INotificationService, DesktopNotificationService>();
                    services.AddSingleton<SessionClockService>();
                    services.AddSingleton<IWebView2BridgeService, WebView2BridgeService>();
                    services.AddSingleton<IAuditService, AuditService>();
                    services.AddSingleton<IStateService, StateService>();
                    services.AddSingleton<IFeedbackService, FeedbackService>();
                    services.AddSingleton<ILessonRepository, LessonRepository>();
                    services.AddSingleton<IRiskCalculationService, RiskCalculationService>();
                    services.AddSingleton<IRuleGuardService, RuleGuardService>();
                    services.AddSingleton<IJournalRepository, JournalRepository>();
                    services.AddSingleton<IScreenshotRecordRepository, ScreenshotRecordRepository>();
                    services.AddSingleton<IAiReviewExportService, AiReviewExportService>();
                    services.AddSingleton<IConceptEvaluationService, ManualConceptEvaluationService>();
                    services.AddTransient<LessonEditorViewModel>();
                    services.AddTransient<LessonEditorWindow>();
                    services.AddSingleton<Func<LessonEditorWindow>>(sp => () => sp.GetRequiredService<LessonEditorWindow>());
                    services.AddSingleton<LearningViewModel>();
                    services.AddSingleton<BrokerPortalViewModel>();
                    services.AddSingleton<TradePlannerViewModel>();
                    services.AddSingleton<JournalViewModel>();
                    services.AddSingleton<ReviewViewModel>();
                    services.AddSingleton<SettingsViewModel>();
                    services.AddSingleton<ShellViewModel>();
                    services.AddSingleton<MainViewModel>();
                    services.AddTransient<MainWindow>();
                })
                .UseSerilog()
                .Build();

            var window = _host.Services.GetRequiredService<MainWindow>();
            window.Show();

            await _host.Services.GetRequiredService<IDatabaseInitializer>().InitializeAsync();
            await _host.Services.GetRequiredService<ShellViewModel>().InitializeAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application startup failed.");
            System.Windows.MessageBox.Show(ex.Message, "Forex Trading Workspace startup failed", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(-1);
        }
    }

    private async void OnExit(object sender, ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync(TimeSpan.FromSeconds(2));
            _host.Dispose();
        }

        Log.CloseAndFlush();
    }

    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Unhandled UI exception.");
        e.Handled = true;
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception exception)
        {
            Log.Fatal(exception, "Unhandled application exception.");
        }
    }

    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Unobserved task exception.");
        e.SetObserved();
    }
}
