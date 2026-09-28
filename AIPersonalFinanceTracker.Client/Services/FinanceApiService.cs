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
}
