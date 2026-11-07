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
    public void PredictCategory_Coffee_ReturnsGroceriesAndFood()
    {
        var result = _service.PredictCategory("Starbucks Coffee");
        Assert.Equal("Groceries & Food", result.CategoryName);
        Assert.Equal(1, result.CategoryId);
    }

    [Fact]
    public void PredictCategory_Target_ReturnsGroceries()
    {
        var result = _service.PredictCategory("Target Store");
        Assert.Equal("Groceries & Food", result.CategoryName);
        Assert.Equal(1, result.CategoryId);
    }

    [Theory]
    [InlineData("gas", 5)]
    [InlineData("gas bill", 2)]
    [InlineData("Uber ride", 5)]
    [InlineData("Cinema tickets", 4)]
    public void PredictCategory_UsesSpecificCategoryRules(string description, int expectedCategoryId)
    {
        var result = _service.PredictCategory(description);

        Assert.Equal(expectedCategoryId, result.CategoryId);
    }

    [Fact]
    public void PredictCategory_UnknownDescription_ReturnsUncategorized()
    {
        var result = _service.PredictCategory("ball");

        Assert.Equal("Uncategorized", result.CategoryName);
        Assert.Equal(0, result.CategoryId);
    }
}
