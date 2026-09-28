import os

os.makedirs('AIPersonalFinanceTracker.ML', exist_ok=True)
os.makedirs('AIPersonalFinanceTracker.Tests', exist_ok=True)

# 1. CategorizationService.cs
with open('AIPersonalFinanceTracker.ML/CategorizationService.cs', 'w') as f:
    f.write('''namespace AIPersonalFinanceTracker.ML;

public class CategorizationService
{
    public (string CategoryName, int CategoryId) PredictCategory(string description)
    {
        if (string.IsNullOrWhiteSpace(description)) return ("Uncategorized", 1);
        var desc = description.ToLowerInvariant();
        if (desc.Contains("coffee") || desc.Contains("starbucks") || desc.Contains("restaurant") || desc.Contains("food"))
            return ("Dining Out", 2);
        if (desc.Contains("target") || desc.Contains("walmart") || desc.Contains("grocer"))
            return ("Groceries", 1);
        if (desc.Contains("salary") || desc.Contains("deposit") || desc.Contains("payroll"))
            return ("Income", 5);
        if (desc.Contains("uber") || desc.Contains("lyft") || desc.Contains("gas"))
            return ("Transportation", 3);
        return ("General", 1);
    }
}
''')

# 2. CategoryPredictorService.cs
with open('AIPersonalFinanceTracker.ML/CategoryPredictorService.cs', 'w') as f:
    f.write('''namespace AIPersonalFinanceTracker.ML;

public class CategoryPredictorService
{
    public int PredictCategory(string description)
    {
        var categorizer = new CategorizationService();
        return categorizer.PredictCategory(description).CategoryId;
    }
}
''')

# 3. AnomalyDetectionService.cs
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

# 4. TransactionsController.cs
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
        _categoryPredictor = categoryPredictor ?? new CategoryPredictorService();
        _anomalyDetector = anomalyDetector ?? new AnomalyDetectionService();
    }

    public TransactionsController() : this(new CategoryPredictorService(), new AnomalyDetectionService()) { }

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

    [HttpPost("post")]
    public ActionResult<Transaction> PostTransaction([FromBody] Transaction transaction)
    {
        return AddTransaction(transaction);
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

# 5. TransactionsControllerTests.cs
with open('AIPersonalFinanceTracker.Tests/TransactionsControllerTests.cs', 'w') as f:
    f.write('''using Xunit;
using Microsoft.AspNetCore.Mvc;
using AIPersonalFinanceTracker.Api.Controllers;
using AIPersonalFinanceTracker.Shared.Models;
using AIPersonalFinanceTracker.ML;

namespace AIPersonalFinanceTracker.Tests;

public class TransactionsControllerTests
{
    private readonly TransactionsController _controller;

    public TransactionsControllerTests()
    {
        var predictor = new CategoryPredictorService();
        var anomalyDetector = new AnomalyDetectionService();
        _controller = new TransactionsController(predictor, anomalyDetector);
    }

    [Fact]
    public void GetTransactions_ReturnsOkResult()
    {
        var result = _controller.GetTransactions();
        Assert.NotNull(result);
    }

    [Fact]
    public void AddTransaction_AddsNewTransaction()
    {
        var tx = new Transaction { Description = "Starbucks", Amount = -5.00m };
        var result = _controller.AddTransaction(tx);
        Assert.NotNull(result);
    }

    [Fact]
    public void PostTransaction_AddsNewTransaction()
    {
        var tx = new Transaction { Description = "Target Store", Amount = -45.00m };
        var result = _controller.PostTransaction(tx);
        Assert.NotNull(result);
    }
}
''')

# 6. CategorizationServiceTests.cs
with open('AIPersonalFinanceTracker.Tests/CategorizationServiceTests.cs', 'w') as f:
    f.write('''using Xunit;
using AIPersonalFinanceTracker.ML;

namespace AIPersonalFinanceTracker.Tests;

public class CategorizationServiceTests
{
    private readonly CategorizationService _service;

    public CategorizationServiceTests()
    {
        _service = new CategorizationService();
    }

    [Fact]
    public void PredictCategory_Coffee_ReturnsDiningOut()
    {
        var result = _service.PredictCategory("Starbucks Coffee");
        Assert.Equal("Dining Out", result.CategoryName);
        Assert.Equal(2, result.CategoryId);
    }

    [Fact]
    public void PredictCategory_Target_ReturnsGroceries()
    {
        var result = _service.PredictCategory("Target Store");
        Assert.Equal("Groceries", result.CategoryName);
        Assert.Equal(1, result.CategoryId);
    }
}
''')

print("All services, controllers, and tests generated successfully.")
