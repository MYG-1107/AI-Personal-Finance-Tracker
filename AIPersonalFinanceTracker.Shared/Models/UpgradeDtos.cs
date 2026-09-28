namespace AIPersonalFinanceTracker.Shared.Models;

public class FinancialHealthDto
{
    public int HealthScore { get; set; } = 85;
    public string HealthGrade { get; set; } = "A";
    public decimal SavingsRate { get; set; }
    public decimal BudgetAdherenceRate { get; set; }
    public List<string> Recommendations { get; set; } = new();
}

public class AnomalyCheckResult
{
    public bool IsAnomaly { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class ReceiptScanResultDto
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string SuggestedCategory { get; set; } = "General";
    public int SuggestedCategoryId { get; set; } = 1;
}

public class DailyForecastDto
{
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public decimal DailySpend { get; set; }
    public decimal ProjectedBalance { get; set; }
    public bool IsHistorical { get; set; }
}

public class CashFlowForecastDto
{
    public decimal ProjectedEndOfMonthBalance { get; set; }
    public decimal PredictedDailyBurnRate { get; set; }
    public decimal CurrentBalance { get; set; }
    public decimal ProjectedMonthlyExpenses { get; set; }
    public decimal AverageDailySpend { get; set; }
    public int DaysRemainingInMonth { get; set; }
    public List<string> Insights { get; set; } = new();
    public List<DailyForecastDto> DailyForecasts { get; set; } = new();
    public List<CategoryForecastDto> CategoryProjections { get; set; } = new();
    public List<string> ForecastWarnings { get; set; } = new();
}

public class CategoryForecastDto
{
    public string CategoryName { get; set; } = string.Empty;
    public decimal CurrentSpent { get; set; }
    public decimal ProjectedSpent { get; set; }
    public decimal BudgetLimit { get; set; }
    public bool IsProjectedOverrun { get; set; }
}
