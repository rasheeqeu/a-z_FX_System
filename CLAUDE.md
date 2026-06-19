# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

---

# ForexTradingWorkspace – Forex Learning & Trading Application

**Tech Stack:** WPF (.NET 8) + MVVM + SQLite + WebView2 (Chromium browser)

**Purpose:** Desktop application that teaches forex trading concepts through a guided learning system, then helps traders practice with risk calculations, trade planning, and journaling in a demo broker account.

## Quick Commands

```powershell
# Build & Run
dotnet clean
dotnet build
dotnet run

# Run with verbose output
dotnet run --verbose

# Publish self-contained executable
dotnet publish -c Release -r win-x64 --self-contained

# Check for compilation errors only (fast)
dotnet build --no-restore
```

## Architecture Overview

### High-Level Layout

The app uses a **three-panel workspace**:

```
┌────────────────────────────────────────────────┐
│ Top Bar: Profile | Session Clock | Daily Risk │
├──────────┬────────────────────────┬────────────┤
│ Left Nav │ Center Workspace       │ Right Side │
│          │ (WebView2 / Lessons)   │ (Assistant │
│  Learn   │                        │  Panel)    │
│  Broker  │ Active Module View     │  Planner   │
│  Plan    │                        │  Risk Calc │
│  Journal │                        │  Checklist │
│  Review  │                        │  Warnings  │
├──────────┴────────────────────────┴────────────┤
│ Bottom: Trade Notes | Screenshots | Journal    │
└────────────────────────────────────────────────┘
```

### Startup Sequence

[App.xaml.cs](App.xaml.cs:27-94):
1. Register exception handlers (dispatcher, AppDomain, tasks)
2. Create app directories (`AppPaths.DataPath`, `ScreenshotsPath`, `LogsPath`)
3. Configure Serilog → daily log files at `%LocalAppData%\ForexTradingWorkspace\Logs\`
4. Build DI container (Microsoft.Extensions.Hosting)
5. Show `MainWindow` immediately (DataContext = `ShellViewModel`)
6. Run async initialization: `IDatabaseInitializer.InitializeAsync()` → `ShellViewModel.InitializeAsync()`

**Key Pattern:** The app shows the UI first, then initializes database/state asynchronously. This prevents blocking the user on startup.

### MVVM Structure

Uses **CommunityToolkit.Mvvm** with source generators:

- **`[ObservableProperty]`** — Auto-generates INotifyPropertyChanged properties
- **`[RelayCommand]`** — Auto-generates ICommand bindings
- **`partial void On<Property>Changed(...)`** — Reactive logic hooks

**View Layer:**
- XAML-only (layout & binding)
- Code-behind allowed only for WPF/WebView2 events that cannot bind cleanly

**ViewModel Layer:**
- Coordinates state and user commands
- Inherits from `ObservableObject` (from CommunityToolkit.Mvvm)
- **Never** references UI thread directly; all commands are on UI thread

**Service Layer:**
- Business logic, risk calculations, persistence
- Must not reference WPF/XAML
- Injected as singletons (see [App.xaml.cs](App.xaml.cs:44-81))

### Two-ViewModel Shell Pattern

There are two top-level ViewModels coordinating the shell:

- **`MainViewModel`** — bound to `MainWindow` DataContext. Owns all workspace-level state: module navigation (`ActiveModule`), layout (`ModuleStateMachine`, `LayoutStateMachine`), browser URL, trades list, session clock, account balance, bookmarks, checklist, and all `[RelayCommand]` methods that drive the shell UI.
- **`ShellViewModel`** — orchestrates the module-scoped sub-ViewModels (Learning, Planner, Journal, Review, Broker, Settings). Called in `App.xaml.cs` for async initialization after the window shows.

Both are registered as singletons and injected where needed.

### Navigation & Layout State Machines

`MainViewModel` contains two state machines that coordinate navigation:

- **`ModuleStateMachine`** — tracks which module is active; drives `ActiveModule` and whether the side panel should open
- **`LayoutStateMachine`** — manages `SinglePanel` vs `SplitPanel` layout; toggled by the Calculator button or module auto-open logic

Navigate via `NavigateCommand.Execute("ModuleName")` — this calls both state machines and saves/restores per-module screen state through `IStateService`.

### Module Architecture

All modules follow the same pattern: **ViewModel → section in MainWindow.xaml → Service(s)**

Modules are **not** separate XAML files — they are sections within `MainWindow.xaml` shown/hidden by `ActiveModule`. The only standalone window is `LessonEditorWindow.xaml` (used for editing lesson content).

| Module | ViewModel | Purpose |
|--------|-----------|---------|
| **Shell** | `ShellViewModel` | Sub-ViewModel orchestration, async init |
| **Main/Workspace** | `MainViewModel` | Shell state, navigation, browser, clocks |
| **Learning** | `LearningViewModel` | Lesson roadmap, lesson details, progress tracking |
| **Broker** | `BrokerPortalViewModel` | Embedded WebView2 broker portal, bookmarks, browser navigation |
| **Plan** | `TradePlannerViewModel` | Trade plan entry (entry, SL, TP, reason, emotion, etc.) |
| **Journal** | `JournalViewModel` | Trade journal with SQLite storage, screenshots, results, mistakes |
| **Review** | `ReviewViewModel` | Analytics: win rate, mistake patterns, rule breaks, progress |
| **Settings** | `SettingsViewModel` | App settings, layout preferences, checklist config |

### Key Services

**Risk & Rules:**
- `IRiskCalculationService` — Isolated (no WPF), calculates money risk, lot size, margin, RR ratio
- `IRuleGuardService` — Detects violations (missing SL, risk too high, RR too low, revenge trades, etc.)

**Database:**
- `IJournalRepository` — Trade journal persistence (SQLite)
- `ILessonRepository` — Lesson catalogue
- `IScreenshotRecordRepository` — Screenshot metadata

**Browser & Capture:**
- `WebView2BridgeService` — Locates WebView2 controls, normalizes URLs
- `IScreenshotService` — Window screenshot capture
- `ScreenshotRecordRepository` — Attaches screenshots to journal entries

**Settings & State:**
- `IAppSettingsService` + `DpapiEncryptionService` — Settings stored at `Data\settings.secure.json` (DPAPI encrypted)
- `IWorkspaceLayoutService` — Layout state → `Data\workspace-layout.json`
- `IStateService` — Module navigation state machine

**Feedback & Notifications:**
- `IFeedbackService` — Central service for toast notifications and progress overlays. Use `ShowNotification(title, msg, type)`, `ShowProgress(title, pct)`, `HideProgress()`. All user-visible feedback must go through this; never write to UI directly from a ViewModel.

**Other:**
- `IAuditService` — Logs all user actions
- `AiReviewExportService` — Exports lesson + plan + risk + screenshots for AI review
- `RuleGuardService` — Pre-trade rule checking (discipline system)

### Data Storage Locations

```
%LocalAppData%\ForexTradingWorkspace\
├── Data\
│   ├── journal.db                 (SQLite – trades, lessons, screenshots, etc.)
│   ├── settings.secure.json       (DPAPI-encrypted settings)
│   └── workspace-layout.json      (Module layout state)
├── Screenshots\
│   └── yyyy-MM-dd\PAIR\*.png      (Timestamped broker/workspace screenshots)
└── Logs\
    └── workspace-*.log             (Daily Serilog files)
```

### Learning System Flow

**User Journey:**
```
1. Browse lesson catalogue (LearningViewModel)
2. Study lesson content (LessonDetailView)
3. Complete practice exercises (links to lesson progress)
4. Plan a demo trade (TradePlannerViewModel + TradePlan model)
5. Calculate risk (RiskCalculationService + RiskResult)
6. Check rules (RuleGuardService)
7. Execute trade in broker portal (BrokerPortalViewModel)
8. Capture before/after screenshots
9. Journal trade result (JournalViewModel + TradeJournalEntry)
10. Review mistakes & analytics (ReviewViewModel)
```

**Key Models:**
- `Lesson` / `LessonProgress` — Lesson catalogue + user progress
- `TradePlan` — Entry, SL, TP, reason, emotion, linked lesson
- `RiskInput` / `RiskResult` — Risk parameters and calculated outputs
- `RuleCheckResult` — Pre-trade rule violations
- `TradeJournalEntry` — Journal record with before/after screenshots, result, mistakes, review notes

### Common Development Patterns

**Adding a New Module:**
1. Create `ViewModels/<ModuleName>ViewModel.cs` inheriting from `ObservableObject`
2. Add a section inside `MainWindow.xaml` for the module's view (modules are not separate XAML files)
3. Register the ViewModel in [App.xaml.cs](App.xaml.cs) DI container as singleton
4. Inject it into `ShellViewModel` and expose as a property
5. Add the module name to `ModuleStateMachine` transitions

**Adding a Service:**
1. Create interface `Services/I<ServiceName>.cs`
2. Implement `Services/<ServiceName>.cs`
3. Register in [App.xaml.cs](App.xaml.cs) as singleton
4. Inject into ViewModels/other services via constructor
5. Keep business logic (risk, rules, export) **independent of WPF**

**Design System:**
All XAML styling uses `DS_` prefixed resource keys defined in [`Resources/DesignSystem.xaml`](Resources/DesignSystem.xaml). Use these keys for colors, brushes, spacing, and typography — do not hardcode values. Key examples: `DS_BackgroundBrush`, `DS_AccentBrush`, `DS_PrimaryTextBrush`, `DS_Spacing_M`, `DS_SectionTitle16`.

**Binding to UI:**
- Property: `[ObservableProperty] private string? tradeReason;` → `{Binding TradeReason}`
- Command: `[RelayCommand] private void SubmitTrade() { ... }` → `Command="{Binding SubmitTrade}"`
- Navigation: `NavigateCommand.Execute("Plan")` on `MainViewModel`

**Database Access:**
- Implement `IRepository<T>` pattern (see `IJournalRepository`)
- Use `Microsoft.Data.Sqlite` for queries
- All persistence should be in repository classes, not services

### WebView2 & Browser Integration

- `WebView2` controls embedded in XAML (e.g., broker portal)
- `WebView2BridgeService` handles navigation and state
- Screenshots captured via `IScreenshotService`
- **Rule:** Browser scraping is helper-only; never depend on scraped values for risk decisions without manual confirmation

### Testing & Validation

Risk calculations, rule checks, and export logic must be **independent of WPF** to allow unit testing. Services should not reference `System.Windows` or `System.Windows.Controls`.

### Logging

All exceptions and key events logged via Serilog to daily files in `%LocalAppData%\ForexTradingWorkspace\Logs\`. Check logs when debugging startup failures or state issues.

## Key Dependencies

| Package | Purpose |
|---------|---------|
| `Microsoft.Web.WebView2` | Chromium browser embedding |
| `Microsoft.Data.Sqlite` | SQLite database |
| `CommunityToolkit.Mvvm` | MVVM source generators |
| `Microsoft.Extensions.Hosting` | Dependency injection |
| `Serilog` + Sinks.File | Structured logging |
| `System.Security.Cryptography.ProtectedData` | DPAPI encryption |
| `ClosedXML` | Excel export |

## Troubleshooting

- **App won't start:** Check `%LocalAppData%\ForexTradingWorkspace\Logs\`
- **WebView2 errors:** Ensure WebView2 Runtime is installed (usually preinstalled on Windows 10/11)
- **Database locked:** Close all instances of the app
- **Settings not saving:** Verify DPAPI permissions (Windows user must have access to encryption keys)
- **Module navigation broken:** Check `MainViewModel.ActiveModule`, `ModuleStateMachine.CurrentState`, and the visibility bindings in `MainWindow.xaml`
