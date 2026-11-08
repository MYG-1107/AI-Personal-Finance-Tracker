using AIPersonalFinanceTracker.Shared.Models;
using Microsoft.ML;
using Microsoft.ML.Transforms.TimeSeries;

namespace AIPersonalFinanceTracker.ML;

public class ForecastService
{
    private readonly MLContext _mlContext = new();

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

        var fallbackDailySpend = currentDay > 0 ? totalExpense / currentDay : 0;
        var predictedDailySpend = ForecastDailyExpenses(transactions, now, remainingDays, fallbackDailySpend);
        var dailyBurnRate = predictedDailySpend.Count > 0
            ? predictedDailySpend.Average()
            : fallbackDailySpend;

        forecast.PredictedDailyBurnRate = dailyBurnRate;
        forecast.AverageDailySpend = dailyBurnRate;
        forecast.CurrentBalance = currentBalance;

        decimal projectedAdditionalExpense = predictedDailySpend.Sum();
        forecast.ProjectedMonthlyExpenses = totalExpense + projectedAdditionalExpense;
        forecast.ProjectedEndOfMonthBalance = currentBalance - projectedAdditionalExpense;

        decimal runningBalance = currentBalance;
        for (int i = 1; i <= remainingDays; i++)
        {
            var dailySpend = predictedDailySpend.Count > i - 1
                ? predictedDailySpend[i - 1]
                : dailyBurnRate;
            runningBalance -= dailySpend;
            forecast.DailyForecasts.Add(new DailyForecastDto
            {
                Date = now.AddDays(i),
                Amount = dailySpend,
                DailySpend = dailySpend,
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

    private List<decimal> ForecastDailyExpenses(
        IReadOnlyCollection<Transaction> transactions,
        DateTime now,
        int horizon,
        decimal fallbackDailySpend)
    {
        if (horizon <= 0)
            return new List<decimal>();

        var startDate = transactions.Count > 0
            ? transactions.Min(t => t.Date).Date
            : now.Date;
        var numberOfDays = (now.Date - startDate).Days + 1;
        if (numberOfDays < 7)
            return Enumerable.Repeat(fallbackDailySpend, horizon).ToList();

        var dailyExpenses = Enumerable.Range(0, numberOfDays)
            .Select(offset =>
            {
                var date = startDate.AddDays(offset);
                return (float)transactions
                    .Where(t => t.Date.Date == date && t.Amount < 0)
                    .Sum(t => Math.Abs(t.Amount));
            })
            .ToList();

        try
        {
            var data = _mlContext.Data.LoadFromEnumerable(
                dailyExpenses.Select(value => new ExpensePoint { Value = value }));
            var windowSize = Math.Clamp(dailyExpenses.Count / 3, 2, 7);
            var pipeline = _mlContext.Forecasting.ForecastBySsa(
                outputColumnName: nameof(ExpenseForecast.Forecast),
                inputColumnName: nameof(ExpensePoint.Value),
                windowSize: windowSize,
                seriesLength: dailyExpenses.Count,
                trainSize: dailyExpenses.Count,
                horizon: horizon,
                confidenceLevel: 0.95f,
                confidenceLowerBoundColumn: null,
                confidenceUpperBoundColumn: null);
            var model = pipeline.Fit(data);
            var forecast = model.Transform(data);
            var values = _mlContext.Data.CreateEnumerable<ExpenseForecast>(forecast, reuseRowObject: false)
                .FirstOrDefault()?.Forecast;

            if (values is { Length: > 0 })
            {
                return values
                    .Take(horizon)
                    .Select(value => Math.Max(0, (decimal)value))
                    .ToList();
            }
        }
        catch (ArgumentException)
        {
            // Sparse or constant histories can be unsuitable for SSA.
        }
        catch (InvalidOperationException)
        {
            // Keep the API usable when a history cannot produce a model.
        }

        return Enumerable.Repeat(fallbackDailySpend, horizon).ToList();
    }

    private sealed class ExpensePoint
    {
        public float Value { get; set; }
    }

    private sealed class ExpenseForecast
    {
        public float[] Forecast { get; set; } = Array.Empty<float>();
    }
}
