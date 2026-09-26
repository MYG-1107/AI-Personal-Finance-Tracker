using Microsoft.ML;
using Microsoft.ML.Data;

namespace AIPersonalFinanceTracker.ML;

public class TransactionData
{
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}

public class TransactionPrediction
{
    [ColumnName("PredictedLabel")]
    public string PredictedCategory { get; set; } = string.Empty;
}

public class CategorizationService
{
    private readonly MLContext _mlContext;
    private ITransformer? _model;
    private PredictionEngine<TransactionData, TransactionPrediction>? _predictionEngine;

    public CategorizationService()
    {
        _mlContext = new MLContext(seed: 0);
        TrainModel();
    }

    private void TrainModel()
    {
        var trainingData = new List<TransactionData>
        {
            // Groceries
            new() { Description = "Walmart Groceries", Category = "Groceries" },
            new() { Description = "Supermarket Food", Category = "Groceries" },
            new() { Description = "Organic Groceries", Category = "Groceries" },
            new() { Description = "Trader Joes Market", Category = "Groceries" },
            new() { Description = "Whole Foods Market", Category = "Groceries" },
            new() { Description = "Boost", Category = "Groceries" },

            // Utilities
            new() { Description = "Electric Bill", Category = "Utilities" },
            new() { Description = "Water Utility Service", Category = "Utilities" },
            new() { Description = "Internet Bill", Category = "Utilities" },
            new() { Description = "Electricity Payment", Category = "Utilities" },
            new() { Description = "Gas & Power Utility", Category = "Utilities" },

            // Dining Out
            new() { Description = "Starbucks Coffee", Category = "Dining Out" },
            new() { Description = "McDonalds Restaurant", Category = "Dining Out" },
            new() { Description = "Subway Sandwich", Category = "Dining Out" },
            new() { Description = "Food Corner", Category = "Dining Out" },
            new() { Description = "Cafe Mocha", Category = "Dining Out" },

            // Entertainment
            new() { Description = "Movie Tickets AMC", Category = "Entertainment" },
            new() { Description = "Netflix Subscription", Category = "Entertainment" },
            new() { Description = "Cinema Theater", Category = "Entertainment" },
            new() { Description = "Concert Ticket", Category = "Entertainment" },

            // Salary
            new() { Description = "Monthly Payroll Deposit", Category = "Salary" },
            new() { Description = "Company Salary Payment", Category = "Salary" },
            new() { Description = "Stipend Direct Deposit", Category = "Salary" },
            new() { Description = "Income", Category = "Salary" }
        };

        var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

        var pipeline = _mlContext.Transforms.Conversion.MapValueToKey("Label", nameof(TransactionData.Category))
            .Append(_mlContext.Transforms.Text.FeaturizeText("Features", nameof(TransactionData.Description)))
            .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy())
            .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

        _model = pipeline.Fit(dataView);
        _predictionEngine = _mlContext.Model.CreatePredictionEngine<TransactionData, TransactionPrediction>(_model);
    }

    public string PredictCategory(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return "Uncategorized";

        var cleanDesc = description.Trim().ToLowerInvariant();

        // Keyword Fallbacks for fuzzy terms
        if (cleanDesc.Contains("walmart") || cleanDesc.Contains("grocery") || cleanDesc.Contains("groceries") || cleanDesc.Contains("supermarket") || cleanDesc.Contains("market"))
            return "Groceries";
        if (cleanDesc.Contains("electric") || cleanDesc.Contains("water") || cleanDesc.Contains("utility") || cleanDesc.Contains("bill") || cleanDesc.Contains("power") || cleanDesc.Contains("internet"))
            return "Utilities";
        if (cleanDesc.Contains("coffee") || cleanDesc.Contains("starbucks") || cleanDesc.Contains("food") || cleanDesc.Contains("restaurant") || cleanDesc.Contains("cafe") || cleanDesc.Contains("burger"))
            return "Dining Out";
        if (cleanDesc.Contains("movie") || cleanDesc.Contains("netflix") || cleanDesc.Contains("cinema") || cleanDesc.Contains("ticket"))
            return "Entertainment";
        if (cleanDesc.Contains("salary") || cleanDesc.Contains("payroll") || cleanDesc.Contains("income") || cleanDesc.Contains("stipend"))
            return "Salary";

        if (_predictionEngine != null)
        {
            var prediction = _predictionEngine.Predict(new TransactionData { Description = description });
            if (!string.IsNullOrEmpty(prediction.PredictedCategory))
                return prediction.PredictedCategory;
        }

        return "Uncategorized";
    }
}
