import os, glob, re

# 1. Update Program.cs to replace FinanceDbContext with AppDbContext and fix namespaces
program_path = 'AIPersonalFinanceTracker.Api/Program.cs'
if os.path.exists(program_path):
    with open(program_path, 'r', encoding='utf-8') as f:
        content = f.read()

    # Add missing usings
    usings = ["using AIPersonalFinanceTracker.Api.Data;", "using AIPersonalFinanceTracker.Api.Services;"]
    for u in usings:
        if u not in content:
            content = u + "\n" + content

    # Replace FinanceDbContext with AppDbContext
    content = content.replace('FinanceDbContext', 'AppDbContext')

    # Register ForecastService if not present
    if 'ForecastService' not in content:
        content = content.replace('builder.Services.AddControllers();', 'builder.Services.AddControllers();\nbuilder.Services.AddScoped<ForecastService>();')

    with open(program_path, 'w', encoding='utf-8') as f:
        f.write(content)
    print("Updated Program.cs successfully.")

# 2. Ensure CategorizationService exists
cat_service_path = 'AIPersonalFinanceTracker.Api/Services/CategorizationService.cs'
if not os.path.exists(cat_service_path):
    os.makedirs('AIPersonalFinanceTracker.Api/Services', exist_ok=True)
    cat_code = """namespace AIPersonalFinanceTracker.Api.Services;

public class CategorizationService
{
    public string PredictCategory(string description)
    {
        return "General";
    }
}
"""
    with open(cat_service_path, 'w', encoding='utf-8') as f:
        f.write(cat_code)
    print("Created CategorizationService.cs.")

# 3. Ensure ForecastService uses AppDbContext
forecast_service_path = 'AIPersonalFinanceTracker.Api/Services/ForecastService.cs'
forecast_code = """using Microsoft.EntityFrameworkCore;
using AIPersonalFinanceTracker.Api.Data;
using AIPersonalFinanceTracker.Shared.Models;

namespace AIPersonalFinanceTracker.Api.Services;

public class ForecastService
{
    private readonly AppDbContext _context;

    public ForecastService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CashFlowForecastDto> GetCashFlowForecastAsync()
    {
        var transactions = await _context.Transactions
            .Include(t => t.Category)
            .OrderBy(t => t.Date)
            .ToListAsync();

        var today = DateTime.UtcNow.Date;
        var endOfMonth = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
        var daysRemaining = (endOfMonth - today).Days;

        var currentBalance = transactions.Sum(t => t.Amount);

        var currentMonthExpenses = transactions
            .Where(t => t.Date.Year == today.Year && t.Date.Month == today.Month && t.Amount < 0)
            .Sum(t => Math.Abs(t.Amount));

        int elapsedDaysInMonth = Math.Max(1, today.Day);
        decimal avgDailySpend = currentMonthExpenses / elapsedDaysInMonth;

        if (avgDailySpend == 0 && transactions.Any(t => t.Amount < 0))
        {
            var firstTxDate = transactions.Min(t => t.Date).Date;
            var totalHistoryDays = Math.Max(1, (today - firstTxDate).Days + 1);
            var totalExpenses = transactions.Where(t => t.Amount < 0).Sum(t => Math.Abs(t.Amount));
            avgDailySpend = totalExpenses / totalHistoryDays;
        }

        decimal projectedRemainingExpenses = avgDailySpend * daysRemaining;
        decimal projectedEndOfMonthBalance = currentBalance - projectedRemainingExpenses;

        var forecastDto = new CashFlowForecastDto
        {
            CurrentBalance = currentBalance,
            ProjectedEndOfMonthBalance = projectedEndOfMonthBalance,
            ProjectedMonthlyExpenses = currentMonthExpenses + projectedRemainingExpenses,
            AverageDailySpend = avgDailySpend,
            DaysRemainingInMonth = daysRemaining
        };

        forecastDto.DailyForecasts.Add(new DailyForecastDto
        {
            Date = today,
            ProjectedBalance = currentBalance,
            DailySpend = 0,
            IsHistorical = true
        });

        var runningBalance = currentBalance;
        for (int i = 1; i <= Math.Min(14, daysRemaining); i++)
        {
            var forecastDate = today.AddDays(i);
            runningBalance -= avgDailySpend;
            forecastDto.DailyForecasts.Add(new DailyForecastDto
            {
                Date = forecastDate,
                ProjectedBalance = runningBalance,
                DailySpend = avgDailySpend,
                IsHistorical = false
            });
        }

        if (projectedEndOfMonthBalance < 0)
        {
            forecastDto.Insights.Add($"Deficit Alert: At your current rate of ${avgDailySpend:F2}/day, your projected end-of-month balance will reach ${projectedEndOfMonthBalance:F2}.");
        }
        else
        {
            forecastDto.Insights.Add($"Cash Flow Healthy: You are projected to finish the month with a surplus balance of ${projectedEndOfMonthBalance:F2}.");
        }

        if (avgDailySpend > 0)
        {
            decimal safeDailySpend = Math.Max(0, currentBalance / Math.Max(1, daysRemaining));
            forecastDto.Insights.Add($"Recommended Daily Budget: To maintain a positive balance, cap daily expenses at ${safeDailySpend:F2}/day for the remaining {daysRemaining} days.");
        }

        return forecastDto;
    }
}
"""
with open(forecast_service_path, 'w', encoding='utf-8') as f:
    f.write(forecast_code)

print("Repair complete.")
