using AIPersonalFinanceTracker.Api.Controllers;
using AIPersonalFinanceTracker.Api.Data;
using AIPersonalFinanceTracker.ML;
using AIPersonalFinanceTracker.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AIPersonalFinanceTracker.Tests;

public class TransactionsControllerTests
{
    private AppDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);
        context.Categories.Add(new Category { Id = 1, Name = "Groceries", Type = "Expense", MonthlyBudgetLimit = 500 });
        context.SaveChanges();
        return context;
    }

    [Fact]
    public async Task PostTransaction_ShouldAutoCategorize()
    {
        var context = GetInMemoryDbContext();
        var mlService = new CategorizationService();
        var controller = new TransactionsController(context, mlService);

        var newTransaction = new Transaction
        {
            Description = "Walmart Grocery Store",
            Amount = 45.50m
        };

        var result = await controller.PostTransaction(newTransaction);

        var actionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnedTransaction = Assert.IsType<Transaction>(actionResult.Value);
        Assert.Equal(1, returnedTransaction.CategoryId);
        Assert.True(returnedTransaction.IsAutoCategorized);
    }
}
