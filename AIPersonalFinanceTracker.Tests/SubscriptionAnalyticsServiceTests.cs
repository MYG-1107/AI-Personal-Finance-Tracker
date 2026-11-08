using AIPersonalFinanceTracker.Api.Services;
using AIPersonalFinanceTracker.Shared.Models;

namespace AIPersonalFinanceTracker.Tests;

public class SubscriptionAnalyticsServiceTests
{
    [Fact]
    public void Analyze_GroupsRepeatedChargesAndProjectsMonthlyBurn()
    {
        var now = new DateTime(2026, 11, 8, 12, 0, 0, DateTimeKind.Utc);
        var transactions = new List<Transaction>
        {
            new() { Description = "StreamFlix", Amount = -15m, Date = now.AddMonths(-2) },
            new() { Description = "StreamFlix", Amount = -15m, Date = now.AddMonths(-1) },
            new() { Description = "StreamFlix", Amount = -15m, Date = now.AddDays(-2) }
        };

        var result = new SubscriptionAnalyticsService().Analyze(transactions, now);

        var subscription = Assert.Single(result.Subscriptions);
        Assert.Equal("streamflix", subscription.Merchant);
        Assert.Equal(15m, subscription.AverageCharge);
        Assert.InRange(result.MonthlyBurnRate, 14m, 16m);
    }
}