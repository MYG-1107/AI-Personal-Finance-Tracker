using AIPersonalFinanceTracker.Api.Data;
using AIPersonalFinanceTracker.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AIPersonalFinanceTracker.Api.Controllers;

[ApiController]
[Route("api/advisor")]
public class AdvisorController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdvisorController(AppDbContext context) => _context = context;

    [HttpPost("ask")]
    public async Task<ActionResult<AdvisorResponse>> Ask(AdvisorRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Question))
            return BadRequest(new { message = "Ask a finance question first." });

        var transactions = await _context.Transactions.AsNoTracking().ToListAsync();
        if (transactions.Count == 0)
            return Ok(new AdvisorResponse { Answer = "Add a few transactions and I will turn them into personalized insights." });

        var question = request.Question.ToLowerInvariant();
        var expenses = transactions.Where(t => t.Amount < 0).ToList();
        var top = expenses.GroupBy(t => t.CategoryId).OrderByDescending(g => g.Sum(t => Math.Abs(t.Amount))).FirstOrDefault();
        var topSpend = top?.Sum(t => Math.Abs(t.Amount)) ?? 0;
        var totalSpend = expenses.Sum(t => Math.Abs(t.Amount));
        var weekSpend = expenses.Where(t => t.Date >= DateTime.UtcNow.AddDays(-7)).Sum(t => Math.Abs(t.Amount));

        if (question.Contains("most") || question.Contains("top") || question.Contains("where"))
        {
            return Ok(new AdvisorResponse
            {
                Answer = $"Your largest spending area is category {top?.Key?.ToString() ?? "uncategorized"}, totaling {topSpend:N2}.",
                Highlights = new() { $"Total tracked spending: {totalSpend:N2}", $"Last 7 days: {weekSpend:N2}" }
            });
        }

        if (question.Contains("transport") || question.Contains("travel"))
        {
            var transport = expenses.Where(t => t.CategoryId == 5).Sum(t => Math.Abs(t.Amount));
            return Ok(new AdvisorResponse
            {
                Answer = $"Transport spending is {transport:N2}. Consider setting a category budget and reviewing recurring trips.",
                Highlights = new() { $"Transport share: {(totalSpend == 0 ? 0 : transport / totalSpend * 100):N1}%", "Review high-value travel transactions first." }
            });
        }

        return Ok(new AdvisorResponse
        {
            Answer = $"You have tracked {transactions.Count} transactions and {totalSpend:N2} in expenses. A good next step is to set a savings goal and review the highest category.",
            Highlights = new() { $"Income: {transactions.Where(t => t.Amount > 0).Sum(t => t.Amount):N2}", $"Last 7 days: {weekSpend:N2}" }
        });
    }
}
