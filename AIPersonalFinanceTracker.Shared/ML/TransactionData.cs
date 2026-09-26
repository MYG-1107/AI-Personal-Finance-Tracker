using Microsoft.ML.Data;

namespace AIPersonalFinanceTracker.Shared.ML;

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
