using Microsoft.ML;
using Microsoft.ML.Data;

namespace AIPersonalFinanceTracker.ML;

public class TransactionData
{
    [LoadColumn(0)]
    public string Description { get; set; } = string.Empty;

    [LoadColumn(1)]
    public string Category { get; set; } = string.Empty;
}

public class CategoryPrediction
{
    [ColumnName("PredictedLabel")]
    public string PredictedCategory { get; set; } = string.Empty;

    public float[] Score { get; set; } = Array.Empty<float>();
}

public class CategorizationService
{
    private static readonly object _fileLock = new();
    private readonly MLContext _mlContext;
    private ITransformer? _model;
    private PredictionEngine<TransactionData, CategoryPrediction>? _predictionEngine;
    private readonly List<TransactionData> _trainingData;
    private readonly string _modelPath;

    public CategorizationService(string? customModelPath = null)
    {
        _mlContext = new MLContext(seed: 0);
        _modelPath = customModelPath ?? Path.Combine(AppContext.BaseDirectory, "model.zip");

        _trainingData = new List<TransactionData>
        {
            new() { Description = "Starbucks Coffee", Category = "Dining Out" },
            new() { Description = "Chipotle Mexican Grill", Category = "Dining Out" },
            new() { Description = "Olive Garden Restaurant", Category = "Dining Out" },
            new() { Description = "Walmart Grocery Store", Category = "Groceries" },
            new() { Description = "Whole Foods Market", Category = "Groceries" },
            new() { Description = "Trader Joes", Category = "Groceries" },
            new() { Description = "Target Supercenter", Category = "Groceries" },
            new() { Description = "Electric Utility Bill", Category = "Utilities" },
            new() { Description = "Water Power Utility", Category = "Utilities" },
            new() { Description = "Mobile Phone Bill", Category = "Utilities" },
            new() { Description = "Netflix Subscription", Category = "Entertainment" },
            new() { Description = "Cinema Movie Tickets", Category = "Entertainment" },
            new() { Description = "Spotify Music", Category = "Entertainment" },
            new() { Description = "Monthly Salary Direct Deposit", Category = "Salary" },
            new() { Description = "Payroll Deposit", Category = "Salary" },
            new() { Description = "Freelance Consulting Payout", Category = "Freelance" }
        };

        InitializeModel();
    }

    private void InitializeModel()
    {
        lock (_fileLock)
        {
            if (File.Exists(_modelPath))
            {
                try
                {
                    DataViewSchema modelSchema;
                    _model = _mlContext.Model.Load(_modelPath, out modelSchema);
                    _predictionEngine = _mlContext.Model.CreatePredictionEngine<TransactionData, CategoryPrediction>(_model);
                    return;
                }
                catch
                {
                    // Fallback to training if file access or schema fails
                }
            }

            TrainAndSaveModelInternal();
        }
    }

    private void TrainAndSaveModelInternal()
    {
        var dataView = _mlContext.Data.LoadFromEnumerable(_trainingData);

        var pipeline = _mlContext.Transforms.Conversion.MapValueToKey("Label", nameof(TransactionData.Category))
            .Append(_mlContext.Transforms.Text.FeaturizeText("Features", nameof(TransactionData.Description)))
            .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy("Label", "Features"))
            .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

        _model = pipeline.Fit(dataView);
        _predictionEngine = _mlContext.Model.CreatePredictionEngine<TransactionData, CategoryPrediction>(_model);

        try
        {
            _mlContext.Model.Save(_model, dataView.Schema, _modelPath);
        }
        catch (IOException)
        {
            // Silently handle race conditions during concurrent test teardowns
        }
    }

    public string PredictCategory(string description)
    {
        lock (_fileLock)
        {
            if (_predictionEngine == null || string.IsNullOrWhiteSpace(description))
                return "Uncategorized";

            var prediction = _predictionEngine.Predict(new TransactionData { Description = description });
            return string.IsNullOrEmpty(prediction.PredictedCategory) ? "Uncategorized" : prediction.PredictedCategory;
        }
    }

    public void LearnFromOverride(string description, string newCategory)
    {
        if (string.IsNullOrWhiteSpace(description) || string.IsNullOrWhiteSpace(newCategory)) return;

        lock (_fileLock)
        {
            _trainingData.Add(new TransactionData { Description = description, Category = newCategory });
            TrainAndSaveModelInternal();
        }
    }
}
