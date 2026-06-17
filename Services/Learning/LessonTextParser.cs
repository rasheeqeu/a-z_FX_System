using ForexTradingWorkspace.Models.Learning;

namespace ForexTradingWorkspace.Services.Learning;

/// <summary>
/// Parses a plain-text lessons file into LessonSection / Lesson objects.
///
/// Format:
///   SECTION: Section Title
///
///   LESSON: 1.1 Lesson Title
///   DURATION: 10:00          (optional)
///   Your summary text here. Can span multiple lines.
///
///   TERMS: pip, lot, spread
///   PRACTICE: Write a short note explaining how this concept applies to a trade.
///   ---
///
/// Rules:
///   - Lines starting with # are comments and are ignored.
///   - SECTION: starts a new section. All lessons below it belong to that section.
///   - LESSON: starts a new lesson. The value after the colon is the title.
///     If it starts with a number (e.g. "2.1 Pips") the number becomes the ID.
///   - DURATION: optional. Sets the lesson duration label.
///   - TERMS: comma-separated key terms.
///   - PRACTICE: the practice task prompt. Can span multiple lines until ---.
///   - --- separates lessons (optional but recommended for readability).
///   - All other non-empty lines after LESSON: are the summary.
/// </summary>
public static class LessonTextParser
{
    public static IReadOnlyList<LessonSection> Parse(string text)
    {
        var sections = new List<LessonSection>();
        LessonSection? currentSection = null;
        LessonBuilder? currentLesson = null;
        var mode = ParseMode.Summary;

        foreach (var rawLine in text.Split('\n'))
        {
            var line = rawLine.TrimEnd();

            if (line.StartsWith('#')) continue;

            if (line.StartsWith("SECTION:", StringComparison.OrdinalIgnoreCase))
            {
                FlushLesson(ref currentLesson, currentSection);
                currentSection = new LessonSection
                {
                    SectionNumber = sections.Count + 2,
                    Title = line["SECTION:".Length..].Trim(),
                    Lessons = []
                };
                sections.Add(currentSection);
                currentLesson = null;
                mode = ParseMode.Summary;
                continue;
            }

            if (line.StartsWith("LESSON:", StringComparison.OrdinalIgnoreCase))
            {
                FlushLesson(ref currentLesson, currentSection);
                currentSection ??= CreateDefaultSection(sections);

                var raw = line["LESSON:".Length..].Trim();
                var (id, title) = ParseLessonIdTitle(raw, currentSection, currentLesson);
                currentLesson = new LessonBuilder { Id = id, Title = title };
                mode = ParseMode.Summary;
                continue;
            }

            if (currentLesson is null) continue;

            if (line.StartsWith("DURATION:", StringComparison.OrdinalIgnoreCase))
            {
                currentLesson.Duration = line["DURATION:".Length..].Trim();
                continue;
            }

            if (line.StartsWith("TERMS:", StringComparison.OrdinalIgnoreCase))
            {
                var terms = line["TERMS:".Length..].Trim();
                currentLesson.KeyTerms = [.. terms.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)];
                mode = ParseMode.Summary;
                continue;
            }

            if (line.StartsWith("CONTENT:", StringComparison.OrdinalIgnoreCase))
            {
                // CONTENT: is an alias that marks detailed course notes (multi-line ok)
                // It becomes the lesson Summary used in AI evaluation comparison
                var contentText = line["CONTENT:".Length..].Trim();
                if (!string.IsNullOrEmpty(contentText))
                    currentLesson.SummaryLines.Add(contentText);
                mode = ParseMode.Content;
                continue;
            }

            if (line.StartsWith("PRACTICE:", StringComparison.OrdinalIgnoreCase))
            {
                var practiceText = line["PRACTICE:".Length..].Trim();
                if (!string.IsNullOrEmpty(practiceText))
                    currentLesson.PracticeLines.Add(practiceText);
                mode = ParseMode.Practice;
                continue;
            }

            if (line.StartsWith("MISTAKES:", StringComparison.OrdinalIgnoreCase))
            {
                var mistakes = line["MISTAKES:".Length..].Trim();
                currentLesson.CommonMistakes = [.. mistakes.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)];
                mode = ParseMode.Summary;
                continue;
            }

            if (line.TrimStart().StartsWith("---"))
            {
                mode = ParseMode.Summary;
                continue;
            }

            if (mode == ParseMode.Practice)
            {
                if (!string.IsNullOrWhiteSpace(line))
                    currentLesson.PracticeLines.Add(line.Trim());
            }
            else if (mode == ParseMode.Content)
            {
                if (!string.IsNullOrWhiteSpace(line))
                    currentLesson.SummaryLines.Add(line.Trim());
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(line))
                    currentLesson.SummaryLines.Add(line.Trim());
            }
        }

        FlushLesson(ref currentLesson, currentSection);
        return sections;
    }

    private static void FlushLesson(ref LessonBuilder? builder, LessonSection? section)
    {
        if (builder is null || section is null) return;

        var summary = builder.SummaryLines.Count > 0
            ? string.Join(" ", builder.SummaryLines)
            : $"Study {builder.Title}, write the idea in your own words, then apply it to one low-risk XM demo plan.";

        var practicePrompt = builder.PracticeLines.Count > 0
            ? string.Join(" ", builder.PracticeLines)
            : $"Explain how {builder.Title} applies to a demo trade before execution.";

        var lesson = new Lesson
        {
            Id = builder.Id,
            SectionNumber = section.SectionNumber,
            SectionTitle = section.Title,
            Title = builder.Title,
            Duration = builder.Duration,
            Summary = summary,
            KeyTerms = builder.KeyTerms.Count > 0 ? builder.KeyTerms : ["risk", "setup", "confirmation"],
            CommonMistakes = builder.CommonMistakes.Count > 0
                ? builder.CommonMistakes
                : ["Skipping the plan", "Taking a trade without a clear reason", "Ignoring risk before entry"],
            PracticeTasks =
            [
                new PracticeTask
                {
                    Id = $"{builder.Id}-practice",
                    Prompt = practicePrompt,
                    ExpectedAction = "Write a short answer, then apply the concept in the trade planner."
                }
            ],
            DemoApplication = "Create one demo trade plan where this lesson concept is explicitly written in the reason field.",
            Checklist = ["I can explain the concept", "I completed a practice note", "I linked it to a trade plan"]
        };

        section.Lessons.Add(lesson);
        builder = null;
    }

    private static (string id, string title) ParseLessonIdTitle(string raw, LessonSection section, LessonBuilder? previous)
    {
        // e.g. "2.1 Understanding Pips" → id="2.1", title="Understanding Pips"
        // e.g. "Understanding Pips"    → id auto-generated
        var parts = raw.Split(' ', 2);
        if (parts.Length == 2 && IsLessonId(parts[0]))
            return (parts[0], parts[1].Trim());

        var lessonCount = section.Lessons.Count + 1;
        var id = $"{section.SectionNumber}.{lessonCount}";
        return (id, raw);
    }

    private static bool IsLessonId(string part) =>
        part.Length > 0 && (char.IsDigit(part[0]) || part[0] == '.') && part.Any(c => c == '.');

    private static LessonSection CreateDefaultSection(List<LessonSection> sections)
    {
        var s = new LessonSection
        {
            SectionNumber = sections.Count + 2,
            Title = "My Lessons",
            Lessons = []
        };
        sections.Add(s);
        return s;
    }

    private enum ParseMode { Summary, Content, Practice }

    private sealed class LessonBuilder
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string Duration { get; set; } = "";
        public List<string> SummaryLines { get; } = [];
        public List<string> KeyTerms { get; set; } = [];
        public List<string> CommonMistakes { get; set; } = [];
        public List<string> PracticeLines { get; } = [];
    }
}
