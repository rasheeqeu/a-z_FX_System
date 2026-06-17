using ForexTradingWorkspace.Models.Learning;

namespace ForexTradingWorkspace.Services.Learning;

public interface IConceptEvaluationService
{
    string BuildPrompt(Lesson lesson, string answer);
    ConceptEvaluationResult ParseExpectedOutput(string text);
    Task<ConceptEvaluationResult> EvaluateAsync(Lesson lesson, string answer);
}
