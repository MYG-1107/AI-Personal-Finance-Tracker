namespace AIPersonalFinanceTracker.Shared.Models;

public class SavingsGoal
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public DateTime? TargetDate { get; set; }
    public string Color { get; set; } = "#0d6efd";
}

public class AdvisorRequest
{
    public string Question { get; set; } = string.Empty;
}

public class AdvisorResponse
{
    public string Answer { get; set; } = string.Empty;
    public List<string> Highlights { get; set; } = new();
}

public class SmartAddRequest
{
    public string Text { get; set; } = string.Empty;
}

public class SmartAddPreview
{
    public string Description { get; set; } = string.Empty;
    public string Merchant { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public int? SuggestedCategoryId { get; set; }
    public string SuggestedCategoryName { get; set; } = "Uncategorized";
    public string? Message { get; set; }
}

public class SubscriptionAnalyticsDto
{
    public decimal MonthlyBurnRate { get; set; }
    public decimal AnnualizedBurnRate { get; set; }
    public List<SubscriptionItemDto> Subscriptions { get; set; } = new();
}

public class SubscriptionItemDto
{
    public string Merchant { get; set; } = string.Empty;
    public decimal AverageCharge { get; set; }
    public decimal EstimatedMonthlyCost { get; set; }
    public int Occurrences { get; set; }
    public DateTime? NextRenewal { get; set; }
    public bool IsExplicitlyRecurring { get; set; }
}

public class ExchangeRatesDto
{
    public string BaseCurrency { get; set; } = "USD";
    public Dictionary<string, decimal> Rates { get; set; } = new();
    public DateTime UpdatedAt { get; set; }
    public bool IsStale { get; set; }
}

public class CurrencyConversionDto
{
    public decimal Amount { get; set; }
    public string FromCurrency { get; set; } = "USD";
    public string ToCurrency { get; set; } = "USD";
    public decimal ConvertedAmount { get; set; }
    public DateTime RateUpdatedAt { get; set; }
}
