namespace AIPersonalFinanceTracker.Shared.Models;

public class Transaction
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public bool IsAutoCategorized { get; set; } = true;
    public bool IsAnomaly { get; set; }
    public string? AnomalyReason { get; set; }
    public int? CategoryId { get; set; }
    public Category? Category { get; set; }
}
