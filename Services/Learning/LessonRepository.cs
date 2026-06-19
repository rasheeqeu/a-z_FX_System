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

    private static readonly string VideosFolder =
        @"C:\Users\rashe\learning\xm-downloader\downloads";

    private static string? VideoFile(string filename)
    {
        var path = Path.Combine(VideosFolder, filename);
        return File.Exists(path) ? path : null;
    }

    private static IReadOnlyList<LessonSection> SeedLessons()
    {
        var definitions = new (int Section, string SectionTitle, string Id, string Title, string Duration, string VideoFile)[]
        {
            (1, "Introduction", "1.1", "Introduction to the Financial Markets", "05:00", "lesson-02-Lesson-11-Introduction-to-the-Financial-Markets.mp4"),
            (1, "Introduction", "1.2", "Introduction to Forex", "05:00", "lesson-03-Lesson-12-Introduction-to-Forex.mp4"),
            (1, "Introduction", "1.3", "Introduction to Shares", "05:00", "lesson-04-Lesson-13-Introduction-to-Shares.mp4"),
            (1, "Introduction", "1.4", "Introduction to CFDs", "05:00", "lesson-05-Lesson-14-Introduction-to-CFDs.mp4"),
            (1, "Introduction", "1.5", "Introduction to Cryptocurrencies", "05:00", "lesson-06-Lesson-15-Introduction-to-Cryptocurrencies.mp4"),
            (2, "Trading Essentials", "2.1", "Understanding Pips, Lots", "09:28", "lesson-07-Lesson-21-Understanding-Pips-Lots-and-Position-Size.mp4"),
            (2, "Trading Essentials", "2.2", "Understanding Leverage", "06:33", "lesson-08-Lesson-22-Understanding-Leverage-and-Margin.mp4"),
            (2, "Trading Essentials", "2.3", "Liquidity, Slippage and Spread", "04:52", "lesson-09-Lesson-23-Liquidity-Slippage-and-Swaps.mp4"),
            (3, "Fundamental Analysis", "3.1", "Fundamental Analysis", "04:23", "lesson-10-Lesson-31-Fundamental-Analysis.mp4"),
            (3, "Fundamental Analysis", "3.2", "Macroeconomic Analysis", "10:03", "lesson-11-Lesson-32-Macroeconomic-Analysis.mp4"),
            (3, "Fundamental Analysis", "3.3", "Microeconomic Analysis", "06:47", "lesson-12-Lesson-33-Microeconomic-Analysis.mp4"),
            (4, "Technical Analysis", "4.1", "Principles of Technical Analysis", "02:47", "lesson-13-Lesson-41-Principles-of-Technical-Analysis.mp4"),
            (4, "Technical Analysis", "4.2", "Chart Construction", "04:34", "lesson-14-Lesson-42-Chart-Construction.mp4"),
            (4, "Technical Analysis", "4.3", "Basic Bar and Candlestick Charts", "08:43", "lesson-15-Lesson-43-Basic-Bar-and-Candlestick-Formations.mp4"),
            (4, "Technical Analysis", "4.4", "Basic Concepts of Trends", "04:11", "lesson-16-Lesson-44-Basic-Concepts-of-Trends.mp4"),
            (4, "Technical Analysis", "4.5", "Trend Lines and Channels", "09:34", "lesson-17-Lesson-45-Trend-Lines-and-Channels.mp4"),
            (4, "Technical Analysis", "4.6A", "Supports and Resistance", "05:59", "lesson-18-Lesson-46-A-Supports-and-Resistances.mp4"),
            (4, "Technical Analysis", "4.6B", "Fibonacci for Support and Resistance", "09:15", "lesson-19-Lesson-46-B-Fibonacci-for-Support-and-Resistance.mp4"),
            (4, "Technical Analysis", "4.7", "Trend Reversal Patterns", "07:19", "lesson-20-Lesson-47-Trend-Reversal-Patterns.mp4"),
            (4, "Technical Analysis", "4.8", "Major Continuation Patterns", "05:10", "lesson-21-Lesson-48-Major-Continuation-Patterns.mp4"),
            (4, "Technical Analysis", "4.9", "Moving Averages and MACD", "11:24", "lesson-22-Lesson-49-Moving-Averages-and-MACD.mp4"),
            (4, "Technical Analysis", "4.10", "Momentum Oscillators", "11:14", "lesson-23-Lesson-410-Momentum-Oscillators.mp4"),
            (4, "Technical Analysis", "4.11", "Oscillator Analysis", "09:27", "lesson-24-Lesson-411-Oscillator-Analysis.mp4"),
            (4, "Technical Analysis", "4.12", "Volatility Indicators", "08:16", "lesson-25-Lesson-412-Volatility-Indicators.mp4"),
            (4, "Technical Analysis", "4.13", "Average Directional Index (ADX)", "05:44", "lesson-26-Lesson-413-Average-Directional-Index-ADX.mp4"),
            (4, "Technical Analysis", "4.14", "Ichimoku Kinko Hyo", "06:53", "lesson-27-Lesson-414-Ichimoku-Kinko-Hyo.mp4"),
            (4, "Technical Analysis", "4.15", "Parabolic SAR", "03:36", "lesson-28-Lesson-415-Parabolic-SAR.mp4"),
            (4, "Technical Analysis", "4.16", "Avramis River Indicator", "06:27", "lesson-29-Lesson-416-Avramis-River-Indicator.mp4"),
            (4, "Technical Analysis", "4.17", "Other Charting Techniques", "06:10", "lesson-30-Lesson-417-Other-Charting-Techniques.mp4"),
            (5, "Money Management", "5.1", "Money Management", "08:37", "lesson-31-Lesson-51-Money-Management.mp4"),
            (5, "Money Management", "5.2", "Risk, Reward and Expectancy", "05:27", "lesson-32-Lesson-52-Risk-Reward-and-Expectancy.mp4"),
            (6, "Trading Psychology", "6.1", "Trading Psychology", "12:02", "lesson-33-Lesson-61-Trading-Psychology.mp4"),
            (7, "Trading Strategies", "7.0", "Filtering Entry Signals", "03:59", "lesson-34-Lesson-70-Filtering-Entry-Signals.mp4"),
            (7, "Trading Strategies", "7.1", "Moving Average Ribbon Signals", "13:01", "lesson-35-Lesson-71-Moving-Average-Ribbon-Signals.mp4"),
            (7, "Trading Strategies", "7.2", "Ichimoku Kinko Hyo Signals", "08:28", "lesson-36-Lesson-72-Ichimoku-Kinko-Hyo-Signals.mp4"),
            (7, "Trading Strategies", "7.3", "ADX and Parabolic SAR Signals", "09:33", "lesson-37-Lesson-73-ADX-and-Parabolic-SAR-Signals.mp4"),
            (7, "Trading Strategies", "7.4", "Bollinger Bands Signals", "10:36", "lesson-38-Lesson-74-Bollinger-Bands-Signals.mp4"),
            (8, "More on Trading", "8.0", "Trading the News", "11:13", "lesson-39-Lesson-80-Trading-the-News.mp4"),
            (8, "More on Trading", "8.1", "Elliott Wave Theory", "10:55", "lesson-40-Lesson-81-Elliott-Wave-Theory.mp4"),
            (8, "More on Trading", "8.2", "Harmonic Patterns", "17:56", "lesson-41-Lesson-82-Harmonic-Patterns.mp4"),
            (8, "More on Trading", "8.3", "Trader Evolution Stages", "20:05", "lesson-42-Lesson-83-Trader-Evolution-Stages.mp4"),
            (8, "More on Trading", "8.4", "Algorithmic Trading", "12:11", "lesson-43-Lesson-84-Algorithmic-Trading.mp4"),
            (8, "More on Trading", "8.5", "Trading Plan", "11:36", "lesson-44-Lesson-85-Trading-Plan.mp4"),
            (9, "Avramis Indicators", "9.1", "Avramis Swing Indicator", "04:37", "lesson-45-Lesson-91-Avramis-Swing-Indicator.mp4"),
            (9, "Avramis Indicators", "9.2", "Avramis Reversal Candle", "11:19", "lesson-46-Lesson-92-Avramis-Reversal-Candle.mp4"),
            (9, "Avramis Indicators", "9.3", "Avramis River", "08:33", "lesson-47-Lesson-93-Avramis-River.mp4"),
            (9, "Avramis Indicators", "9.4", "Avramis Analyzer", "25:51", "lesson-48-Lesson-94-Avramis-Analyzer.mp4"),
        };

        return definitions
            .GroupBy(x => new { x.Section, x.SectionTitle })
            .Select(group => new LessonSection
            {
                SectionNumber = group.Key.Section,
                Title = group.Key.SectionTitle,
                Lessons = group.Select(x => CreateLesson(x.Section, x.SectionTitle, x.Id, x.Title, x.Duration, x.VideoFile)).ToList()
            })
            .ToList();
    }

    private static Lesson CreateLesson(int section, string sectionTitle, string id, string title, string duration, string videoFile)
    {
        return new Lesson
        {
            Id = id,
            SectionNumber = section,
            SectionTitle = sectionTitle,
            Title = title,
            Duration = duration,
            VideoPath = VideoFile(videoFile),
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
