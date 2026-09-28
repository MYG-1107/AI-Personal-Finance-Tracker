using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AIPersonalFinanceTracker.Api.Data;
using AIPersonalFinanceTracker.Shared.Models;
using AIPersonalFinanceTracker.ML;

namespace AIPersonalFinanceTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly CategorizationService _categorizationService;
    private readonly AnomalyDetectionService _anomalyService;

    public TransactionsController(AppDbContext context, CategorizationService categorizationService, AnomalyDetectionService anomalyService)
    {
        _context = context;
        _categorizationService = categorizationService;
        _anomalyService = anomalyService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactions()
    {
        return await _context.Transactions
            .Include(t => t.Category)
            .OrderByDescending(t => t.Date)
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<Transaction>> CreateTransaction(Transaction transaction)
    {
        if (transaction.CategoryId == null || transaction.CategoryId == 0)
        {
            var (catName, catId) = _categorizationService.PredictCategory(transaction.Description);
            transaction.CategoryId = catId;
            transaction.IsAutoCategorized = true;
        }

        var history = await _context.Transactions.ToListAsync();
        var anomalyCheck = _anomalyService.DetectAnomaly(transaction.Amount, transaction.Description, history);
        transaction.IsAnomaly = anomalyCheck.IsAnomaly;
        transaction.AnomalyReason = anomalyCheck.Reason;

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        await _context.Entry(transaction).Reference(t => t.Category).LoadAsync();
        return CreatedAtAction(nameof(GetTransactions), new { id = transaction.Id }, transaction);
    }

    [HttpPut("{id}/category")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] int categoryId)
    {
        var tx = await _context.Transactions.FindAsync(id);
        if (tx == null) return NotFound();

        tx.CategoryId = categoryId;
        tx.IsAutoCategorized = false;
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
