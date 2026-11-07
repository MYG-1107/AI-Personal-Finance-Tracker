namespace AIPersonalFinanceTracker.Shared;

public sealed record TransactionCategoryPrediction(int CategoryId, string CategoryName)
{
    public bool IsKnown => CategoryId > 0;
}

public static class TransactionCategoryClassifier
{
    public static TransactionCategoryPrediction Predict(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return Unknown;

        var value = description.Trim().ToLowerInvariant();

        if (ContainsAny(value, "salary", "payroll", "direct deposit", "income", "bonus"))
            return new(3, "Salary & Income");

        if (ContainsAny(value, "gas bill", "gas utility", "electric", "electricity", "water",
                "utility", "utilities", "internet", "phone bill", "rent", "mortgage", "insurance"))
            return new(2, "Utilities & Bills");

        if (ContainsAny(value, "uber", "lyft", "taxi", "fuel", "petrol", "gasoline", "gas",
                "gas station", "parking", "toll", "flight", "airline", "travel", "trip",
                "bike", "bicycle", "car", "vehicle", "bus", "train"))
            return new(5, "Transport & Travel");

        if (ContainsAny(value, "movie", "cinema", "ticket", "netflix", "spotify", "concert", "game"))
            return new(4, "Entertainment");

        if (ContainsAny(value, "grocery", "groceries", "supermarket", "walmart", "target",
                "restaurant", "cafe", "coffee", "starbucks", "dining", "food", "fruit",
                "grape", "produce", "bakery", "vegetable", "market"))
            return new(1, "Groceries & Food");

        return Unknown;
    }

    private static bool ContainsAny(string value, params string[] terms) =>
        terms.Any(term => value.Contains(term, StringComparison.Ordinal));

    private static readonly TransactionCategoryPrediction Unknown = new(0, "Uncategorized");
}
