# AGENTS.md - Forex Trading Workspace

Professional forex trading workspace application with minimalistic, border-free UI and full MVVM compliance.

## Project Overview

**ForexTradingWorkspace** is a WPF desktop application (Windows 10/11, .NET 8) that provides an all-in-one trading environment for forex traders. Features include:

- Embedded WebView2 browser (TradingView integration)
- Trade journal with SQLite database
- Position-size calculator
- Performance analytics
- Session clocks and market data
- Professional dark theme (minimalistic, border-free design)

## Prerequisites

- Windows 10 or 11
- .NET 8 SDK
- Microsoft Edge WebView2 Runtime (preinstalled on most Windows systems)

## Build & Run

```powershell
# Clean and build
dotnet clean
dotnet build

# Run the application
dotnet run

# Publish self-contained executable
dotnet publish -c Release -r win-x64 --self-contained
```

## Architecture Overview

### Startup & Dependency Injection

`App.xaml.cs` bootstraps a `Microsoft.Extensions.Hosting` host with singleton services:

1. Create app directories (`AppPaths`)
2. Configure Serilog (logs to `%LocalAppData%\ForexTradingWorkspace\Logs\`)
3. Build DI container
4. Show `MainWindow` immediately
5. Initialize database (`IDatabaseInitializer`)
6. Initialize `MainViewModel` (load settings, layout, trades)

Exception handling catches unhandled UI dispatcher, AppDomain, and task exceptions — all logged via Serilog.

### Data Storage

`%LocalAppData%\ForexTradingWorkspace\`:
- `Data\settings.secure.json` — DPAPI-encrypted settings
- `Data\workspace-layout.json` — workspace state
- `Data\journal.db` — SQLite trade journal
- `Logs\` — daily Serilog files

### MVVM Pattern

Uses **CommunityToolkit.Mvvm** source generators:
- `[ObservableProperty]` for properties with `INotifyPropertyChanged`
- `[RelayCommand]` for command bindings
- `partial void On<Property>Changed(...)` for reactive logic

**MainViewModel** is the single top-level view model containing:
- Navigation state (`ActiveModule`)
- Account balance & daily notes
- Browser integration (bookmarks, URL)
- Calculator & Analytics sub-VMs

### Module Architecture

All modules are fully independent with MVVM compliance:
- **Dashboard** — Account overview and quick stats
- **Markets** — WebView2 browser with TradingView integration
- **Performance** — Trade journal with SQLite backend and analytics
- **Configuration** — Settings, layout preferences, checklist

### Browser Integration

`MainWindow.xaml` hosts `WebView2` controls for Chromium browser embedding. Navigation helpers locate the correct WebView2 and normalize URLs before navigation.

## Key Dependencies

| Package | Purpose |
|---|---|
| `Microsoft.Web.WebView2` | Embedded Chromium browser |
| `Microsoft.Data.Sqlite` | SQLite trade journal |
| `CommunityToolkit.Mvvm` | MVVM source generators |
| `Microsoft.Extensions.Hosting` | Dependency injection |
| `ClosedXML` | Excel export |
| `Serilog` | Structured logging |
| `System.Security.Cryptography.ProtectedData` | DPAPI encryption |

## Design

The application uses a **minimalistic, border-free aesthetic** with professional dark theme. No unnecessary visual elements or decorative borders. Clean, functional interface optimized for trader workflows.

## Key Features

- **Minimalistic border-free UI** — Clean dark theme optimized for traders
- **Full MVVM compliance** — Independent modules with state management
- **Real-time market data** — WebView2 bridge for TradingView integration
- **Persistent state** — Module layouts and settings saved on exit
- **Encrypted storage** — DPAPI-encrypted settings and credentials
- **Audit logging** — All user actions logged via Serilog

## Troubleshooting

- **App won't start:** Check logs at `%LocalAppData%\ForexTradingWorkspace\Logs\`
- **WebView2 errors:** Ensure WebView2 Runtime is installed
- **Database locked:** Close other instances of the app
- **Settings not saving:** Verify DPAPI encryption permissions
