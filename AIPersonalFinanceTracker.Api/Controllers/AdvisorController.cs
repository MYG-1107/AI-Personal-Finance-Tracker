using AIPersonalFinanceTracker.Api.Data;
using AIPersonalFinanceTracker.Shared.Models;
using AIPersonalFinanceTracker.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AIPersonalFinanceTracker.Api.Controllers;

[ApiController]
[Route("api/advisor")]
public class AdvisorController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly FinancialAdvisorService _advisor;

    public AdvisorController(AppDbContext context, FinancialAdvisorService advisor)
    {
        _context = context;
        _advisor = advisor;
    }

    [HttpPost("ask")]
    public async Task<ActionResult<AdvisorResponse>> Ask(AdvisorRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Question))
            return BadRequest(new { message = "Ask a finance question first." });

        var transactions = await _context.Transactions.AsNoTracking().ToListAsync();
        var categories = await _context.Categories.AsNoTracking().ToDictionaryAsync(c => c.Id, c => c.Name);
        var goals = await _context.SavingsGoals.AsNoTracking().ToListAsync();
        return Ok(_advisor.Answer(request.Question, transactions, categories, goals));
    }
}
