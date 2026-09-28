using AIPersonalFinanceTracker.Shared.Models;

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
        forecast.AverageDailySpend = dailyBurnRate;
        forecast.CurrentBalance = currentBalance;

        decimal projectedAdditionalExpense = dailyBurnRate * remainingDays;
        forecast.ProjectedMonthlyExpenses = totalExpense + projectedAdditionalExpense;
        forecast.ProjectedEndOfMonthBalance = currentBalance - projectedAdditionalExpense;

        decimal runningBalance = currentBalance;
        for (int i = 1; i <= remainingDays; i++)
        {
            runningBalance -= dailyBurnRate;
            forecast.DailyForecasts.Add(new DailyForecastDto
            {
                Date = now.AddDays(i),
                Amount = dailyBurnRate,
                DailySpend = dailyBurnRate,
                ProjectedBalance = runningBalance,
                IsHistorical = false
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
