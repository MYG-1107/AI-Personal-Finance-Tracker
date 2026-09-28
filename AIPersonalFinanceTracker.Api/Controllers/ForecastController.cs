using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AIPersonalFinanceTracker.Api.Data;
using AIPersonalFinanceTracker.Shared.Models;
using AIPersonalFinanceTracker.ML;

namespace AIPersonalFinanceTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ForecastController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ForecastService _forecastService;

    public ForecastController(AppDbContext context, ForecastService forecastService)
    {
        _context = context;
        _forecastService = forecastService;
    }

    [HttpGet]
    public async Task<ActionResult<CashFlowForecastDto>> GetForecast()
    {
        var transactions = await _context.Transactions.ToListAsync();
        var categories = await _context.Categories.ToListAsync();
        return Ok(_forecastService.PredictCashFlow(transactions, categories));
    }
}
