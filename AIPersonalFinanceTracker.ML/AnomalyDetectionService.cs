using AIPersonalFinanceTracker.Shared.Models;

namespace AIPersonalFinanceTracker.ML;

public class AnomalyResult
{
    public bool IsAnomaly { get; set; }
    public string Reason { get; set; } = "";
}

public class AnomalyDetectionService
{
    public AnomalyResult DetectAnomaly(Transaction transaction)
    {
        if (Math.Abs(transaction.Amount) > 1000m)
        {
            return new AnomalyResult { IsAnomaly = true, Reason = "High-value transaction over $1,000" };
        }
        return new AnomalyResult { IsAnomaly = false, Reason = "" };
    }
}
