namespace ForexTradingWorkspace.Models.Trading;

public enum TradeWorkflowState
{
    NoTrade,
    LessonSelected,
    Planning,
    RiskCalculated,
    ChecklistComplete,
    ScreenshotCaptured,
    ReadyForManualExecution,
    ManuallyPlaced,
    ResultRecorded,
    Reviewed,
    Completed
}
