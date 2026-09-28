namespace AIPersonalFinanceTracker.Shared.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "Expense";
    public decimal MonthlyBudgetLimit { get; set; }
    public decimal BudgetLimit { get; set; }
}
