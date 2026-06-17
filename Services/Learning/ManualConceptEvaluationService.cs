using System.Text.Json;
using ForexTradingWorkspace.Models.Learning;

namespace ForexTradingWorkspace.Services.Learning;

public sealed class ManualConceptEvaluationService : IConceptEvaluationService
{
    public string BuildPrompt(Lesson lesson, string answer)
    {
        var hasCourseContent = !lesson.Summary.StartsWith("Study ", StringComparison.OrdinalIgnoreCase);
        var courseSection = hasCourseContent
            ? $"""

        ACTUAL COURSE CONTENT (compare my explanation against this):
        {lesson.Summary}
        """
            : "";

        var termsLine = lesson.KeyTerms.Count > 0
            ? $"Key terms I should know: {string.Join(", ", lesson.KeyTerms)}"
            : "";

        var mistakesLine = lesson.CommonMistakes.Count > 0
            ? $"Common mistakes to avoid: {string.Join(", ", lesson.CommonMistakes)}"
            : "";

        var practicePrompt = lesson.PracticeTasks.FirstOrDefault()?.Prompt ?? "";

        return $$"""
        You are evaluating whether I understood a forex lesson well enough to create one demo trade plan.
        Be strict but beginner-friendly. Do not pass vague or copy-pasted answers.

        LESSON: {{lesson.Id}} — {{lesson.Title}}
        {{termsLine}}
        {{mistakesLine}}
        {{courseSection}}

        PRACTICE TASK: {{practicePrompt}}

        MY EXPLANATION:
        {{answer}}

        EVALUATION RULES:
        - passed = true ONLY if I explained the concept clearly in my own words
        - I must include at least one practical trading example (mention entry, stop, risk, target, or pair)
        - If actual course content is provided above, check that my explanation is consistent with it
        - If my answer is shorter than 2 sentences, passed must be false
        - score = 0 to 100 (pass threshold is 70)
        - next_action must be exactly "rewrite_understanding" or "start_demo_plan"

        Return ONLY this JSON, no other text:
        {
          "passed": false,
          "score": 0,
          "missing": ["what I missed — be specific"],
          "feedback": "one short beginner-friendly sentence explaining what to improve",
          "next_action": "rewrite_understanding"
        }
        """;
    }

    public ConceptEvaluationResult ParseExpectedOutput(string text)
    {
        var result = new ConceptEvaluationResult
        {
            Passed = false,
            Score = 0,
            Feedback = "Could not parse AI result. Paste only the JSON object returned by the AI.",
            NextAction = "rewrite_understanding"
        };

        var json = ExtractJsonObject(text);
        if (string.IsNullOrWhiteSpace(json))
        {
            result.Missing.Add("Valid JSON result");
            return result;
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            result.Passed = root.TryGetProperty("passed", out var passed)
                && passed.ValueKind is JsonValueKind.True or JsonValueKind.False
                && passed.GetBoolean();

            result.Score = root.TryGetProperty("score", out var score) && score.TryGetInt32(out var scoreValue)
                ? Math.Clamp(scoreValue, 0, 100)
                : 0;

            result.Feedback = root.TryGetProperty("feedback", out var feedback)
                ? feedback.GetString() ?? ""
                : "No feedback provided.";

            result.NextAction = root.TryGetProperty("next_action", out var next)
                ? next.GetString() ?? "rewrite_understanding"
                : "rewrite_understanding";

            if (root.TryGetProperty("missing", out var missing) && missing.ValueKind == JsonValueKind.Array)
            {
                result.Missing = missing.EnumerateArray()
                    .Select(x => x.GetString())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x!)
                    .ToList();
            }

            if (result.Score < 70 || !string.Equals(result.NextAction, "start_demo_plan", StringComparison.OrdinalIgnoreCase))
            {
                result.Passed = false;
            }

            result.UsedAi = true;
            return result;
        }
        catch
        {
            result.Missing.Add("Parseable JSON object");
            return result;
        }
    }

    public Task<ConceptEvaluationResult> EvaluateAsync(Lesson lesson, string answer)
    {
        return Task.FromResult(EvaluateLocally(lesson, answer));
    }

    private static ConceptEvaluationResult EvaluateLocally(Lesson lesson, string answer)
    {
        var normalized = answer.Trim();
        var conceptWords = lesson.Title
            .Split([' ', ',', '/', '-', '(', ')'], StringSplitOptions.RemoveEmptyEntries)
            .Where(x => x.Length >= 4)
            .Concat(lesson.KeyTerms)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var mentionsConcept = conceptWords.Any(word => normalized.Contains(word, StringComparison.OrdinalIgnoreCase));
        var hasTradingExample = new[] { "trade", "entry", "stop", "risk", "target", "because", "if", "then" }
            .Any(word => normalized.Contains(word, StringComparison.OrdinalIgnoreCase));
        var longEnough = normalized.Length >= 60;

        var missing = new List<string>();
        if (!longEnough) missing.Add("Write a fuller explanation.");
        if (!mentionsConcept) missing.Add("Mention the lesson concept directly.");
        if (!hasTradingExample) missing.Add("Add one trading example with risk, entry, stop, or target.");

        var passed = missing.Count == 0;
        return new ConceptEvaluationResult
        {
            Passed = passed,
            Score = Math.Clamp((longEnough ? 35 : 0) + (mentionsConcept ? 30 : 0) + (hasTradingExample ? 35 : 0), 0, 100),
            Missing = missing,
            Feedback = passed
                ? "Local check passed. For stronger checking, copy the prompt to an AI and paste back the JSON result."
                : "Local check failed. Improve the answer or ask an AI to evaluate it.",
            NextAction = passed ? "start_demo_plan" : "rewrite_understanding",
            UsedAi = false
        };
    }

    private static string ExtractJsonObject(string text)
    {
        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');
        return start >= 0 && end > start ? text[start..(end + 1)] : "";
    }
}
