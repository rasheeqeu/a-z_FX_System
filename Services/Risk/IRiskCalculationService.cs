using ForexTradingWorkspace.Models.Risk;

namespace ForexTradingWorkspace.Services.Risk;

public interface IRiskCalculationService
{
    RiskResult Calculate(RiskInput input);
}
