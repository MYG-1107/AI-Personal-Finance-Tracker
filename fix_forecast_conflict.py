import os

# 1. Remove duplicate ForecastService from API Services folder
api_forecast_path = 'AIPersonalFinanceTracker.Api/Services/ForecastService.cs'
if os.path.exists(api_forecast_path):
    os.remove(api_forecast_path)

# 2. Update Shared DTOs to include both primary and backwards-compatible properties
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
"""
with open(dtos_path, 'w', encoding='utf-8') as f:
    f.write(dtos_code)

# 3. Update ForecastService in ML project
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
"""
with open(ml_forecast_path, 'w', encoding='utf-8') as f:
    f.write(ml_forecast_code)

# 4. Update Program.cs to reference AIPersonalFinanceTracker.ML.ForecastService explicitly
program_path = 'AIPersonalFinanceTracker.Api/Program.cs'
program_code = """using Microsoft.EntityFrameworkCore;
using AIPersonalFinanceTracker.Api.Data;
using AIPersonalFinanceTracker.Api.Services;
using AIPersonalFinanceTracker.ML;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=finance.db"));

builder.Services.AddScoped<CategorizationService>();
builder.Services.AddScoped<AnomalyDetectionService>();
builder.Services.AddScoped<FinancialHealthEngine>();
builder.Services.AddScoped<AIPersonalFinanceTracker.ML.ForecastService>();
builder.Services.AddScoped<OcrReceiptService>();

builder.Services.AddControllers().AddJsonOptions(options => {
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", policy => {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseCors("AllowAll");

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
"""
with open(program_path, 'w', encoding='utf-8') as f:
    f.write(program_code)

print("Duplicate ForecastService removed and unified forecast DTOs applied successfully.")
