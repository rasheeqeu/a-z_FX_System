# Forex Demo Learning Workspace Architecture

## Product Goal

Build a local Windows desktop app that helps a beginner learn forex while practicing inside a broker demo account. The app does not place trades automatically. It teaches concepts, guides trade planning, calculates risk, records screenshots, journals decisions, and reviews mistakes.

Core loop:

```text
Learn concept -> Practice exercise -> Plan demo trade -> Calculate risk -> Checklist -> Manual broker execution -> Screenshot/journal -> Review
```

## Platform Decision

Use the existing stack:

```text
WPF + .NET 8
CommunityToolkit.Mvvm
WebView2
SQLite
DPAPI encrypted settings
Serilog
Local screenshot storage
```

Reason: the app needs a strong desktop workspace, embedded broker portal, local files, local database, screenshot capture, and privacy. A web app or mobile app adds complexity before the learning system is proven.

## High-Level Layout

```text
+--------------------------------------------------------------------------------+
| Top Bar: profile, session clock, daily risk, current lesson, quick actions       |
+----------+------------------------------------------------------+----------------+
| Left Nav | Center Workspace                                     | Right Assistant |
|          |                                                      |                 |
| Learn    | Broker portal / TradingView / lesson content         | Trade planner   |
| Broker   |                                                      | Risk calculator |
| Plan     | WebView2 for broker demo account                     | Checklist       |
| Journal  |                                                      | Warnings        |
| Review   |                                                      | Lesson help     |
| Settings |                                                      |                 |
+----------+------------------------------------------------------+----------------+
| Bottom Panel: planned trade, notes, screenshots, open/recent journal entries      |
+--------------------------------------------------------------------------------+
```

## Main Modules

### 1. Shell

Owns global layout only.

Responsibilities:
- navigation
- active workspace module
- split panel state
- global profile
- top bar state
- app notifications

Should not contain trading logic, calculator logic, journal logic, or broker scraping logic.

Suggested files:

```text
ViewModels/ShellViewModel.cs
Views/ShellView.xaml
Services/NavigationService.cs
Models/NavigationItem.cs
```

### 2. Learning Roadmap

The backbone of the app. Every lesson becomes a guided learning unit.

Lesson status flow:

```text
NotStarted -> Studying -> Practicing -> DemoApplied -> Reviewed -> Completed
```

Each lesson should contain:
- section
- lesson number
- title
- duration
- beginner explanation
- key terms
- formulas
- examples
- common mistakes
- practice exercises
- quiz questions
- demo application task
- completion checklist

Suggested files:

```text
ViewModels/LearningRoadmapViewModel.cs
ViewModels/LessonDetailViewModel.cs
Models/Lesson.cs
Models/LessonSection.cs
Models/LessonProgress.cs
Models/QuizQuestion.cs
Services/LessonRepository.cs
Data/lessons.seed.json
```

### 3. Practice Lab

Turns lessons into action.

Examples:
- pips/lots: calculate pip value, lot size, risk amount
- leverage: compare margin usage
- liquidity/slippage: simulate expected vs filled price
- trends: identify uptrend, downtrend, range
- support/resistance: record key level
- Fibonacci: record retracement zone
- risk/reward: calculate RR and expectancy
- psychology: record emotion and rule quality

Suggested files:

```text
ViewModels/PracticeLabViewModel.cs
Models/PracticeTask.cs
Models/PracticeAttempt.cs
Services/PracticeEvaluationService.cs
```

### 4. Broker Portal

Embeds the broker demo account and chart websites.

Responsibilities:
- WebView2 broker portal
- bookmarks
- browser navigation
- screenshot capture
- optional visible-page helper extraction

Important rule: scraping is helper-only. The app must never depend on scraped page values for risk-critical decisions without manual confirmation.

Suggested files:

```text
ViewModels/BrokerPortalViewModel.cs
Views/BrokerPortalView.xaml
Services/BrokerBrowserService.cs
Services/ScreenshotService.cs
Models/BrokerBookmark.cs
```

### 4A. Screenshot Capture System

Screenshot capture must be a core learning feature. Every important demo action should be visually recorded so the user can review what they saw, what they planned, and what happened.

Capture types:
- broker portal screenshot
- full app workspace screenshot
- selected region screenshot
- before-trade screenshot
- during-trade screenshot
- after-trade screenshot
- lesson/practice screenshot

Required metadata:
- screenshot id
- linked lesson id
- linked trade plan id
- linked journal entry id
- capture type
- broker/profile
- pair/instrument
- timestamp
- file path
- notes

Storage pattern:

```text
%LocalAppData%/ForexTradingWorkspace/Screenshots/
  yyyy-MM-dd/
    EURUSD/
      143012_before.png
      143945_during.png
      151230_after.png
```

Screenshot service responsibilities:
- capture WebView2 content when the broker/chart is inside the app
- capture the full WPF window when the user needs the whole workspace
- allow manual screenshot import if browser capture fails
- save files with stable names
- write screenshot records to SQLite
- link screenshots to lesson progress, trade plans, and journal entries

Suggested files:

```text
Services/Screenshots/IScreenshotCaptureService.cs
Services/Screenshots/WebViewScreenshotCaptureService.cs
Services/Screenshots/WindowScreenshotCaptureService.cs
Services/Screenshots/ScreenshotRepository.cs
Models/ScreenshotRecord.cs
Models/ScreenshotCaptureType.cs
```

Minimum first version:

```text
1. Capture broker WebView2 screenshot.
2. Save screenshot to local app data.
3. Attach screenshot to current trade plan.
4. Show thumbnail in the journal.
5. Let user retake screenshot if wrong.
```

### 5. Trade Planner

Creates a structured plan before a demo trade.

Fields:
- linked lesson
- pair/instrument
- direction
- session
- setup type
- market condition
- entry price
- stop loss
- take profit
- reason for entry
- invalidation reason
- news checked
- emotion before trade
- screenshots

Suggested files:

```text
ViewModels/TradePlannerViewModel.cs
Models/TradePlan.cs
Models/TradeSetupType.cs
Models/TradingSession.cs
Services/TradePlanService.cs
```

### 6. Risk Engine

Calculates and explains risk.

Outputs:
- money risk
- lot size
- potential reward
- risk/reward ratio
- pip distance
- margin estimate
- daily risk remaining
- rule warnings

Must be isolated from WPF so it can be unit tested.

Suggested files:

```text
Services/RiskCalculationService.cs
Models/RiskInput.cs
Models/RiskResult.cs
Models/RiskRuleViolation.cs
```

### 7. Rule Guard

Protects learning discipline before demo execution.

Warnings:
- no stop loss
- risk above allowed limit
- RR below rule
- no trade reason
- lesson concept not applied
- daily trade count exceeded
- daily loss limit reached
- news unchecked
- revenge-trade risk after recent loss

Suggested files:

```text
Services/RuleGuardService.cs
Models/TradingRule.cs
Models/RuleCheckResult.cs
Models/RuleSeverity.cs
```

### 8. Journal

Records every planned/demo trade and its review.

Journal entry should include:
- trade plan
- risk result
- lesson applied
- before screenshot
- after screenshot
- broker/manual result
- profit/loss
- followed plan yes/no
- mistake tags
- lesson learned
- review notes

Suggested files:

```text
ViewModels/JournalViewModel.cs
Models/TradeJournalEntry.cs
Models/MistakeTag.cs
Services/JournalRepository.cs
```

### 9. Review Analytics

Shows learning quality and trading behavior.

Metrics:
- lessons completed
- concepts practiced
- demo trades per lesson
- win rate
- average RR
- net demo P/L
- max drawdown
- best/worst pair
- best/worst setup
- most common mistake
- rule breaks
- psychology patterns

Suggested files:

```text
ViewModels/ReviewDashboardViewModel.cs
Services/AnalyticsService.cs
Models/LearningAnalytics.cs
Models/TradingAnalytics.cs
```

### 10. AI Review Export

Exports structured context for external AI review.

Export package:
- lesson currently applied
- trade plan
- calculator result
- checklist state
- screenshots paths
- journal notes
- mistakes selected

Suggested files:

```text
Services/AiReviewExportService.cs
Models/AiReviewPackage.cs
```

## Lesson Catalogue Model

Initial sections:

```text
1. Introduction
2. Trading Essentials
3. Fundamental Analysis
4. Technical Analysis
5. Money Management
6. Trading Psychology
7. Trading Strategies
8. More on Trading
9. Avramis Indicators
```

Example lesson JSON:

```json
{
  "id": "2.1",
  "sectionNumber": 2,
  "sectionTitle": "Trading Essentials",
  "title": "Understanding Pips, Lots",
  "duration": "09:28",
  "summary": "",
  "keyTerms": [],
  "formulas": [],
  "examples": [],
  "commonMistakes": [],
  "practiceTasks": [],
  "quiz": [],
  "demoApplication": "",
  "checklist": []
}
```

## Database Tables

Use SQLite with migrations/schema scripts.

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

Important relationships:

```text
Lesson 1 -> many PracticeAttempts
Lesson 1 -> many TradePlans
TradePlan 1 -> 1 RiskResult
TradePlan 1 -> many RuleCheckResults
TradePlan 1 -> 1 JournalEntry
JournalEntry 1 -> many Screenshots
JournalEntry 1 -> many MistakeTags
```

## MVVM Rules

Use these rules to keep the app strong:

- Views contain XAML layout only.
- Code-behind is allowed only for WPF/WebView2 events that cannot bind cleanly.
- ViewModels coordinate screen state and commands.
- Services contain business logic.
- Repositories contain persistence.
- Risk calculations must not reference WPF.
- Rule checks must not reference WPF.
- Analytics must not reference WPF.
- Browser automation/scraping must be isolated behind a service.

## Recommended Folder Structure

```text
Modules/
  Shell/
  Learning/
  Practice/
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

Services/
  Learning/
  Broker/
  Planning/
  Risk/
  Journal/
  Analytics/
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

## Implementation Phases

### Phase 1: Architecture Cleanup

Goal: prepare the current app for modules.

- create ShellViewModel
- create module view models
- move calculator logic into RiskCalculationService
- move browser actions into BrokerBrowserService
- split MainWindow.xaml into user controls

### Phase 2: Learning Roadmap

Goal: store and display the lesson catalogue.

- add lesson models
- add lesson seed JSON
- add roadmap UI
- add lesson detail UI
- add lesson progress state

### Phase 3: Planner + Risk Engine

Goal: every demo trade starts with a plan.

- create TradePlan model
- create RiskInput/RiskResult
- create RuleGuardService
- show order ticket summary

### Phase 4: Broker Demo Workspace

Goal: broker portal and assistant side-by-side.

- embed broker portal
- bookmarks
- screenshot capture
- manual confirmation fields

### Phase 5: Journal

Goal: every demo trade becomes a learning record.

- save plan to SQLite
- attach screenshots
- add result entry
- add mistake tags
- link journal entry to lesson

### Phase 6: Review

Goal: show progress and mistakes clearly.

- learning progress analytics
- trading performance analytics
- mistake analytics
- rule-break analytics

### Phase 7: AI Review Export

Goal: make it easy to ask for help.

- export selected lesson, plan, risk, screenshots, and notes to JSON
- copy package to clipboard
- optionally save export file

## First Strong Milestone

Build this first:

```text
The user cannot mark a demo trade as ready unless it has:
1. linked lesson or concept
2. entry, stop loss, take profit
3. calculated risk
4. completed checklist
5. screenshot captured
6. written reason for the trade
```

This creates the discipline system that makes the app valuable.
