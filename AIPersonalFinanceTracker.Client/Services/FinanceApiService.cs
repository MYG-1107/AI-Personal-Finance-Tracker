using System.Net.Http.Json;
using AIPersonalFinanceTracker.Shared.Models;

namespace AIPersonalFinanceTracker.Client.Services;

public class FinanceApiService
{
    private readonly HttpClient _http;

    public FinanceApiService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<Transaction>> GetTransactionsAsync()
    {
        return await _http.GetFromJsonAsync<List<Transaction>>("api/transactions") ?? new();
    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        return await _http.GetFromJsonAsync<List<Category>>("api/categories") ?? new();
    }

    public async Task<Transaction?> CreateTransactionAsync(Transaction transaction)
    {
        var response = await _http.PostAsJsonAsync("api/transactions", transaction);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<Transaction>();
        }
        return null;
    }

    public async Task<bool> UpdateTransactionCategoryAsync(int transactionId, int categoryId)
    {
        var response = await _http.PutAsync($"api/transactions/{transactionId}/category/{categoryId}", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<CashFlowForecastDto?> GetCashFlowForecastAsync()
    {
        return await _http.GetFromJsonAsync<CashFlowForecastDto>("api/forecast/cashflow");
    }

    public async Task<UserProfile?> GetProfileAsync()
    {
        return await _http.GetFromJsonAsync<UserProfile>("api/profile");
    }

    public async Task<UserProfile?> UpdateProfileAsync(UserProfile profile)
    {
        var response = await _http.PutAsJsonAsync("api/profile", profile);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<UserProfile>();

        var message = await response.Content.ReadAsStringAsync();
        throw new InvalidOperationException(string.IsNullOrWhiteSpace(message)
            ? $"Profile update failed ({(int)response.StatusCode})."
            : message);
    }

    public async Task<List<SavingsGoal>> GetGoalsAsync() =>
        await _http.GetFromJsonAsync<List<SavingsGoal>>("api/goals") ?? new();

    public async Task<SavingsGoal?> CreateGoalAsync(SavingsGoal goal)
    {
        var response = await _http.PostAsJsonAsync("api/goals", goal);
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<SavingsGoal>() : null;
    }

    public async Task<AdvisorResponse?> AskAdvisorAsync(string question)
    {
        var response = await _http.PostAsJsonAsync("api/advisor/ask", new AdvisorRequest { Question = question });
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<AdvisorResponse>() : null;
    }
}
