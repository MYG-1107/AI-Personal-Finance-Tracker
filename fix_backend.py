import os

os.makedirs('AIPersonalFinanceTracker.ML', exist_ok=True)

# 1. CategoryPredictorService.cs
with open('AIPersonalFinanceTracker.ML/CategoryPredictorService.cs', 'w') as f:
    f.write('''namespace AIPersonalFinanceTracker.ML;

public class CategoryPredictorService
{
    public int PredictCategory(string description)
    {
        if (string.IsNullOrWhiteSpace(description)) return 1;
        var desc = description.ToLowerInvariant();
        if (desc.Contains("coffee") || desc.Contains("starbucks") || desc.Contains("restaurant") || desc.Contains("food")) return 2;
        if (desc.Contains("target") || desc.Contains("walmart") || desc.Contains("grocer")) return 1;
        if (desc.Contains("salary") || desc.Contains("deposit") || desc.Contains("payroll")) return 5;
        if (desc.Contains("uber") || desc.Contains("lyft") || desc.Contains("gas")) return 3;
        return 1;
    }
}
''')

# 2. AnomalyDetectionService.cs
with open('AIPersonalFinanceTracker.ML/AnomalyDetectionService.cs', 'w') as f:
    f.write('''using AIPersonalFinanceTracker.Shared.Models;

namespace AIPersonalFinanceTracker.ML;

public class AnomalyResult
{
    public bool IsAnomaly { get; set; }
    public string Reason { get; set; } = "";
}

public class AnomalyDetectionService
{
    public AnomalyResult DetectAnomaly(Transaction transaction)
    {
        if (Math.Abs(transaction.Amount) > 1000m)
        {
            return new AnomalyResult { IsAnomaly = true, Reason = "High-value transaction over $1,000" };
        }
        return new AnomalyResult { IsAnomaly = false, Reason = "" };
    }
}
''')

# 3. TransactionsController.cs
with open('AIPersonalFinanceTracker.Api/Controllers/TransactionsController.cs', 'w') as f:
    f.write('''using Microsoft.AspNetCore.Mvc;
using AIPersonalFinanceTracker.Shared.Models;
using AIPersonalFinanceTracker.ML;

namespace AIPersonalFinanceTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private static readonly List<Transaction> _transactions = new()
    {
        new Transaction { Id = 1, Description = "Starbucks Coffee", Amount = -5.50m, Date = DateTime.UtcNow.AddDays(-1), CategoryId = 2, IsAutoCategorized = true, IsAnomaly = false },
        new Transaction { Id = 2, Description = "Target Superstore", Amount = -120.45m, Date = DateTime.UtcNow.AddDays(-2), CategoryId = 1, IsAutoCategorized = true, IsAnomaly = false },
        new Transaction { Id = 3, Description = "Salary Direct Deposit", Amount = 3500.00m, Date = DateTime.UtcNow.AddDays(-5), CategoryId = 5, IsAutoCategorized = false, IsAnomaly = false }
    };

    private readonly CategoryPredictorService _categoryPredictor;
    private readonly AnomalyDetectionService _anomalyDetector;

    public TransactionsController(CategoryPredictorService categoryPredictor, AnomalyDetectionService anomalyDetector)
    {
        _categoryPredictor = categoryPredictor;
        _anomalyDetector = anomalyDetector;
    }

    [HttpGet]
    public ActionResult<List<Transaction>> GetTransactions()
    {
        return Ok(_transactions);
    }

    [HttpPost]
    public ActionResult<Transaction> AddTransaction([FromBody] Transaction transaction)
    {
        transaction.Id = _transactions.Count > 0 ? _transactions.Max(t => t.Id) + 1 : 1;
        
        if (transaction.CategoryId == null || transaction.CategoryId == 0)
        {
            transaction.CategoryId = _categoryPredictor.PredictCategory(transaction.Description);
            transaction.IsAutoCategorized = true;
        }

        var anomalyResult = _anomalyDetector.DetectAnomaly(transaction);
        transaction.IsAnomaly = anomalyResult.IsAnomaly;
        transaction.AnomalyReason = anomalyResult.Reason;

        _transactions.Add(transaction);
        return Ok(transaction);
    }

    [HttpPut("{id}/category")]
    public IActionResult UpdateCategory(int id, [FromBody] int categoryId)
    {
        var tx = _transactions.FirstOrDefault(t => t.Id == id);
        if (tx == null) return NotFound();

        tx.CategoryId = categoryId;
        tx.IsAutoCategorized = false;
        return NoContent();
    }
}
''')

print("Backend files generated successfully.")
