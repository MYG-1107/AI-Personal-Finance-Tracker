using AIPersonalFinanceTracker.Shared.Models;

namespace AIPersonalFinanceTracker.ML;

public class AnomalyDetectionService
{
    public AnomalyCheckResult DetectAnomaly(decimal amount, string description, List<Transaction> history)
    {
        var result = new AnomalyCheckResult();
        var absAmount = Math.Abs(amount);

        if (absAmount > 1500 && !description.Contains("Salary", StringComparison.OrdinalIgnoreCase))
        {
            result.IsAnomaly = true;
            result.Reason = $"High Expense Spike: ${absAmount:F2} exceeds expected baseline threshold.";
            return result;
        }

        var recentDuplicates = history.Where(t => 
            Math.Abs(t.Amount) == absAmount && 
            t.Description.Equals(description, StringComparison.OrdinalIgnoreCase) &&
            (DateTime.UtcNow - t.Date).TotalDays < 2).ToList();

        if (recentDuplicates.Any())
        {
            result.IsAnomaly = true;
            result.Reason = "Potential Duplicate Charge detected within 48 hours.";
            return result;
        }

        return result;
    }
}
