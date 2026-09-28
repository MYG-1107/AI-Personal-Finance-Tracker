namespace AIPersonalFinanceTracker.ML;

public class CategorizationService
{
    public (string CategoryName, int CategoryId) PredictCategory(string description)
    {
        if (string.IsNullOrWhiteSpace(description)) return ("Uncategorized", 1);
        var desc = description.ToLowerInvariant();
        if (desc.Contains("coffee") || desc.Contains("starbucks") || desc.Contains("restaurant") || desc.Contains("food"))
            return ("Dining Out", 2);
        if (desc.Contains("target") || desc.Contains("walmart") || desc.Contains("grocer"))
            return ("Groceries", 1);
        if (desc.Contains("salary") || desc.Contains("deposit") || desc.Contains("payroll"))
            return ("Income", 5);
        if (desc.Contains("uber") || desc.Contains("lyft") || desc.Contains("gas"))
            return ("Transportation", 3);
        return ("General", 1);
    }
}
