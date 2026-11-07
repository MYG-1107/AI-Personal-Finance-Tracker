using AIPersonalFinanceTracker.Api.Data;
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
            "USD", "EUR", "GBP", "INR", "JPY", "CAD", "AUD",
            "NOK", "SEK", "CHF", "BRL", "AED"
        };

    private readonly AppDbContext _context;

    public ProfileController(AppDbContext context)
    {
        _context = context;
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

        existing.DisplayName = profile.DisplayName?.Trim() ?? string.Empty;
        existing.Email = profile.Email?.Trim() ?? string.Empty;
        existing.CountryCode = profile.CountryCode.Trim().ToUpperInvariant();
        existing.CurrencyCode = currency;
        existing.TimeZone = string.IsNullOrWhiteSpace(profile.TimeZone) ? "UTC" : profile.TimeZone.Trim();
        existing.MonthlyBudgetTarget = profile.MonthlyBudgetTarget;
        existing.AutoGoalAllocationPercent = profile.AutoGoalAllocationPercent;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(existing);
    }
}
