namespace AIPersonalFinanceTracker.ML;

public class CategoryPredictorService
{
    public int PredictCategory(string description)
    {
        var categorizer = new CategorizationService();
        return categorizer.PredictCategory(description).CategoryId;
    }
}
