using Microsoft.ML;
using AIPersonalFinanceTracker.Shared.ML;

namespace AIPersonalFinanceTracker.ML;

public class CategorizationService
{
    private readonly MLContext _mlContext;
    private ITransformer? _model;
    private PredictionEngine<TransactionData, CategoryPrediction>? _predictionEngine;

    public CategorizationService()
    {
        _mlContext = new MLContext(seed: 0);
        TrainInitialModel();
    }

    private void TrainInitialModel()
    {
        var sampleData = new List<TransactionData>
        {
            new() { Description = "Starbucks Coffee", Category = "Dining Out" },
            new() { Description = "Walmart Supercenter Groceries", Category = "Groceries" },
            new() { Description = "Electric Utility Bill", Category = "Utilities" },
            new() { Description = "Monthly Salary Deposit", Category = "Salary" },
            new() { Description = "AMC Movie Tickets", Category = "Entertainment" },
            new() { Description = "Uber Ride", Category = "Transportation" },
            new() { Description = "McDonalds Fast Food", Category = "Dining Out" },
            new() { Description = "Target Market Grocery", Category = "Groceries" },
            new() { Description = "Netflix Subscription", Category = "Entertainment" },
            new() { Description = "Water Bill Payment", Category = "Utilities" }
        };

        var trainingData = _mlContext.Data.LoadFromEnumerable(sampleData);

        var pipeline = _mlContext.Transforms.Conversion.MapValueToKey("Label", nameof(TransactionData.Category))
            .Append(_mlContext.Transforms.Text.FeaturizeText("Features", nameof(TransactionData.Description)))
            .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy("Label", "Features"))
            .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel", "Label"));

        _model = pipeline.Fit(trainingData);
        _predictionEngine = _mlContext.Model.CreatePredictionEngine<TransactionData, CategoryPrediction>(_model);
    }

    public string PredictCategory(string description)
    {
        if (_predictionEngine == null || string.IsNullOrWhiteSpace(description))
            return "Uncategorized";

        var prediction = _predictionEngine.Predict(new TransactionData { Description = description });
        return string.IsNullOrEmpty(prediction.PredictedCategory) ? "Uncategorized" : prediction.PredictedCategory;
    }
}
