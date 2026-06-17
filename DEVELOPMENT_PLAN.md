# Forex Demo Learning Workspace Development Plan

## North Star

Build a smooth WPF desktop app that helps a beginner practice inside an XM.AE demo account systematically.

The app must not place trades automatically. It should guide the user through learning, planning, risk calculation, manual demo execution, screenshot capture, journaling, and review.

Main workflow:

```text
Choose lesson -> Understand concept -> Practice task -> Plan demo trade -> Calculate risk -> Complete checklist -> Capture screenshot -> Place manually in XM demo -> Record result -> Review mistake/lesson
```

## Product Rules

1. The app stays calm and uncluttered.
2. One main task per screen.
3. The right assistant panel guides the next step.
4. Risk calculation is required before a trade is marked ready.
5. A screenshot is required before a trade is marked ready.
6. Manual broker execution only.
7. Scraping is helper-only, never trusted for risk-critical decisions.
8. All important actions are saved locally.
9. Every trade should link to a lesson, setup, screenshot, and journal review.

## Target Stack

```text
Platform: WPF .NET 8
Architecture: MVVM + services + state machines
Browser: WebView2
Database: SQLite
Settings: DPAPI encrypted JSON
Logging: Serilog
Screenshots: local app data folder
Broker mode: XM.AE manual demo workflow
```

## Final App Sections

```text
Learn
Broker
Plan
Journal
Review
Settings
```

## Final Screen Layout

```text
+--------------------------------------------------------------------------------+
| Demo Profile | XM.AE | Current Lesson | Daily Risk | Trading Session             |
+----------+-------------------------------------------------------+---------------+
| Sidebar  | Main Workspace                                        | Assistant     |
|          |                                                       |               |
| Learn    | Lesson / Broker Portal / Trade Plan / Journal /Review | Active step   |
| Broker   |                                                       | Risk          |
| Plan     |                                                       | Checklist     |
| Journal  |                                                       | Warnings      |
| Review   |                                                       | Actions       |
| Settings |                                                       |               |
+----------+-------------------------------------------------------+---------------+
```

No permanent crowded bottom panel. Notes, screenshots, and trade details can open as a drawer or detail panel only when needed.

## Required State Machines

### Navigation State

```text
Learn
Broker
Plan
Journal
Review
Settings
```

### Layout State

```text
Normal
AssistantCollapsed
SidebarCollapsed
FocusMode
```

### Lesson State

```text
NotStarted
Studying
Practicing
DemoApplied
Reviewed
Completed
```

### Trade Workflow State

```text
NoTrade
LessonSelected
Planning
RiskCalculated
ChecklistComplete
ScreenshotCaptured
ReadyForManualExecution
ManuallyPlaced
ResultRecorded
Reviewed
Completed
```

This is the most important state machine. It controls what the assistant panel shows and which actions are allowed.

## Target Folder Structure

```text
Modules/
  Shell/
  Learning/
  Broker/
  Planning/
  Risk/
  Journal/
  Review/
  Settings/

Models/
  Learning/
  Trading/
  Risk/
  Journal/
  Analytics/
  Screenshots/

Services/
  Learning/
  Broker/
  Planning/
  Risk/
  Journal/
  Analytics/
  Screenshots/
  Export/
  Persistence/
  Security/

Data/
  schema.sql
  lessons.seed.json

Resources/
  DesignSystem.xaml
  Theme.xaml
```

## Phase 1: Clean App Shell

### Goal

Create a smooth, modular shell and stop growing the current giant `MainViewModel` and giant `MainWindow.xaml`.

### Build

- `ShellViewModel`
- `NavigationState`
- `LayoutState`
- left sidebar
- top status bar
- right assistant host
- center module host
- shared notification/status service

### UI

The first screen should feel calm:

```text
Today
Current Lesson: none selected
Demo Goal: Plan one correct demo trade
Daily Risk Used: 0%

[Continue Learning] [Open Broker] [Plan Trade]
```

### Acceptance Criteria

- App opens without clutter.
- User can switch between Learn, Broker, Plan, Journal, Review, Settings.
- Assistant panel can show module-specific content.
- No new business logic is added to `MainWindow.xaml.cs`.

## Phase 2: Learning Roadmap

### Goal

Add the lesson catalogue as the app backbone.

### Build

- `Lesson`
- `LessonSection`
- `LessonProgress`
- `QuizQuestion`
- `PracticeTask`
- `LearningRoadmapViewModel`
- `LessonDetailViewModel`
- `LessonRepository`
- `Data/lessons.seed.json`

### Initial Catalogue

```text
2. Trading Essentials
3. Fundamental Analysis
4. Technical Analysis
5. Money Management
6. Trading Psychology
7. Trading Strategies
8. More on Trading
9. Avramis Indicators
```

Section 1 can be added later when full lesson details are available.

### Lesson Page Structure

```text
Title
Duration
Beginner explanation
Key terms
Important formulas
Example
Common mistakes
Practice task
Mini quiz
Demo application task
Completion checklist
```

### Acceptance Criteria

- User can browse sections and lessons.
- User can open a lesson.
- User can mark lesson as studying/practicing.
- Lesson progress persists.

## Phase 3: Practice Lab

### Goal

Turn lessons into exercises before demo trading.

### Build

- `PracticeAttempt`
- `PracticeEvaluationService`
- lesson-specific practice task UI

### Examples

```text
Pips/Lots: calculate pip value and lot size
Leverage: calculate margin impact
Slippage: compare expected and actual fill
Trends: identify uptrend/downtrend/range
Support/Resistance: write key level
Risk/Reward: calculate RR
Psychology: record emotion and rule risk
```

### Acceptance Criteria

- A lesson can require a practice task.
- User can submit an answer.
- App stores the attempt.
- App can unlock "Apply in Demo" after practice.

## Phase 4: XM Broker Workspace

### Goal

Create the manual XM.AE demo practice workspace.

### Build

- `BrokerPortalViewModel`
- `BrokerBrowserService`
- XM.AE broker preset
- bookmarks
- WebView2 broker portal
- manual platform notes for MT5/MT4

### XM Preset

```text
Broker: XM.AE
Portal: https://www.xm.ae/
Mode: Manual demo execution
Platforms: MT5, MT4, XM App
Screenshot modes: WebView2, full window, manual import
```

### Acceptance Criteria

- User can open XM.AE portal in the app.
- User can navigate broker pages.
- User can keep the assistant panel open beside the broker portal.
- App clearly says execution is manual.

## Phase 5: Trade Planner

### Goal

Make every demo trade start with a structured plan.

### Build

- `TradePlan`
- `TradePlannerViewModel`
- `TradePlanService`
- `TradeWorkflowState`

### Fields

```text
Linked lesson
Pair/instrument
Direction
Session
Setup type
Market condition
Entry price
Stop loss
Take profit
Risk %
Reason for entry
Invalidation reason
News checked
Emotion before trade
```

### Acceptance Criteria

- User can create a trade plan.
- Plan links to current lesson.
- Plan can be saved as draft.
- Plan cannot move forward if entry, SL, TP, or reason is missing.

## Phase 6: Risk Engine

### Goal

Move risk calculation into a testable engine.

### Build

- `RiskInput`
- `RiskResult`
- `RiskCalculationService`
- `RiskRuleViolation`

### Inputs

```text
Account balance
Risk %
Pair/instrument
Direction
Entry
Stop loss
Take profit
Pip size
Pip value
Lot step
Minimum lot
Daily risk limit
```

### Outputs

```text
Pip distance
Money risk
Lot size
Potential reward
Risk/reward ratio
Margin estimate
Daily risk remaining
Warnings
```

### Acceptance Criteria

- Risk engine does not reference WPF.
- Risk result updates when entry/SL/TP/risk changes.
- App explains each result in beginner language.
- Risk result is saved with the trade plan.

## Phase 7: Rule Guard

### Goal

Prevent random or unsafe demo practice.

### Build

- `TradingRule`
- `RuleCheckResult`
- `RuleGuardService`
- assistant warning UI

### Rules

```text
Stop loss required
Take profit required
Risk <= max allowed
RR >= minimum
Reason required
News check required
Before screenshot required
Max trades per day
Daily loss limit
Recent revenge-trade risk
```

### Acceptance Criteria

- Rules return OK, Warning, or Blocked.
- Blocked rules prevent "Ready for Manual Execution".
- Warnings are clear and not noisy.
- User can see exactly what is missing.

## Phase 8: Screenshot Capture System

### Goal

Make screenshots a core learning record.

### Build

- `ScreenshotRecord`
- `ScreenshotCaptureType`
- `IScreenshotCaptureService`
- `WebViewScreenshotCaptureService`
- `WindowScreenshotCaptureService`
- `ScreenshotRepository`

### Capture Types

```text
BeforeTrade
DuringTrade
AfterTrade
FullWorkspace
LessonPractice
ManualImport
```

### Storage

```text
%LocalAppData%/ForexTradingWorkspace/Screenshots/
  yyyy-MM-dd/
    EURUSD/
      143012_before.png
      143945_during.png
      151230_after.png
```

### Acceptance Criteria

- User can capture broker WebView2 screenshot.
- Screenshot saves locally.
- Screenshot links to current trade plan.
- Journal can show screenshot thumbnail.
- User can retake screenshot.

## Phase 9: Journal

### Goal

Turn every demo trade into a learning record.

### Build

- `TradeJournalEntry`
- `MistakeTag`
- `JournalViewModel`
- `JournalRepository`

### Journal Entry

```text
Trade plan
Risk result
Lesson applied
Before screenshot
After screenshot
Manual result
Profit/loss
Followed plan yes/no
Mistake tags
Lesson learned
Review notes
```

### Mistake Tags

```text
FOMO
Revenge trade
No plan
Bad entry
Moved stop loss
Exited early
Ignored news
Risk too high
No patience
Overtrading
```

### Acceptance Criteria

- User can convert a planned trade into a journal entry.
- User can add result after trade closes.
- User can attach screenshots.
- User can tag mistakes.
- Journal list stays clean and filterable.

## Phase 10: Review Analytics

### Goal

Show what the user is learning and repeating.

### Build

- `AnalyticsService`
- `LearningAnalytics`
- `TradingAnalytics`
- `ReviewDashboardViewModel`

### Metrics

```text
Lessons completed
Practice tasks completed
Demo trades linked to lessons
Win rate
Average RR
Net demo P/L
Best/worst pair
Best/worst setup
Most common mistake
Rule breaks
Psychology patterns
```

### Acceptance Criteria

- Review screen is simple, not crowded.
- User can see top 3 mistakes.
- User can see lesson progress.
- User can see whether rules are improving.

## Phase 11: AI Review Export

### Goal

Make it easy to ask for help with a trade.

### Build

- `AiReviewPackage`
- `AiReviewExportService`

### Export Includes

```text
Current lesson
Trade plan
Risk result
Checklist/rule state
Screenshot paths
Journal notes
Mistake tags
User question
```

### Acceptance Criteria

- User can export one trade review package.
- Package copies to clipboard as JSON/text.
- User can also save it to file.

## Database Plan

Core tables:

```text
Lessons
LessonProgress
PracticeAttempts
TradePlans
RiskResults
RuleCheckResults
JournalEntries
JournalMistakeTags
Screenshots
Settings
AuditEvents
```

Relationships:

```text
Lesson 1 -> many PracticeAttempts
Lesson 1 -> many TradePlans
TradePlan 1 -> 1 RiskResult
TradePlan 1 -> many RuleCheckResults
TradePlan 1 -> many Screenshots
TradePlan 1 -> 1 JournalEntry
JournalEntry 1 -> many MistakeTags
```

## UI Smoothness Rules

1. No screen should show more than one primary action.
2. Hide advanced fields until needed.
3. Use drawers for screenshots and details.
4. Use quiet status messages instead of popups.
5. Use color only for meaning:
   - green = OK/profit
   - red = blocked/loss/risk
   - amber = warning/attention
   - gold = active/selected
6. Avoid emoji navigation.
7. Avoid heavy borders.
8. Use compact forms with clear section headers.
9. Keep analytics out of the broker screen.
10. Always show the next step.

## First Milestone

Build the first useful version with this workflow:

```text
1. Select lesson.
2. Open XM.AE broker portal.
3. Create trade plan.
4. Calculate risk.
5. Complete checklist.
6. Capture before screenshot.
7. Mark trade ready for manual execution.
8. Save journal draft.
```

Done means:

```text
The app can guide one complete planned demo trade without clutter.
```

## Second Milestone

Add post-trade review:

```text
1. Enter trade result.
2. Capture after screenshot.
3. Add mistake tags.
4. Write lesson learned.
5. Mark reviewed.
```

Done means:

```text
The app turns one demo trade into a complete learning record.
```

## Third Milestone

Add analytics:

```text
1. Lesson progress.
2. Trade stats.
3. Mistake stats.
4. Rule-break stats.
```

Done means:

```text
The app can show what the user should improve next.
```

## Development Order

```text
1. Shell and layout
2. State machines
3. Learning roadmap
4. Trade planner
5. Risk engine
6. Rule guard
7. Broker portal
8. Screenshot system
9. Journal
10. Review analytics
11. AI export
```

## What Not To Build First

Do not start with:

```text
Broker API automation
Auto trade execution
Complex scraping
Advanced indicators
Crowded dashboards
Mobile app
Cloud sync
Subscriptions
Strategy scanner
```

These can come later after the core learning workflow is strong.

