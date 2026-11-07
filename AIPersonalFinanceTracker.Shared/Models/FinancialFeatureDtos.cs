namespace AIPersonalFinanceTracker.Shared.Models;

public class SavingsGoal
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public DateTime? TargetDate { get; set; }
    public string Color { get; set; } = "#0d6efd";
}

public class AdvisorRequest
{
    public string Question { get; set; } = string.Empty;
}

public class AdvisorResponse
{
    public string Answer { get; set; } = string.Empty;
    public List<string> Highlights { get; set; } = new();
}
