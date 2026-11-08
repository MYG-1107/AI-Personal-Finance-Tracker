using System.Security.Cryptography;
using AIPersonalFinanceTracker.Api.Data;
using AIPersonalFinanceTracker.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AIPersonalFinanceTracker.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context) => _context = context;

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Email) || request.Password.Length < 8 ||
            string.IsNullOrWhiteSpace(request.CountryCode) || request.CountryCode.Trim().Length != 2 ||
            string.IsNullOrWhiteSpace(request.CurrencyCode))
            return BadRequest(new { message = "Name, email, country, currency and a password of at least 8 characters are required." });

        var email = request.Email.Trim().ToLowerInvariant();
        if (await _context.UserAccounts.AnyAsync(user => user.Email == email))
            return Conflict(new { message = "An account with this email already exists. Please sign in." });

        var user = new UserAccount
        {
            Name = request.Name.Trim(),
            Email = email,
            PasswordHash = HashPassword(request.Password)
        };
        _context.UserAccounts.Add(user);
        var profile = await _context.UserProfiles.SingleOrDefaultAsync(item => item.Id == 1);
        if (profile != null)
        {
            profile.DisplayName = user.Name;
            profile.Email = user.Email;
            profile.CountryCode = request.CountryCode.Trim().ToUpperInvariant();
            profile.CurrencyCode = request.CurrencyCode.Trim().ToUpperInvariant();
            profile.UpdatedAt = DateTime.UtcNow;
        }
        await _context.SaveChangesAsync();
        return Ok(ToResponse(user));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrEmpty(request.Password))
            return BadRequest(new { message = "Email and password are required." });
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _context.UserAccounts.SingleOrDefaultAsync(item => item.Email == email);
        if (user is null || !VerifyPassword(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "The email or password is incorrect." });
        return Ok(ToResponse(user));
    }

    private static AuthResponse ToResponse(UserAccount user) => new()
    {
        UserId = user.Id,
        Name = user.Name,
        Email = user.Email
    };

    private static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 120_000, HashAlgorithmName.SHA256, 32);
        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    private static bool VerifyPassword(string password, string stored)
    {
        var parts = stored.Split('.');
        if (parts.Length != 2) return false;
        var salt = Convert.FromBase64String(parts[0]);
        var expected = Convert.FromBase64String(parts[1]);
        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, 120_000, HashAlgorithmName.SHA256, expected.Length);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
