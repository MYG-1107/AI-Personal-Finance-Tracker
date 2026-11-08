using AIPersonalFinanceTracker.ML;
using AIPersonalFinanceTracker.Shared.Models;

namespace AIPersonalFinanceTracker.Tests;

public class AnomalyDetectionServiceTests
{
    [Fact]
    public void DetectAnomaly_FlagsUnusualAmountAgainstHistory()
    {
        var history = Enumerable.Range(1, 6)
            .Select(day => new Transaction
            {
                Amount = -(10m + day),
                Date = DateTime.UtcNow.Date.AddDays(-day).AddHours(12)
            })
            .ToList();

        var result = new AnomalyDetectionService().DetectAnomaly(
            new Transaction { Amount = -100m, Date = DateTime.UtcNow.Date.AddHours(12) }, history);

        Assert.True(result.IsAnomaly);
        Assert.Contains("standard deviations", result.Reason);
    }

    [Fact]
    public void DetectAnomaly_FlagsLateNightTransaction()
    {
        var result = new AnomalyDetectionService().DetectAnomaly(
            new Transaction { Amount = -10m, Date = new DateTime(2026, 11, 8, 2, 0, 0, DateTimeKind.Utc) });

        Assert.True(result.IsAnomaly);
        Assert.Contains("unusual spending hour", result.Reason);
    }

    [Fact]
    public void DetectAnomaly_IgnoresIncome()
    {
        var result = new AnomalyDetectionService().DetectAnomaly(
            new Transaction { Amount = 5000m, Date = new DateTime(2026, 11, 8, 2, 0, 0, DateTimeKind.Utc) });

        Assert.False(result.IsAnomaly);
        Assert.Equal(0m, result.Score);
        Assert.Empty(result.Reason);
    }
}