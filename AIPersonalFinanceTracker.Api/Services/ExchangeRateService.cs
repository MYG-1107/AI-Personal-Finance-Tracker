using System.Text.Json;
using AIPersonalFinanceTracker.Shared.Models;

namespace AIPersonalFinanceTracker.Api.Services;

public class ExchangeRateService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private ExchangeRatesDto? _cachedRates;

    public ExchangeRateService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<ExchangeRatesDto> GetRatesAsync(string baseCurrency = "USD", bool forceRefresh = false)
    {
        baseCurrency = NormalizeCurrency(baseCurrency);
        if (!forceRefresh && _cachedRates is not null &&
            _cachedRates.BaseCurrency == baseCurrency &&
            _cachedRates.UpdatedAt >= DateTime.UtcNow.AddHours(-24))
            return _cachedRates;

        await _refreshLock.WaitAsync();
        try
        {
            if (!forceRefresh && _cachedRates is not null &&
                _cachedRates.BaseCurrency == baseCurrency &&
                _cachedRates.UpdatedAt >= DateTime.UtcNow.AddHours(-24))
                return _cachedRates;

            try
            {
                var client = _httpClientFactory.CreateClient("exchange-rates");
                var payload = await client.GetFromJsonAsync<FrankfurterResponse>(
                    $"latest?from={baseCurrency}");
                if (payload is not null)
                {
                    _cachedRates = new ExchangeRatesDto
                    {
                        BaseCurrency = baseCurrency,
                        Rates = payload.Rates,
                        UpdatedAt = DateTime.UtcNow,
                        IsStale = false
                    };
                }
            }
            catch (HttpRequestException)
            {
                if (_cachedRates is not null)
                    _cachedRates.IsStale = true;
            }

            return _cachedRates ?? new ExchangeRatesDto
            {
                BaseCurrency = baseCurrency,
                Rates = new Dictionary<string, decimal> { [baseCurrency] = 1m },
                UpdatedAt = DateTime.UtcNow,
                IsStale = true
            };
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    public async Task<CurrencyConversionDto?> ConvertAsync(
        decimal amount,
        string fromCurrency,
        string toCurrency)
    {
        fromCurrency = NormalizeCurrency(fromCurrency);
        toCurrency = NormalizeCurrency(toCurrency);
        var rates = await GetRatesAsync(fromCurrency);
        if (!rates.Rates.TryGetValue(toCurrency, out var rate))
            return null;

        return new CurrencyConversionDto
        {
            Amount = amount,
            FromCurrency = fromCurrency,
            ToCurrency = toCurrency,
            ConvertedAmount = decimal.Round(amount * rate, 2),
            RateUpdatedAt = rates.UpdatedAt
        };
    }

    private static string NormalizeCurrency(string currency) =>
        string.IsNullOrWhiteSpace(currency) ? "USD" : currency.Trim().ToUpperInvariant();

    private sealed class FrankfurterResponse
    {
        public Dictionary<string, decimal> Rates { get; set; } = new();
    }
}

public class ExchangeRateRefreshService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ExchangeRateRefreshService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<ExchangeRateService>();
            await service.GetRatesAsync("USD");
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }
}