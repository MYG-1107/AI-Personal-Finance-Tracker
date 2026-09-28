using Xunit;
using AIPersonalFinanceTracker.ML;

namespace AIPersonalFinanceTracker.Tests;

public class CategorizationServiceTests
{
    private readonly CategorizationService _service;

    public CategorizationServiceTests()
    {
        _service = new CategorizationService();
    }

    [Fact]
    public void PredictCategory_Coffee_ReturnsDiningOut()
    {
        var result = _service.PredictCategory("Starbucks Coffee");
        Assert.Equal("Dining Out", result.CategoryName);
        Assert.Equal(2, result.CategoryId);
    }

    [Fact]
    public void PredictCategory_Target_ReturnsGroceries()
    {
        var result = _service.PredictCategory("Target Store");
        Assert.Equal("Groceries", result.CategoryName);
        Assert.Equal(1, result.CategoryId);
    }
}
