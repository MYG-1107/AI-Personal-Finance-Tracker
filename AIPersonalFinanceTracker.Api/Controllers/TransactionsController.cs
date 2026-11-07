using Microsoft.AspNetCore.Mvc;
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

    private static readonly List<Transaction> _sharedTransactions = new()
    {
        new Transaction { Id = 1, Description = "Monthly Salary Direct Deposit", Amount = 5000.00m, Date = DateTime.UtcNow.AddDays(-10), CategoryId = 3, IsAutoCategorized = true, IsAnomaly = false },
        new Transaction { Id = 2, Description = "Target Home Goods & Groceries", Amount = -120.45m, Date = DateTime.UtcNow.AddDays(-5), CategoryId = 1, IsAutoCategorized = true, IsAnomaly = false },
        new Transaction { Id = 3, Description = "Water Utility Bill", Amount = -5.50m, Date = DateTime.UtcNow.AddDays(-3), CategoryId = 2, IsAutoCategorized = true, IsAnomaly = false },
        new Transaction { Id = 4, Description = "Cinema Movie Tickets", Amount = -45.00m, Date = DateTime.UtcNow.AddDays(-2), CategoryId = 4, IsAutoCategorized = true, IsAnomaly = false },
        new Transaction { Id = 5, Description = "Uber Trip to Airport", Amount = -150.00m, Date = DateTime.UtcNow.AddDays(-1), CategoryId = 5, IsAutoCategorized = true, IsAnomaly = true, AnomalyReason = "High Expense Spike" }
    };

    private static readonly object _lock = new();

    public TransactionsController(CategoryPredictorService? categoryPredictor = null, AnomalyDetectionService? anomalyDetector = null)
    {
        _categoryPredictor = categoryPredictor ?? new CategoryPredictorService();
        _anomalyDetector = anomalyDetector ?? new AnomalyDetectionService();
    }

    // Support EF Core DI constructor overload if injected
    public TransactionsController(object? dbContext, CategoryPredictorService categoryPredictor, AnomalyDetectionService anomalyDetector)
        : this(categoryPredictor, anomalyDetector) { }

    [HttpGet]
    public IActionResult GetTransactions()
    {
        lock (_lock)
        {
            return Ok(_sharedTransactions.OrderByDescending(t => t.Date).ToList());
        }
    }

    [HttpPost]
    public IActionResult AddTransaction([FromBody] TransactionCreateDto dto)
    {
        try
        {
            var desc = string.IsNullOrWhiteSpace(dto?.Description) ? "Unspecified Transaction" : dto.Description;
            var amt = dto?.Amount ?? 0m;
            var catId = dto?.CategoryId;

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
                transaction.CategoryId = predId > 0 ? predId : 1;
                transaction.IsAutoCategorized = true;
            }

            var anomaly = _anomalyDetector.DetectAnomaly(transaction);
            transaction.IsAnomaly = anomaly.IsAnomaly;
            transaction.AnomalyReason = anomaly.Reason;

            lock (_lock)
            {
                transaction.Id = _sharedTransactions.Count > 0 ? _sharedTransactions.Max(t => t.Id) + 1 : 1;
                _sharedTransactions.Add(transaction);
            }

            return Ok(transaction);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
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
        return AddTransaction(dto);
    }

    [NonAction]
    public IActionResult PostTransaction(Transaction transaction)
    {
        return AddTransaction(transaction);
    }
}
