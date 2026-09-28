namespace AIPersonalFinanceTracker.ML;

public class CategorizationService
{
    private readonly Dictionary<string, (string CategoryName, int CategoryId)> _keywordRules = new(StringComparer.OrdinalIgnoreCase)
    {
        { "water", ("Utilities", 2) },
        { "electricity", ("Utilities", 2) },
        { "utility", ("Utilities", 2) },
        { "uber", ("Dining Out", 5) },
        { "target", ("Groceries", 1) },
        { "walmart", ("Groceries", 1) },
        { "grocery", ("Groceries", 1) },
        { "salary", ("Salary", 3) },
        { "deposit", ("Salary", 3) },
        { "cinema", ("Entertainment", 4) },
        { "movie", ("Entertainment", 4) },
        { "tickets", ("Entertainment", 4) },
        { "starbucks", ("Dining Out", 5) },
        { "coffee", ("Dining Out", 5) }
    };

    public (string CategoryName, int CategoryId) PredictCategory(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return ("Groceries", 1);

        foreach (var rule in _keywordRules)
        {
            if (description.Contains(rule.Key, StringComparison.OrdinalIgnoreCase))
                return rule.Value;
        }

        return ("Groceries", 1);
    }

    public void LearnFromOverride(string description, string category)
    {
        // Dynamic ML weight update hook
    }
}
