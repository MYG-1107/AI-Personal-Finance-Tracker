using AIPersonalFinanceTracker.Shared.Models;

namespace AIPersonalFinanceTracker.ML;

public class AnomalyResult
{
    public bool IsAnomaly { get; set; }
    public decimal Score { get; set; }
    public string Reason { get; set; } = "";
}

public class AnomalyDetectionService
{
    public AnomalyResult DetectAnomaly(Transaction transaction)
    {
        return DetectAnomaly(transaction, Array.Empty<Transaction>());
    }

    public AnomalyResult DetectAnomaly(Transaction transaction, IEnumerable<Transaction> history)
    {
        if (transaction.Amount >= 0)
        {
            return new AnomalyResult();
        }

        var comparableAmounts = history
            .Where(item => item.Amount < 0)
            .Select(item => Math.Abs(item.Amount))
            .Where(amount => amount > 0)
            .ToList();
        var amount = Math.Abs(transaction.Amount);
        var reasons = new List<string>();
        var score = 0m;

        if (amount > 1000m)
        {
            reasons.Add("high-value transaction over 1,000 currency units");
            score += 35m;
        }

        if (comparableAmounts.Count >= 5)
        {
            var average = comparableAmounts.Average();
            var variance = comparableAmounts.Sum(value => Math.Pow((double)(value - average), 2)) / comparableAmounts.Count;
            var standardDeviation = (decimal)Math.Sqrt((double)variance);
            if (standardDeviation > 0 && amount > average + (2m * standardDeviation))
            {
                reasons.Add("amount is more than two standard deviations above spending baseline");
                score += 40m;
            }
        }

        var sameDayCount = history.Count(item => item.Date.Date == transaction.Date.Date);
        if (sameDayCount >= 8)
        {
            reasons.Add("unusually high transaction velocity for the day");
            score += 15m;
        }

        var hour = transaction.Date.Hour;
        if (hour < 5 || hour >= 23)
        {
            reasons.Add("transaction occurred during an unusual spending hour");
            score += 10m;
        }

        return new AnomalyResult
        {
            IsAnomaly = reasons.Count > 0,
            Score = Math.Min(100m, score),
            Reason = reasons.Count == 0
                ? string.Empty
                : $"Risk score {Math.Min(100m, score):N0}/100: {string.Join("; ", reasons)}"
        };
    }
}
