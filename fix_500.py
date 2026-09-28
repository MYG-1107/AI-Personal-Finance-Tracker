import os

# 1. Update Program.cs with DbContext & Service DI Registrations
with open('AIPersonalFinanceTracker.Api/Program.cs', 'w') as f:
    f.write('''using AIPersonalFinanceTracker.ML;
using AIPersonalFinanceTracker.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Register SQLite AppDbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=finance.db"));

// Register ML Services
builder.Services.AddScoped<CategoryPredictorService>();
builder.Services.AddScoped<AnomalyDetectionService>();

var app = builder.Build();

// Ensure Database is Created & Seeded on Startup
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

# 2. Update CategoriesController.cs with Fallback Handling
with open('AIPersonalFinanceTracker.Api/Controllers/CategoriesController.cs', 'w') as f:
    f.write('''using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AIPersonalFinanceTracker.Shared.Models;
using AIPersonalFinanceTracker.Api.Data;

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
    public async Task<ActionResult<List<Category>>> GetCategories()
    {
        try
        {
            if (_context != null && _context.Categories != null)
            {
                var categories = await _context.Categories.ToListAsync();
                if (categories.Count > 0) return Ok(categories);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching categories: {ex.Message}");
        }

        // Return fallback seeded categories if DB query fails or is empty
        return Ok(new List<Category>
        {
            new Category { Id = 1, Name = "Groceries", Type = "Expense", MonthlyBudgetLimit = 500m },
            new Category { Id = 2, Name = "Utilities", Type = "Expense", MonthlyBudgetLimit = 200m },
            new Category { Id = 3, Name = "Salary", Type = "Income", MonthlyBudgetLimit = 0m },
            new Category { Id = 4, Name = "Entertainment", Type = "Expense", MonthlyBudgetLimit = 150m },
            new Category { Id = 5, Name = "Dining Out", Type = "Expense", MonthlyBudgetLimit = 300m }
        });
    }
}
''')

print("DbContext registration and CategoriesController generated successfully.")
