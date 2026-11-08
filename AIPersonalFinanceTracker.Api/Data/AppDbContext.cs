using Microsoft.EntityFrameworkCore;
using AIPersonalFinanceTracker.Shared.Models;

namespace AIPersonalFinanceTracker.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<SavingsGoal> SavingsGoals => Set<SavingsGoal>();
    public DbSet<BudgetWorkspace> BudgetWorkspaces => Set<BudgetWorkspace>();
    public DbSet<BudgetMember> BudgetMembers => Set<BudgetMember>();
    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserProfile>().HasData(new UserProfile
        {
            Id = 1,
            CountryCode = "US",
            CurrencyCode = "USD",
            TimeZone = "UTC",
            MonthlyBudgetTarget = 0,
            UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Groceries", Type = "Expense", MonthlyBudgetLimit = 500.00m },
            new Category { Id = 2, Name = "Utilities", Type = "Expense", MonthlyBudgetLimit = 200.00m },
            new Category { Id = 3, Name = "Salary", Type = "Income", MonthlyBudgetLimit = 0.00m },
            new Category { Id = 4, Name = "Entertainment", Type = "Expense", MonthlyBudgetLimit = 150.00m },
            new Category { Id = 5, Name = "Transport & Travel", Type = "Expense", MonthlyBudgetLimit = 300.00m }
        );

    }
}
