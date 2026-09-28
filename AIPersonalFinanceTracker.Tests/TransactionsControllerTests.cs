using Xunit;
using Microsoft.AspNetCore.Mvc;
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
        _controller = new TransactionsController(predictor, anomalyDetector);
    }

    [Fact]
    public void GetTransactions_ReturnsOkResult()
    {
        var result = _controller.GetTransactions();
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
}
