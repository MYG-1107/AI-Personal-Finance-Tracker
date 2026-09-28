import os

# 1. Update TransactionsController.cs (Single Constructor)
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

# 2. Update Program.cs (DI & Blazor Static File Routing)
with open('AIPersonalFinanceTracker.Api/Program.cs', 'w') as f:
    f.write('''using AIPersonalFinanceTracker.ML;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Register Services in DI Container
builder.Services.AddScoped<CategoryPredictorService>();
builder.Services.AddScoped<AnomalyDetectionService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseHsts();
}

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
''')

print("Backend Controller and Program.cs updated successfully.")
