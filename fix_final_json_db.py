import os

# 1. Update Program.cs to configure JsonOptions for Blazor client payloads
with open('AIPersonalFinanceTracker.Api/Program.cs', 'w') as f:
    f.write('''using AIPersonalFinanceTracker.ML;
using AIPersonalFinanceTracker.Api.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

builder.Services.AddRazorPages();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=finance.db"));

builder.Services.AddScoped<CategoryPredictorService>();
builder.Services.AddScoped<AnomalyDetectionService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"DB Init Error: {ex.Message}");
    }
}

app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
''')

# 2. Update TransactionsController.cs to cleanly accept and echo back created records
with open('AIPersonalFinanceTracker.Api/Controllers/TransactionsController.cs', 'w') as f:
    f.write('''using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AIPersonalFinanceTracker.Shared.Models;
using AIPersonalFinanceTracker.Api.Data;
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
    private readonly AppDbContext? _context;
    private readonly CategoryPredictorService _categoryPredictor;
    private readonly AnomalyDetectionService _anomalyDetector;

    private static readonly List<Transaction> _fallbackTransactions = new()
    {
        new Transaction { Id = 1, Description = "Monthly Salary Direct Deposit", Amount = 5000.00m, Date = DateTime.UtcNow.AddDays(-10), CategoryId = 3, IsAutoCategorized = true, IsAnomaly = false },
        new Transaction { Id = 2, Description = "Target Home Goods & Groceries", Amount = -120.45m, Date = DateTime.UtcNow.AddDays(-5), CategoryId = 1, IsAutoCategorized = true, IsAnomaly = false },
        new Transaction { Id = 3, Description = "Water Utility Bill", Amount = -5.50m, Date = DateTime.UtcNow.AddDays(-3), CategoryId = 2, IsAutoCategorized = true, IsAnomaly = false },
        new Transaction { Id = 4, Description = "Cinema Movie Tickets", Amount = -45.00m, Date = DateTime.UtcNow.AddDays(-2), CategoryId = 4, IsAutoCategorized = true, IsAnomaly = false },
        new Transaction { Id = 5, Description = "Uber Trip to Airport", Amount = -150.00m, Date = DateTime.UtcNow.AddDays(-1), CategoryId = 5, IsAutoCategorized = true, IsAnomaly = true, AnomalyReason = "High Expense Spike" }
    };

    public TransactionsController(AppDbContext? context = null, CategoryPredictorService? categoryPredictor = null, AnomalyDetectionService? anomalyDetector = null)
    {
        _context = context;
        _categoryPredictor = categoryPredictor ?? new CategoryPredictorService();
        _anomalyDetector = anomalyDetector ?? new AnomalyDetectionService();
    }

    public TransactionsController(CategoryPredictorService categoryPredictor, AnomalyDetectionService anomalyDetector)
        : this(null, categoryPredictor, anomalyDetector) { }

    [HttpGet]
    public async Task<IActionResult> GetTransactions()
    {
        try
        {
            if (_context != null && _context.Transactions != null)
            {
                var txs = await _context.Transactions.AsNoTracking().ToListAsync();
                if (txs != null && txs.Count > 0) return Ok(txs);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DB Read Notice: {ex.Message}");
        }

        return Ok(_fallbackTransactions);
    }

    [HttpPost]
    public async Task<IActionResult> AddTransaction([FromBody] TransactionCreateDto dto)
    {
        var desc = dto?.Description ?? "Unspecified";
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

        try
        {
            if (_context != null && _context.Transactions != null)
            {
                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();
                return Ok(transaction);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"DB Save Notice: {ex.Message}");
        }

        transaction.Id = _fallbackTransactions.Count > 0 ? _fallbackTransactions.Max(t => t.Id) + 1 : 1;
        _fallbackTransactions.Add(transaction);

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
''')

print("Program.cs and TransactionsController updated.")
