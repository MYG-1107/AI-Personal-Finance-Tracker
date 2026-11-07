using AIPersonalFinanceTracker.Shared;

namespace AIPersonalFinanceTracker.ML;

public class CategorizationService
{
    public (string CategoryName, int CategoryId) PredictCategory(string description)
    {
        var prediction = TransactionCategoryClassifier.Predict(description);
        return (prediction.CategoryName, prediction.CategoryId);
    }
}
