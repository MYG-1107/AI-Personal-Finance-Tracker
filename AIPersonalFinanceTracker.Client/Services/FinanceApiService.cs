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

    public async Task<List<Category>> GetCategoriesAsync()
    {
        return await _http.GetFromJsonAsync<List<Category>>("/api/categories") ?? new List<Category>();
    }

    public async Task<List<Transaction>> GetTransactionsAsync()
    {
        return await _http.GetFromJsonAsync<List<Transaction>>("/api/transactions") ?? new List<Transaction>();
    }

    public async Task<bool> CreateTransactionAsync(Transaction transaction)
    {
        var response = await _http.PostAsJsonAsync("/api/transactions", transaction);
        return response.IsSuccessStatusCode;
    }

    public async Task<string> PredictCategoryAsync(string description)
    {
        var response = await _http.PostAsJsonAsync("/api/transactions/predict-category", description);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<PredictionResult>();
            return result?.Category ?? "Uncategorized";
        }
        return "Uncategorized";
    }

    private class PredictionResult
    {
        public string Category { get; set; } = string.Empty;
    }
}
