using AIPersonalFinanceTracker.Api.Services;
using AIPersonalFinanceTracker.Shared.Models;

namespace AIPersonalFinanceTracker.Tests;

public class FinancialAdvisorServiceTests
{
    [Fact]
    public void Answer_UsesRetrievedTransactionsAndGoals()
    {
        var transactions = new List<Transaction>
        {
            new() { Description = "Rent", Amount = -900m, CategoryId = 2, Date = DateTime.UtcNow },
            new() { Description = "Salary", Amount = 3000m, CategoryId = 3, Date = DateTime.UtcNow }
        };
        var goals = new List<SavingsGoal>
        {
            new() { Name = "Emergency fund", CurrentAmount = 100m, TargetAmount = 500m }
        };

        var response = new FinancialAdvisorService().Answer(
            "How am I doing on my savings goal?",
            transactions,
            new Dictionary<int, string> { [2] = "Housing", [3] = "Salary" },
            goals);

        Assert.Contains("Emergency fund", response.Answer);
        Assert.Contains("400.00", response.Answer);
    }
}