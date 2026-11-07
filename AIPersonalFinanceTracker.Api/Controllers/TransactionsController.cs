using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AIPersonalFinanceTracker.Api.Data;
using AIPersonalFinanceTracker.Shared.Models;
using AIPersonalFinanceTracker.ML;

namespace AIPersonalFinanceTracker.Api.Controllers;

public class TransactionCreateDto
{
    public string Description { get; set; } = "";
    public decimal Amount { get; set; }
    public int? CategoryId { get; set; }
    public DateTime? Date { get; set; }
}

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly CategoryPredictorService _categoryPredictor;
    private readonly AnomalyDetectionService _anomalyDetector;

    public TransactionsController(
        AppDbContext context,
        CategoryPredictorService categoryPredictor,
        AnomalyDetectionService anomalyDetector)
    {
        _context = context;
        _categoryPredictor = categoryPredictor ?? new CategoryPredictorService();
        _anomalyDetector = anomalyDetector ?? new AnomalyDetectionService();
    }

    private readonly AppDbContext _context;

    [HttpGet]
    public async Task<ActionResult<List<Transaction>>> GetTransactions()
    {
        return Ok(await _context.Transactions
            .AsNoTracking()
            .OrderByDescending(t => t.Date)
            .ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> AddTransaction([FromBody] TransactionCreateDto dto)
    {
        try
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Description))
                return BadRequest(new { message = "A transaction description is required." });

            if (dto.Amount == 0)
                return BadRequest(new { message = "A transaction amount must be non-zero." });

            var desc = dto.Description.Trim();
            var amt = dto.Amount;
            var catId = dto.CategoryId;

            var transaction = new Transaction
            {
                Description = desc,
                Amount = amt,
                Date = dto?.Date ?? DateTime.UtcNow,
                CategoryId = catId
            };

            if (!transaction.CategoryId.HasValue || transaction.CategoryId == 0)
            {
                var predId = _categoryPredictor.PredictCategory(transaction.Description);
                transaction.CategoryId = predId > 0 ? predId : null;
                transaction.IsAutoCategorized = predId > 0;
            }

            var anomaly = _anomalyDetector.DetectAnomaly(transaction);
            transaction.IsAnomaly = anomaly.IsAnomaly;
            transaction.AnomalyReason = anomaly.Reason;

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return Ok(transaction);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteTransaction(int id)
    {
        var transaction = await _context.Transactions.FindAsync(id);
        if (transaction == null)
            return NotFound(new { message = "Transaction not found." });

        _context.Transactions.Remove(transaction);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("{id:int}/category/{categoryId:int}")]
    public async Task<IActionResult> UpdateCategory(int id, int categoryId)
    {
        var transaction = await _context.Transactions.FindAsync(id);
        if (transaction == null)
            return NotFound(new { message = "Transaction not found." });

        if (!await _context.Categories.AnyAsync(c => c.Id == categoryId))
            return BadRequest(new { message = "Category not found." });

        transaction.CategoryId = categoryId;
        transaction.IsAutoCategorized = false;
        transaction.IsAnomaly = false;
        transaction.AnomalyReason = null;
        await _context.SaveChangesAsync();
        return Ok(transaction);
    }

    [NonAction]
    public IActionResult AddTransaction(Transaction transaction)
    {
        if (transaction == null) return BadRequest("Transaction required.");
        var dto = new TransactionCreateDto
        {
            Description = transaction.Description,
            Amount = transaction.Amount,
            CategoryId = transaction.CategoryId,
            Date = transaction.Date
        };
        return AddTransaction(dto).GetAwaiter().GetResult();
    }

    [NonAction]
    public IActionResult PostTransaction(Transaction transaction)
    {
        return AddTransaction(transaction);
    }
}
