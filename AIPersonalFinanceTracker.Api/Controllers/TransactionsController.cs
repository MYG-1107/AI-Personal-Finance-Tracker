using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AIPersonalFinanceTracker.Api.Data;
using AIPersonalFinanceTracker.Shared.Models;
using AIPersonalFinanceTracker.ML;
using System.Globalization;
using System.Text.RegularExpressions;

namespace AIPersonalFinanceTracker.Api.Controllers;

public class TransactionCreateDto
{
    public string Description { get; set; } = "";
    public decimal Amount { get; set; }
    public int? CategoryId { get; set; }
    public DateTime? Date { get; set; }
    public bool IsRecurring { get; set; }
    public int? RecurrenceDay { get; set; }
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
                CategoryId = catId,
                IsRecurring = dto.IsRecurring,
                RecurrenceDay = dto.RecurrenceDay,
                NextOccurrence = dto.IsRecurring
                    ? (dto.Date ?? DateTime.UtcNow).AddMonths(1)
                    : null
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

            if (transaction.Amount > 0)
            {
                var allocationPercent = await _context.UserProfiles
                    .Where(p => p.Id == 1)
                    .Select(p => p.AutoGoalAllocationPercent)
                    .SingleOrDefaultAsync();
                var activeGoals = await _context.SavingsGoals
                    .Where(g => g.CurrentAmount < g.TargetAmount)
                    .ToListAsync();
                var allocation = Math.Min(transaction.Amount * allocationPercent / 100m,
                    activeGoals.Sum(g => Math.Max(0, g.TargetAmount - g.CurrentAmount)));
                if (allocation > 0 && activeGoals.Count > 0)
                {
                    var remaining = allocation;
                    foreach (var goal in activeGoals)
                    {
                        var contribution = Math.Min(
                            Math.Round(allocation / activeGoals.Count, 2),
                            goal.TargetAmount - goal.CurrentAmount);
                        contribution = Math.Min(contribution, remaining);
                        goal.CurrentAmount += contribution;
                        remaining -= contribution;
                    }

                    if (remaining > 0)
                    {
                        var first = activeGoals.First();
                        first.CurrentAmount = Math.Min(first.TargetAmount, first.CurrentAmount + remaining);
                    }

                    await _context.SaveChangesAsync();
                }
            }

            return Ok(transaction);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPost("smart-add")]
    public ActionResult<SmartAddPreview> SmartAdd(SmartAddRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Text))
            return BadRequest(new { message = "Describe the transaction first." });

        var text = request.Text.Trim();
        var amountMatch = Regex.Match(text, @"(?<amount>\d+(?:[.,]\d{1,2})?)");
        if (!amountMatch.Success ||
            !decimal.TryParse(amountMatch.Groups["amount"].Value.Replace(',', '.'),
                NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) ||
            amount <= 0)
            return BadRequest(new { message = "Include a positive amount, for example 'spent 450 INR on groceries'." });

        var currencyMatch = Regex.Match(text, @"\b(USD|INR|EUR|GBP|NOK|SEK|CAD|AUD|JPY|CHF|AED|BRL)\b", RegexOptions.IgnoreCase);
        var currency = currencyMatch.Success ? currencyMatch.Value.ToUpperInvariant() : "USD";
        var date = text.Contains("yesterday", StringComparison.OrdinalIgnoreCase)
            ? DateTime.UtcNow.Date.AddDays(-1)
            : DateTime.UtcNow;
        var description = Regex.Replace(text, @"\b(spent|paid|bought|purchase|on|for|today|yesterday)\b", "", RegexOptions.IgnoreCase)
            .Trim(' ', ',', '.', ':');
        var categoryId = _categoryPredictor.PredictCategory(description);

        return Ok(new SmartAddPreview
        {
            Description = description,
            Amount = -amount,
            CurrencyCode = currency,
            Date = date,
            SuggestedCategoryId = categoryId > 0 ? categoryId : null,
            SuggestedCategoryName = categoryId switch
            {
                1 => "Groceries & Food",
                2 => "Utilities & Bills",
                3 => "Salary & Income",
                4 => "Entertainment",
                5 => "Transport & Travel",
                _ => "Uncategorized"
            },
            Message = "Review the parsed fields before saving."
        });
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
    public async Task<IActionResult> AddTransaction(Transaction transaction)
    {
        if (transaction == null) return BadRequest("Transaction required.");
        var dto = new TransactionCreateDto
        {
            Description = transaction.Description,
            Amount = transaction.Amount,
            CategoryId = transaction.CategoryId,
            Date = transaction.Date
        };
        return await AddTransaction(dto);
    }

    [NonAction]
    public async Task<IActionResult> PostTransaction(Transaction transaction)
    {
        return await AddTransaction(transaction);
    }
}
