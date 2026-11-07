using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AIPersonalFinanceTracker.Shared.Models;
using AIPersonalFinanceTracker.Api.Data;

namespace AIPersonalFinanceTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _context;

    public DashboardController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("health")]
    public IActionResult GetHealth()
    {
        return Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow });
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        try
        {
            var transactions = await _context.Transactions.AsNoTracking().ToListAsync();
            var income = transactions.Where(t => t.Amount > 0).Sum(t => t.Amount);
            var expenses = transactions.Where(t => t.Amount < 0).Sum(t => Math.Abs(t.Amount));

            return Ok(new
            {
                TotalIncome = income,
                TotalExpenses = expenses,
                TotalBalance = income - expenses
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Unable to calculate dashboard summary: {ex.Message}" });
        }
    }
}
