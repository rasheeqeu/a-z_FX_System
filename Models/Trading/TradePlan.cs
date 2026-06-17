namespace ForexTradingWorkspace.Models.Trading;

public sealed class TradePlan
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string? LinkedLessonId { get; set; }
    public string Pair { get; set; } = "EURUSD";
    public string Direction { get; set; } = "Buy";
    public string Session { get; set; } = "London / New York";
    public string SetupType { get; set; } = "Lesson practice";
    public decimal Entry { get; set; } = 1.1000m;
    public decimal StopLoss { get; set; } = 1.0980m;
    public decimal TakeProfit { get; set; } = 1.1040m;
    public decimal RiskPercent { get; set; } = 1m;
    public string Reason { get; set; } = "";
    public string Invalidation { get; set; } = "";
    public bool NewsChecked { get; set; }
    public string Emotion { get; set; } = "Calm";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
