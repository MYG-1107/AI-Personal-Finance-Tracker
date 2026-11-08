using AIPersonalFinanceTracker.Api.Data;
using AIPersonalFinanceTracker.Api.Services;
using AIPersonalFinanceTracker.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AIPersonalFinanceTracker.Api.Controllers;

[ApiController]
[Route("api/profile")]
public class ProfileController : ControllerBase
{
    private static readonly HashSet<string> SupportedCurrencies =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "USD", "EUR", "GBP", "INR", "CAD", "AUD", "JPY", "CNY",
            "CHF", "BRL", "ZAR", "AED", "SAR", "SGD", "HKD", "NZD",
            "MXN", "KRW", "THB", "IDR", "MYR", "TRY", "NOK", "SEK",
            "DKK", "PLN", "CZK", "ILS", "PHP", "VND"
        };

    private readonly AppDbContext _context;
    private readonly ExchangeRateService _exchangeRateService;

    public ProfileController(AppDbContext context, ExchangeRateService exchangeRateService)
    {
        _context = context;
        _exchangeRateService = exchangeRateService;
    }

    [HttpGet]
    public async Task<ActionResult<UserProfile>> GetProfile()
    {
        var profile = await _context.UserProfiles.SingleOrDefaultAsync(p => p.Id == 1);
        if (profile == null)
        {
            profile = new UserProfile();
            _context.UserProfiles.Add(profile);
            await _context.SaveChangesAsync();
        }

        return Ok(profile);
    }

    [HttpPut]
    public async Task<ActionResult<UserProfile>> UpdateProfile(UserProfile profile)
    {
        if (profile == null)
            return BadRequest(new { message = "Profile data is required." });

        if (string.IsNullOrWhiteSpace(profile.CountryCode) ||
            profile.CountryCode.Trim().Length != 2)
            return BadRequest(new { message = "Country must be a valid two-letter country code." });

        var currency = profile.CurrencyCode.Trim().ToUpperInvariant();
        if (!SupportedCurrencies.Contains(currency))
            return BadRequest(new { message = "The selected currency is not supported." });

        if (profile.MonthlyBudgetTarget < 0)
            return BadRequest(new { message = "Monthly budget cannot be negative." });
        if (profile.AutoGoalAllocationPercent < 0 || profile.AutoGoalAllocationPercent > 100)
            return BadRequest(new { message = "Goal allocation must be between 0 and 100 percent." });

        var existing = await _context.UserProfiles.SingleOrDefaultAsync(p => p.Id == 1);
        if (existing == null)
        {
            existing = new UserProfile { Id = 1 };
            _context.UserProfiles.Add(existing);
        }

        var previousCurrency = string.IsNullOrWhiteSpace(existing.CurrencyCode)
            ? "USD"
            : existing.CurrencyCode.Trim().ToUpperInvariant();
        var exchangeRate = 1m;
        if (previousCurrency != currency)
        {
            var rates = await _exchangeRateService.GetRatesAsync(previousCurrency);
            if (!rates.Rates.TryGetValue(currency, out var rate))
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    message = $"A live exchange rate from {previousCurrency} to {currency} is currently unavailable. Try again shortly."
                });
            }
            exchangeRate = rate;

            static decimal ConvertAmount(decimal amount, decimal rate) =>
                decimal.Round(amount * rate, 2, MidpointRounding.AwayFromZero);

            foreach (var transaction in await _context.Transactions.ToListAsync())
                transaction.Amount = ConvertAmount(transaction.Amount, exchangeRate);
            foreach (var category in await _context.Categories.ToListAsync())
            {
                category.MonthlyBudgetLimit = ConvertAmount(category.MonthlyBudgetLimit, exchangeRate);
                category.BudgetLimit = ConvertAmount(category.BudgetLimit, exchangeRate);
            }
            foreach (var goal in await _context.SavingsGoals.ToListAsync())
            {
                goal.TargetAmount = ConvertAmount(goal.TargetAmount, exchangeRate);
                goal.CurrentAmount = ConvertAmount(goal.CurrentAmount, exchangeRate);
            }
        }

        existing.DisplayName = profile.DisplayName?.Trim() ?? string.Empty;
        existing.Email = profile.Email?.Trim() ?? string.Empty;
        existing.CountryCode = profile.CountryCode.Trim().ToUpperInvariant();
        existing.CurrencyCode = currency;
        existing.TimeZone = string.IsNullOrWhiteSpace(profile.TimeZone) ? "UTC" : profile.TimeZone.Trim();
        existing.MonthlyBudgetTarget = previousCurrency == currency
            ? profile.MonthlyBudgetTarget
            : decimal.Round(profile.MonthlyBudgetTarget * exchangeRate, 2, MidpointRounding.AwayFromZero);
        existing.AutoGoalAllocationPercent = profile.AutoGoalAllocationPercent;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(existing);
    }
}
