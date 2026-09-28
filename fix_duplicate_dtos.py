import os, glob

# 1. Clean up AIPersonalFinanceTracker.Shared/Models directory
shared_models_dir = 'AIPersonalFinanceTracker.Shared/Models'
if os.path.exists(shared_models_dir):
    for f in glob.glob(os.path.join(shared_models_dir, "*.cs")):
        os.remove(f)

os.makedirs(shared_models_dir, exist_ok=True)

# 2. Write Transaction.cs
with open(os.path.join(shared_models_dir, 'Transaction.cs'), 'w', encoding='utf-8') as f:
    f.write("""namespace AIPersonalFinanceTracker.Shared.Models;

public class Transaction
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public bool IsAutoCategorized { get; set; } = true;
    public bool IsAnomaly { get; set; }
    public string? AnomalyReason { get; set; }
    public int? CategoryId { get; set; }
    public Category? Category { get; set; }
}
""")

# 3. Write Category.cs
with open(os.path.join(shared_models_dir, 'Category.cs'), 'w', encoding='utf-8') as f:
    f.write("""namespace AIPersonalFinanceTracker.Shared.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "Expense";
    public decimal MonthlyBudgetLimit { get; set; }
}
""")

# 4. Write UpgradeDtos.cs (Consolidated DTOs without duplicates)
with open(os.path.join(shared_models_dir, 'UpgradeDtos.cs'), 'w', encoding='utf-8') as f:
    f.write("""namespace AIPersonalFinanceTracker.Shared.Models;

public class FinancialHealthDto
{
    public int HealthScore { get; set; } = 85;
    public string HealthGrade { get; set; } = "A";
    public decimal SavingsRate { get; set; }
    public decimal BudgetAdherenceRate { get; set; }
    public List<string> Recommendations { get; set; } = new();
}

public class AnomalyCheckResult
{
    public bool IsAnomaly { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class ReceiptScanResultDto
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string SuggestedCategory { get; set; } = "General";
    public int SuggestedCategoryId { get; set; } = 1;
}

public class CashFlowForecastDto
{
    public decimal ProjectedEndOfMonthBalance { get; set; }
    public decimal PredictedDailyBurnRate { get; set; }
    public List<CategoryForecastDto> CategoryProjections { get; set; } = new();
    public List<string> ForecastWarnings { get; set; } = new();
}

public class CategoryForecastDto
{
    public string CategoryName { get; set; } = string.Empty;
    public decimal CurrentSpent { get; set; }
    public decimal ProjectedSpent { get; set; }
    public decimal BudgetLimit { get; set; }
    public bool IsProjectedOverrun { get; set; }
}
""")

# 5. Ensure CategoriesController exists in API project
cat_ctrl_path = 'AIPersonalFinanceTracker.Api/Controllers/CategoriesController.cs'
with open(cat_ctrl_path, 'w', encoding='utf-8') as f:
    f.write("""using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AIPersonalFinanceTracker.Api.Data;
using AIPersonalFinanceTracker.Shared.Models;

namespace AIPersonalFinanceTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _context;

    public CategoriesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
    {
        return await _context.Categories.ToListAsync();
    }
}
""")

print("Cleaned up Shared Models directory and removed duplicate DTO definitions.")
