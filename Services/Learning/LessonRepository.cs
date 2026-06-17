using System.Text.Json;
using ForexTradingWorkspace.Models.Learning;

namespace ForexTradingWorkspace.Services.Learning;

public sealed class LessonRepository : ILessonRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly string _progressFile = Path.Combine(AppPaths.DataPath, "lesson-progress.json");
    private readonly string _attemptsFile = Path.Combine(AppPaths.DataPath, "practice-attempts.json");

    private static readonly string LessonsFile = Path.Combine(AppPaths.DataPath, "lessons.txt");

    public async Task<IReadOnlyList<LessonSection>> LoadSectionsAsync()
    {
        if (File.Exists(LessonsFile))
        {
            var text = await File.ReadAllTextAsync(LessonsFile);
            var parsed = LessonTextParser.Parse(text);
            if (parsed.Count > 0 && parsed.Any(s => s.Lessons.Count > 0))
                return parsed;
        }

        await WriteTemplateIfMissingAsync();
        return SeedLessons();
    }

    private static async Task WriteTemplateIfMissingAsync()
    {
        if (File.Exists(LessonsFile)) return;
        Directory.CreateDirectory(AppPaths.DataPath);
        await File.WriteAllTextAsync(LessonsFile, LessonTemplate);
    }

    public async Task<Dictionary<string, LessonProgress>> LoadProgressAsync()
    {
        if (!File.Exists(_progressFile))
        {
            return [];
        }

        var json = await File.ReadAllTextAsync(_progressFile);
        var items = JsonSerializer.Deserialize<List<LessonProgress>>(json, JsonOptions) ?? [];
        return items.ToDictionary(x => x.LessonId, StringComparer.OrdinalIgnoreCase);
    }

    public async Task SaveProgressAsync(IEnumerable<LessonProgress> progress)
    {
        Directory.CreateDirectory(AppPaths.DataPath);
        var json = JsonSerializer.Serialize(progress.OrderBy(x => x.LessonId).ToList(), JsonOptions);
        await File.WriteAllTextAsync(_progressFile, json);
    }

    public async Task<IReadOnlyList<PracticeAttempt>> LoadPracticeAttemptsAsync()
    {
        if (!File.Exists(_attemptsFile))
        {
            return [];
        }

        var json = await File.ReadAllTextAsync(_attemptsFile);
        return JsonSerializer.Deserialize<List<PracticeAttempt>>(json, JsonOptions) ?? [];
    }

    public async Task SavePracticeAttemptAsync(PracticeAttempt attempt)
    {
        Directory.CreateDirectory(AppPaths.DataPath);
        var attempts = (await LoadPracticeAttemptsAsync()).ToList();
        attempts.Add(attempt);
        var json = JsonSerializer.Serialize(attempts, JsonOptions);
        await File.WriteAllTextAsync(_attemptsFile, json);
    }

    private const string LessonTemplate = """
# ============================================================
# MY FOREX LEARNING PLAN — lessons.txt
# ============================================================
# Add your lessons here as you study your course.
# App reloads this file every time it starts.
# Lines starting with # are comments (ignored).
#
# FORMAT:
#
#   SECTION: Section Name
#
#   LESSON: 2.1 Lesson Title
#   DURATION: 09:28                          (optional)
#   CONTENT:
#   Paste or write your actual course content here.
#   This is what the AI compares your explanation against.
#   Can span multiple lines — add as much detail as you want.
#
#   TERMS: pip, lot, spread, leverage
#   PRACTICE: What you must do to practice this concept in a demo trade.
#   MISTAKES: common mistake 1, common mistake 2
#   ---
#
# HOW THE AI EVALUATION USES THIS FILE:
#   When you click "Copy Prompt for AI", the app sends:
#     - Your CONTENT (actual course material) as reference
#     - Your written explanation
#   The AI checks: does your explanation match the course content?
#   The more detail you put in CONTENT, the stricter and more
#   accurate the evaluation becomes.
#
# TIPS:
#   - Add one lesson at a time as you study your course videos.
#   - Paste key points directly from your course notes.
#   - Use CONTENT: for course material, PRACTICE: for your task.
# ============================================================

SECTION: Trading Essentials

LESSON: 2.1 Understanding Pips, Lots
DURATION: 09:28
CONTENT:
A pip (percentage in point) is the smallest standard price move in forex.
For most pairs like EURUSD, GBPUSD: 1 pip = 0.0001.
For JPY pairs like USDJPY: 1 pip = 0.01.
A lot is a standardised unit of currency trade size:
  Standard lot = 100,000 units
  Mini lot     = 10,000 units
  Micro lot    = 1,000 units
Pip value depends on lot size and the pair. For EURUSD standard lot: 1 pip = $10.
Knowing pip value is essential before calculating risk or lot size.

TERMS: pip, lot, micro lot, mini lot, standard lot, pip value
PRACTICE: Calculate the pip value for a 0.01 lot EURUSD trade. Then explain what happens to your dollar loss if stop loss is 20 pips away.
MISTAKES: Not knowing pip value before trading, confusing lot size with units, ignoring pip value when setting stop loss
---

# Add your next lesson below as you progress through the course:
# LESSON: 2.2 Understanding Leverage
# DURATION: 06:33
# CONTENT:
# (paste your course notes here)
# ...
# ---

""";

    private static IReadOnlyList<LessonSection> SeedLessons()
    {
        var definitions = new (int Section, string SectionTitle, string Id, string Title, string Duration)[]
        {
            (2, "Trading Essentials", "2.1", "Understanding Pips, Lots", "09:28"),
            (2, "Trading Essentials", "2.2", "Understanding Leverage", "06:33"),
            (2, "Trading Essentials", "2.3", "Liquidity, Slippage and Spread", "04:52"),
            (3, "Fundamental Analysis", "3.1", "Fundamental Analysis", "04:23"),
            (3, "Fundamental Analysis", "3.2", "Macroeconomic Analysis", "10:03"),
            (3, "Fundamental Analysis", "3.3", "Microeconomic Analysis", "06:47"),
            (4, "Technical Analysis", "4.1", "Principles of Technical Analysis", "02:47"),
            (4, "Technical Analysis", "4.2", "Chart Construction", "04:34"),
            (4, "Technical Analysis", "4.3", "Basic Bar and Candlestick Charts", "08:43"),
            (4, "Technical Analysis", "4.4", "Basic Concepts of Trends", "04:11"),
            (4, "Technical Analysis", "4.5", "Trend Lines and Channels", "09:34"),
            (4, "Technical Analysis", "4.6A", "Supports and Resistance", "05:59"),
            (4, "Technical Analysis", "4.6B", "Fibonacci for Support and Resistance", "09:15"),
            (4, "Technical Analysis", "4.7", "Trend Reversal Patterns", "07:19"),
            (4, "Technical Analysis", "4.8", "Major Continuation Patterns", "05:10"),
            (4, "Technical Analysis", "4.9", "Moving Averages and MACD", "11:24"),
            (4, "Technical Analysis", "4.10", "Momentum Oscillators", "11:14"),
            (4, "Technical Analysis", "4.11", "Oscillator Analysis", "09:27"),
            (4, "Technical Analysis", "4.12", "Volatility Indicators", "08:16"),
            (4, "Technical Analysis", "4.13", "Average Directional Index (ADX)", "05:44"),
            (4, "Technical Analysis", "4.14", "Ichimoku Kinko Hyo", "06:53"),
            (4, "Technical Analysis", "4.15", "Parabolic SAR", "03:36"),
            (4, "Technical Analysis", "4.16", "Avramis River Indicator", "06:27"),
            (4, "Technical Analysis", "4.17", "Other Charting Techniques", "06:10"),
            (5, "Money Management", "5.1", "Money Management", "08:37"),
            (5, "Money Management", "5.2", "Risk, Reward and Expectancy", "05:27"),
            (6, "Trading Psychology", "6.1", "Trading Psychology", "12:02"),
            (7, "Trading Strategies", "7.0", "Filtering Entry Signals", "03:59"),
            (7, "Trading Strategies", "7.1", "Moving Average Ribbon Signals", "13:01"),
            (7, "Trading Strategies", "7.2", "Ichimoku Kinko Hyo Signals", "08:28"),
            (7, "Trading Strategies", "7.3", "ADX and Parabolic SAR Signals", "09:33"),
            (7, "Trading Strategies", "7.4", "Bollinger Bands Signals", "10:36"),
            (8, "More on Trading", "8.0", "Trading the News", "11:13"),
            (8, "More on Trading", "8.1", "Elliott Wave Theory", "10:55"),
            (8, "More on Trading", "8.2", "Harmonic Patterns", "17:56"),
            (8, "More on Trading", "8.3", "Trader Evolution Stages", "20:05"),
            (8, "More on Trading", "8.4", "Algorithmic Trading", "12:11"),
            (8, "More on Trading", "8.5", "Trading Plan", "11:36"),
            (9, "Avramis Indicators", "9.1", "Avramis Swing Indicator", "04:37"),
            (9, "Avramis Indicators", "9.2", "Avramis Reversal Candle", "11:19"),
            (9, "Avramis Indicators", "9.3", "Avramis River", "08:33"),
            (9, "Avramis Indicators", "9.4", "Avramis Analyzer", "25:51")
        };

        return definitions
            .GroupBy(x => new { x.Section, x.SectionTitle })
            .Select(group => new LessonSection
            {
                SectionNumber = group.Key.Section,
                Title = group.Key.SectionTitle,
                Lessons = group.Select(x => CreateLesson(x.Section, x.SectionTitle, x.Id, x.Title, x.Duration)).ToList()
            })
            .ToList();
    }

    private static Lesson CreateLesson(int section, string sectionTitle, string id, string title, string duration)
    {
        return new Lesson
        {
            Id = id,
            SectionNumber = section,
            SectionTitle = sectionTitle,
            Title = title,
            Duration = duration,
            Summary = $"Study {title}, write the idea in your own words, then apply it to one low-risk XM demo plan.",
            KeyTerms = ["risk", "setup", "confirmation"],
            CommonMistakes = ["Skipping the plan", "Taking a trade without a clear reason", "Ignoring risk before entry"],
            PracticeTasks =
            [
                new PracticeTask
                {
                    Id = $"{id}-practice",
                    Prompt = $"Explain how {title} affects a demo trade before execution.",
                    ExpectedAction = "Write a short answer, then apply the concept in the trade planner."
                }
            ],
            DemoApplication = "Create one demo trade plan where this lesson concept is explicitly written in the reason field.",
            Checklist = ["I can explain the concept", "I completed a practice note", "I linked it to a trade plan"]
        };
    }
}
