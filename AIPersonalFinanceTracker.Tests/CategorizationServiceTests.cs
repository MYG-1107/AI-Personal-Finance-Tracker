using AIPersonalFinanceTracker.ML;
using Xunit;

namespace AIPersonalFinanceTracker.Tests;

public class CategorizationServiceTests
{
    [Theory]
    [InlineData("Starbucks Coffee", "Dining Out")]
    [InlineData("Walmart Grocery Store", "Groceries")]
    [InlineData("Electric Utility Bill", "Utilities")]
    [InlineData("Netflix Subscription", "Entertainment")]
    public void PredictCategory_ShouldReturnExpectedCategory(string description, string expectedCategory)
    {
        var service = new CategorizationService();

        var result = service.PredictCategory(description);

        Assert.Equal(expectedCategory, result);
    }

    [Fact]
    public void LearnFromOverride_ShouldUpdateModelPrediction()
    {
        var service = new CategorizationService();
        string customDescription = "Tech Corp Salary Ref 99021";
        string newCategory = "Salary";

        service.LearnFromOverride(customDescription, newCategory);
        var result = service.PredictCategory(customDescription);

        Assert.Equal(newCategory, result);
    }
}
