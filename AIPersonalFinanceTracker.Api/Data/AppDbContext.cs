using Microsoft.EntityFrameworkCore;
using AIPersonalFinanceTracker.Shared.Models;

namespace AIPersonalFinanceTracker.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seed default categories
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Groceries", Type = "Expense", MonthlyBudgetLimit = 500 },
            new Category { Id = 2, Name = "Utilities", Type = "Expense", MonthlyBudgetLimit = 200 },
            new Category { Id = 3, Name = "Salary", Type = "Income", MonthlyBudgetLimit = 0 },
            new Category { Id = 4, Name = "Entertainment", Type = "Expense", MonthlyBudgetLimit = 150 },
            new Category { Id = 5, Name = "Dining Out", Type = "Expense", MonthlyBudgetLimit = 300 }
        );
    }
}
