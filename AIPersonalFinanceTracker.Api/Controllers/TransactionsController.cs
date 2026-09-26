using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AIPersonalFinanceTracker.Api.Data;
using AIPersonalFinanceTracker.ML;
using AIPersonalFinanceTracker.Shared.Models;

namespace AIPersonalFinanceTracker.Api.Controllers;

public class PredictionRequest
{
    public string Description { get; set; } = string.Empty;
}

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly CategorizationService _mlService;

    public TransactionsController(AppDbContext context, CategorizationService mlService)
    {
        _context = context;
        _mlService = mlService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactions()
    {
        return await _context.Transactions.Include(t => t.Category).ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<Transaction>> PostTransaction(Transaction transaction)
    {
        if (transaction.Date == default)
            transaction.Date = DateTime.UtcNow;

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetTransactions), new { id = transaction.Id }, transaction);
    }

    [HttpPost("bulk")]
    public async Task<IActionResult> PostBulkTransactions([FromBody] List<Transaction> transactions)
    {
        if (transactions == null || !transactions.Any())
            return BadRequest("No transactions provided.");

        foreach (var t in transactions)
        {
            if (t.Date == default) t.Date = DateTime.UtcNow;
            _context.Transactions.Add(t);
        }

        await _context.SaveChangesAsync();
        return Ok(new { Count = transactions.Count });
    }

    [HttpPost("predict-category")]
    public ActionResult<object> PredictCategory([FromBody] PredictionRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Description))
            return Ok(new { Category = "Uncategorized" });

        var category = _mlService.PredictCategory(request.Description);
        return Ok(new { Category = category });
    }
}
