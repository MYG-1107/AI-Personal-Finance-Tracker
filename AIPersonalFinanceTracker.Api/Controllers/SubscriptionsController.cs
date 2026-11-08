using AIPersonalFinanceTracker.Api.Data;
using AIPersonalFinanceTracker.Api.Services;
using AIPersonalFinanceTracker.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AIPersonalFinanceTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubscriptionsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly SubscriptionAnalyticsService _analytics;

    public SubscriptionsController(AppDbContext context, SubscriptionAnalyticsService analytics)
    {
        _context = context;
        _analytics = analytics;
    }

    [HttpGet]
    public async Task<ActionResult<SubscriptionAnalyticsDto>> GetSubscriptions()
    {
        var transactions = await _context.Transactions.AsNoTracking().ToListAsync();
        return Ok(_analytics.Analyze(transactions, DateTime.UtcNow));
    }
}