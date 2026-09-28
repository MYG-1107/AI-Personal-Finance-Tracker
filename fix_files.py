import os

os.makedirs('AIPersonalFinanceTracker.Api/Services', exist_ok=True)
os.makedirs('AIPersonalFinanceTracker.Api/Controllers', exist_ok=True)

service_code = """using Microsoft.EntityFrameworkCore;
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

controller_code = """using Microsoft.AspNetCore.Mvc;
using AIPersonalFinanceTracker.Api.Services;
using AIPersonalFinanceTracker.Shared.Models;

namespace AIPersonalFinanceTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ForecastController : ControllerBase
{
    private readonly ForecastService _forecastService;

    public ForecastController(ForecastService forecastService)
    {
        _forecastService = forecastService;
    }

    [HttpGet("cashflow")]
    public async Task<ActionResult<CashFlowForecastDto>> GetCashFlowForecast()
    {
        var forecast = await _forecastService.GetCashFlowForecastAsync();
        return Ok(forecast);
    }
}
"""

with open('AIPersonalFinanceTracker.Api/Services/ForecastService.cs', 'w') as f:
    f.write(service_code)

with open('AIPersonalFinanceTracker.Api/Controllers/ForecastController.cs', 'w') as f:
    f.write(controller_code)

print("Generated ForecastService.cs and ForecastController.cs using AppDbContext.")
