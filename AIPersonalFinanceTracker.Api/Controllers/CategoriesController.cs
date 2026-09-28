using Microsoft.AspNetCore.Mvc;
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
