using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AIPersonalFinanceTracker.Api.Data;
using AIPersonalFinanceTracker.Shared.Models;
using AIPersonalFinanceTracker.ML;

namespace AIPersonalFinanceTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly FinancialHealthEngine _healthEngine;
    private readonly ForecastService _forecastService;

    public DashboardController(AppDbContext context, FinancialHealthEngine healthEngine, ForecastService forecastService)
    {
        _context = context;
        _healthEngine = healthEngine;
        _forecastService = forecastService;
    }

    [HttpGet("health")]
    public async Task<ActionResult<FinancialHealthDto>> GetHealthScore()
    {
        var transactions = await _context.Transactions.ToListAsync();
        var categories = await _context.Categories.ToListAsync();
        return _healthEngine.CalculateHealthScore(transactions, categories);
    }

    [HttpGet("forecast")]
    public async Task<ActionResult<CashFlowForecastDto>> GetCashFlowForecast()
    {
        var transactions = await _context.Transactions.ToListAsync();
        var categories = await _context.Categories.ToListAsync();
        return _forecastService.PredictCashFlow(transactions, categories);
    }
}
