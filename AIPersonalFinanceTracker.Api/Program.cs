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
builder.Services.AddScoped<SubscriptionAnalyticsService>();
builder.Services.AddScoped<FinancialAdvisorService>();
builder.Services.AddScoped<FinancialReportService>();
builder.Services.AddHttpClient("exchange-rates", client =>
{
    client.BaseAddress = new Uri("https://api.frankfurter.app/");
    client.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddSingleton<ExchangeRateService>();
builder.Services.AddHostedService<ExchangeRateRefreshService>();

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
                AutoGoalAllocationPercent TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL
            );
            INSERT OR IGNORE INTO UserProfiles
                (Id, DisplayName, Email, CountryCode, CurrencyCode, TimeZone, MonthlyBudgetTarget, AutoGoalAllocationPercent, UpdatedAt)
            VALUES
                (1, '', '', 'US', 'USD', 'UTC', '0', '0', '2026-01-01T00:00:00Z');
            CREATE TABLE IF NOT EXISTS SavingsGoals (
                Id INTEGER NOT NULL CONSTRAINT PK_SavingsGoals PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                TargetAmount TEXT NOT NULL,
                CurrentAmount TEXT NOT NULL,
                TargetDate TEXT NULL,
                Color TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS BudgetWorkspaces (
                Id INTEGER NOT NULL CONSTRAINT PK_BudgetWorkspaces PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                OwnerEmail TEXT NOT NULL,
                CreatedAt TEXT NOT NULL
            );
            CREATE TABLE IF NOT EXISTS BudgetMembers (
                Id INTEGER NOT NULL CONSTRAINT PK_BudgetMembers PRIMARY KEY AUTOINCREMENT,
                BudgetWorkspaceId INTEGER NOT NULL,
                Email TEXT NOT NULL,
                Role TEXT NOT NULL,
                JoinedAt TEXT NOT NULL,
                FOREIGN KEY (BudgetWorkspaceId) REFERENCES BudgetWorkspaces(Id) ON DELETE CASCADE
            );
            """);
        EnsureTransactionColumn(context, "IsRecurring", "INTEGER NOT NULL DEFAULT 0");
        EnsureTransactionColumn(context, "RecurrenceDay", "INTEGER NULL");
        EnsureTransactionColumn(context, "NextOccurrence", "TEXT NULL");
        EnsureTransactionColumn(context, "AnomalyStatus", "TEXT NULL");
        EnsureProfileColumn(context, "AutoGoalAllocationPercent", "TEXT NOT NULL DEFAULT '0'");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"DB Init Error: {ex.Message}");
    }
}

static void EnsureTransactionColumn(AppDbContext context, string columnName, string definition)
    {
        using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = "PRAGMA table_info(Transactions)";
        context.Database.OpenConnection();
        var exists = false;
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                if (string.Equals(reader.GetString(1), columnName, StringComparison.OrdinalIgnoreCase))
                {
                    exists = true;
                    break;
                }
            }
        }

        if (!exists)
            context.Database.ExecuteSqlRaw($"ALTER TABLE Transactions ADD COLUMN {columnName} {definition}");

        context.Database.CloseConnection();
    }

static void EnsureProfileColumn(AppDbContext context, string columnName, string definition)
    {
        using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = "PRAGMA table_info(UserProfiles)";
        context.Database.OpenConnection();
        var exists = false;
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                if (string.Equals(reader.GetString(1), columnName, StringComparison.OrdinalIgnoreCase))
                {
                    exists = true;
                    break;
                }
            }
        }

        if (!exists)
            context.Database.ExecuteSqlRaw($"ALTER TABLE UserProfiles ADD COLUMN {columnName} {definition}");

        context.Database.CloseConnection();
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
