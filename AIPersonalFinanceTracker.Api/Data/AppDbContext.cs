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

        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Groceries", Type = "Expense", MonthlyBudgetLimit = 500.00m },
            new Category { Id = 2, Name = "Utilities", Type = "Expense", MonthlyBudgetLimit = 200.00m },
            new Category { Id = 3, Name = "Salary", Type = "Income", MonthlyBudgetLimit = 0.00m },
            new Category { Id = 4, Name = "Entertainment", Type = "Expense", MonthlyBudgetLimit = 150.00m },
            new Category { Id = 5, Name = "Dining Out", Type = "Expense", MonthlyBudgetLimit = 300.00m }
        );

        modelBuilder.Entity<Transaction>().HasData(
            new Transaction { Id = 1, Description = "Monthly Salary Direct Deposit", Amount = 5000.00m, Date = DateTime.UtcNow.AddDays(-10), IsAutoCategorized = true, CategoryId = 3 },
            new Transaction { Id = 2, Description = "Water Utility Bill", Amount = -100.00m, Date = DateTime.UtcNow.AddDays(-8), IsAutoCategorized = true, CategoryId = 2 },
            new Transaction { Id = 3, Description = "Target Home Goods & Groceries", Amount = -150.00m, Date = DateTime.UtcNow.AddDays(-5), IsAutoCategorized = true, CategoryId = 1 },
            new Transaction { Id = 4, Description = "Uber Trip to Airport", Amount = -2000.00m, Date = DateTime.UtcNow.AddDays(-2), IsAutoCategorized = true, IsAnomaly = true, AnomalyReason = "High Expense Spike: $2000.00 exceeds normal category baseline.", CategoryId = 5 },
            new Transaction { Id = 5, Description = "Cinema Movie Tickets", Amount = -45.00m, Date = DateTime.UtcNow.AddDays(-1), IsAutoCategorized = true, CategoryId = 4 }
        );
    }
}
