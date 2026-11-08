using System.Text.RegularExpressions;
using AIPersonalFinanceTracker.Shared.Models;

namespace AIPersonalFinanceTracker.Api.Services;

public class SubscriptionAnalyticsService
{
    public SubscriptionAnalyticsDto Analyze(IEnumerable<Transaction> transactions, DateTime now)
    {
        var candidates = transactions
            .Where(transaction => transaction.Amount < 0)
            .GroupBy(transaction => NormalizeMerchant(transaction.Description))
            .Where(group => group.Any(transaction => transaction.IsRecurring) || group.Count() >= 2)
            .Select(group => BuildSubscription(group.Key, group.ToList(), now))
            .OrderByDescending(subscription => subscription.EstimatedMonthlyCost)
            .ToList();

        return new SubscriptionAnalyticsDto
        {
            MonthlyBurnRate = candidates.Sum(subscription => subscription.EstimatedMonthlyCost),
            AnnualizedBurnRate = candidates.Sum(subscription => subscription.EstimatedMonthlyCost) * 12,
            Subscriptions = candidates
        };
    }

    private static SubscriptionItemDto BuildSubscription(
        string merchant,
        List<Transaction> transactions,
        DateTime now)
    {
        var charges = transactions.Select(transaction => Math.Abs(transaction.Amount)).ToList();
        var averageCharge = charges.Average();
        var dates = transactions.OrderBy(transaction => transaction.Date).Select(transaction => transaction.Date).ToList();
        var averageInterval = dates.Count > 1
            ? dates.Zip(dates.Skip(1), (previous, current) => (current - previous).TotalDays).Average()
            : 30;
        var monthlyCost = averageCharge * (30m / Math.Max(1, (decimal)averageInterval));
        var latest = transactions.OrderByDescending(transaction => transaction.Date).First();
        var nextRenewal = latest.NextOccurrence ?? latest.Date.AddDays(Math.Max(1, averageInterval));

        return new SubscriptionItemDto
        {
            Merchant = merchant,
            AverageCharge = decimal.Round(averageCharge, 2),
            EstimatedMonthlyCost = decimal.Round(monthlyCost, 2),
            Occurrences = transactions.Count,
            NextRenewal = nextRenewal >= now.Date ? nextRenewal : nextRenewal.AddDays(Math.Max(1, averageInterval)),
            IsExplicitlyRecurring = transactions.Any(transaction => transaction.IsRecurring)
        };
    }

    private static string NormalizeMerchant(string description)
    {
        var normalized = Regex.Replace(description.Trim().ToLowerInvariant(), "[^a-z0-9 ]", " ");
        return Regex.Replace(normalized, "\\s+", " ").Trim();
    }
}