import os

# 1. Update Category.cs to include BudgetLimit
cat_path = 'AIPersonalFinanceTracker.Shared/Models/Category.cs'
cat_code = """namespace AIPersonalFinanceTracker.Shared.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "Expense";
    public decimal MonthlyBudgetLimit { get; set; }
    public decimal BudgetLimit { get; set; }
}
"""
with open(cat_path, 'w', encoding='utf-8') as f:
    f.write(cat_code)

# 2. Update UpgradeDtos.cs to add missing forecast members
dtos_path = 'AIPersonalFinanceTracker.Shared/Models/UpgradeDtos.cs'
dtos_code = """namespace AIPersonalFinanceTracker.Shared.Models;

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
    public decimal ProjectedBalance { get; set; }
}

public class CashFlowForecastDto
{
    public decimal ProjectedEndOfMonthBalance { get; set; }
    public decimal PredictedDailyBurnRate { get; set; }
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
"""
with open(dtos_path, 'w', encoding='utf-8') as f:
    f.write(dtos_code)

# 3. Update ForecastService.cs to populate the missing fields
ml_forecast_path = 'AIPersonalFinanceTracker.ML/ForecastService.cs'
ml_forecast_code = """using AIPersonalFinanceTracker.Shared.Models;

namespace AIPersonalFinanceTracker.ML;

public class ForecastService
{
    public CashFlowForecastDto PredictCashFlow(List<Transaction> transactions, List<Category> categories)
    {
        var forecast = new CashFlowForecastDto();
        var now = DateTime.UtcNow;
        int daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
        int currentDay = Math.Max(1, now.Day);
        int remainingDays = daysInMonth - currentDay;

        forecast.DaysRemainingInMonth = remainingDays;

        decimal totalIncome = transactions.Where(t => t.Amount > 0).Sum(t => t.Amount);
        decimal totalExpense = transactions.Where(t => t.Amount < 0).Sum(t => Math.Abs(t.Amount));
        decimal currentBalance = totalIncome - totalExpense;

        decimal dailyBurnRate = currentDay > 0 ? totalExpense / currentDay : 0;
        forecast.PredictedDailyBurnRate = dailyBurnRate;

        decimal projectedAdditionalExpense = dailyBurnRate * remainingDays;
        forecast.ProjectedEndOfMonthBalance = currentBalance - projectedAdditionalExpense;

        decimal runningBalance = currentBalance;
        for (int i = 1; i <= remainingDays; i++)
        {
            runningBalance -= dailyBurnRate;
            forecast.DailyForecasts.Add(new DailyForecastDto
            {
                Date = now.AddDays(i),
                Amount = dailyBurnRate,
                ProjectedBalance = runningBalance
            });
        }

        foreach (var cat in categories.Where(c => c.Type == "Expense"))
        {
            decimal catSpent = transactions
                .Where(t => t.CategoryId == cat.Id && t.Amount < 0)
                .Sum(t => Math.Abs(t.Amount));

            decimal catLimit = cat.BudgetLimit > 0 ? cat.BudgetLimit : cat.MonthlyBudgetLimit;
            decimal catDailyBurn = currentDay > 0 ? catSpent / currentDay : 0;
            decimal catProjected = catSpent + (catDailyBurn * remainingDays);
            bool isOverrun = catLimit > 0 && catProjected > catLimit;

            forecast.CategoryProjections.Add(new CategoryForecastDto
            {
                CategoryName = cat.Name,
                CurrentSpent = catSpent,
                ProjectedSpent = catProjected,
                BudgetLimit = catLimit,
                IsProjectedOverrun = isOverrun
            });

            if (isOverrun)
            {
                string warnMsg = $"Projected Overrun: '{cat.Name}' is on track to hit ${catProjected:F2} (Budget Limit: ${catLimit:F2}).";
                forecast.ForecastWarnings.Add(warnMsg);
                forecast.Insights.Add(warnMsg);
            }
        }

        if (forecast.ProjectedEndOfMonthBalance < 0)
        {
            forecast.Insights.Add("Alert: Projected balance will fall below $0 at current burn rate.");
        }
        else
        {
            forecast.Insights.Add($"On track: Projected end-of-month balance is ${forecast.ProjectedEndOfMonthBalance:F2}.");
        }

        return forecast;
    }
}
"""
with open(ml_forecast_path, 'w', encoding='utf-8') as f:
    f.write(ml_forecast_code)

print("Models and ForecastService updated successfully.")
