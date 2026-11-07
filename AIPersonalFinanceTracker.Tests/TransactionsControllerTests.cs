using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using AIPersonalFinanceTracker.Api.Data;
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
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new AppDbContext(options);
        context.Database.EnsureCreated();
        _controller = new TransactionsController(context, predictor, anomalyDetector);
    }

    [Fact]
    public async Task GetTransactions_ReturnsOkResult()
    {
        var result = await _controller.GetTransactions();
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

    [Fact]
    public async Task AddTransaction_RejectsZeroAmount()
    {
        var result = await _controller.AddTransaction(new TransactionCreateDto
        {
            Description = "Bike",
            Amount = 0
        });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public void DependencyInjection_CanActivateController()
    {
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        services.AddSingleton(new CategoryPredictorService());
        services.AddSingleton(new AnomalyDetectionService());
        using var provider = services.BuildServiceProvider();

        var controller = ActivatorUtilities.CreateInstance<TransactionsController>(provider);

        Assert.IsType<TransactionsController>(controller);
    }
}
