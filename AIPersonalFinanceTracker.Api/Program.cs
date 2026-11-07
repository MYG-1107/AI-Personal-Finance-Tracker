using AIPersonalFinanceTracker.ML;
using AIPersonalFinanceTracker.Api.Data;
using AIPersonalFinanceTracker.Api.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

builder.Services.AddRazorPages();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=finance.db"));

builder.Services.AddScoped<CategoryPredictorService>();
builder.Services.AddScoped<AnomalyDetectionService>();
builder.Services.AddScoped<ForecastService>();
builder.Services.AddScoped<OcrReceiptService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated();
        context.Database.ExecuteSqlRaw("""
            CREATE TABLE IF NOT EXISTS UserProfiles (
                Id INTEGER NOT NULL CONSTRAINT PK_UserProfiles PRIMARY KEY,
                DisplayName TEXT NOT NULL,
                Email TEXT NOT NULL,
                CountryCode TEXT NOT NULL,
                CurrencyCode TEXT NOT NULL,
                TimeZone TEXT NOT NULL,
                MonthlyBudgetTarget TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL
            );
            INSERT OR IGNORE INTO UserProfiles
                (Id, DisplayName, Email, CountryCode, CurrencyCode, TimeZone, MonthlyBudgetTarget, UpdatedAt)
            VALUES
                (1, '', '', 'US', 'USD', 'UTC', '0', '2026-01-01T00:00:00Z');
            CREATE TABLE IF NOT EXISTS SavingsGoals (
                Id INTEGER NOT NULL CONSTRAINT PK_SavingsGoals PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                TargetAmount TEXT NOT NULL,
                CurrentAmount TEXT NOT NULL,
                TargetDate TEXT NULL,
                Color TEXT NOT NULL
            );
            """);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"DB Init Error: {ex.Message}");
    }
}

app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
