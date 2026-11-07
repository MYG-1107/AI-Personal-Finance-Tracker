namespace AIPersonalFinanceTracker.Shared.Models;

public class UserProfile
{
    public int Id { get; set; } = 1;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string CountryCode { get; set; } = "US";
    public string CurrencyCode { get; set; } = "USD";
    public string TimeZone { get; set; } = "UTC";
    public decimal MonthlyBudgetTarget { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
