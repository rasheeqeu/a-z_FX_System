namespace ForexTradingWorkspace.Models;

public abstract class ScreenState
{
    public DateTime LastSaved { get; set; } = DateTime.UtcNow;
    public abstract bool IsValid { get; }
}
