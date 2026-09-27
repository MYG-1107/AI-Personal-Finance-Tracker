using AIPersonalFinanceTracker.Api.Data;
using AIPersonalFinanceTracker.ML;
using AIPersonalFinanceTracker.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AIPersonalFinanceTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly CategorizationService _categorizationService;

    public TransactionsController(AppDbContext context, CategorizationService categorizationService)
    {
        _context = context;
        _categorizationService = categorizationService;
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
    public async Task<ActionResult<Transaction>> PostTransaction(Transaction transaction)
    {
        if (transaction.CategoryId == null || transaction.CategoryId == 0)
        {
            var predictedCategory = _categorizationService.PredictCategory(transaction.Description);
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name.ToLower() == predictedCategory.ToLower());

            if (category != null)
            {
                transaction.CategoryId = category.Id;
                transaction.IsAutoCategorized = true;
            }
            else
            {
                transaction.CategoryId = null;
            }
        }

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        if (transaction.CategoryId.HasValue)
        {
            await _context.Entry(transaction).Reference(t => t.Category).LoadAsync();
        }

        return CreatedAtAction(nameof(GetTransactions), new { id = transaction.Id }, transaction);
    }
}
