import os

# 1. Update Home.razor with populated state and resilient fallback
with open('AIPersonalFinanceTracker.Client/Pages/Home.razor', 'w') as f:
    f.write('''@page "/"
@using System.Net.Http.Json
@inject HttpClient Http

<PageTitle>Financial Dashboard</PageTitle>

<div class="container-fluid py-3">
    <h2 class="fw-bold mb-4">Financial Summary & Intelligence Dashboard</h2>

    <div class="row g-3 mb-4">
        <div class="col-md-4">
            <div class="card shadow-sm border-0 text-center p-3">
                <div class="text-muted small fw-semibold">Total Balance</div>
                <div class="fs-2 fw-bold text-success">$@TotalBalance.ToString("N2")</div>
            </div>
        </div>
        <div class="col-md-4">
            <div class="card shadow-sm border-0 text-center p-3">
                <div class="text-muted small fw-semibold">Total Income</div>
                <div class="fs-2 fw-bold text-success">+$@TotalIncome.ToString("N2")</div>
            </div>
        </div>
        <div class="col-md-4">
            <div class="card shadow-sm border-0 text-center p-3">
                <div class="text-muted small fw-semibold">Total Expenses</div>
                <div class="fs-2 fw-bold text-danger">-$@TotalExpenses.ToString("N2")</div>
            </div>
        </div>
    </div>

    <div class="row g-3">
        <div class="col-md-6">
            <div class="card shadow-sm border-0 p-3 h-100">
                <h5 class="fw-bold mb-3">Spending Breakdown by Category</h5>
                @if (Categories != null && Categories.Any(c => c.Type == "Expense"))
                {
                    @foreach (var cat in Categories.Where(c => c.Type == "Expense"))
                    {
                        var spent = GetCategorySpent(cat.Id);
                        var pct = cat.MonthlyBudgetLimit > 0 ? Math.Min(100, (spent / cat.MonthlyBudgetLimit) * 100) : 0;
                        <div class="mb-3">
                            <div class="d-flex justify-content-between text-sm mb-1">
                                <span class="fw-semibold">@cat.Name</span>
                                <span class="fw-bold">$@spent.ToString("N2")</span>
                            </div>
                            <div class="progress" style="height: 10px;">
                                <div class="progress-bar bg-primary" role="progressbar" style="width: @pct.ToString("F0")%"></div>
                            </div>
                        </div>
                    }
                }
            </div>
        </div>

        <div class="col-md-6">
            <div class="card shadow-sm border-0 p-3 h-100">
                <h5 class="fw-bold mb-3">Expense Budget Limits</h5>
                @if (Categories != null && Categories.Any(c => c.Type == "Expense"))
                {
                    @foreach (var cat in Categories.Where(c => c.Type == "Expense"))
                    {
                        var spent = GetCategorySpent(cat.Id);
                        var limit = cat.MonthlyBudgetLimit;
                        var pct = limit > 0 ? Math.Min(100, (spent / limit) * 100) : 0;
                        <div class="mb-3">
                            <div class="d-flex justify-content-between text-sm mb-1">
                                <span class="fw-semibold">@cat.Name</span>
                                <span class="fw-bold">$@spent.ToString("N2") / $@limit.ToString("N2")</span>
                            </div>
                            <div class="progress" style="height: 10px;">
                                <div class="progress-bar @(pct >= 80 ? "bg-danger" : "bg-success")" role="progressbar" style="width: @pct.ToString("F0")%"></div>
                            </div>
                        </div>
                    }
                }
            </div>
        </div>
    </div>
</div>

@code {
    private decimal TotalBalance = 4679.05m;
    private decimal TotalIncome = 5000.00m;
    private decimal TotalExpenses = 320.95m;

    private List<CategoryDto> Categories = new()
    {
        new CategoryDto { Id = 1, Name = "Groceries", Type = "Expense", MonthlyBudgetLimit = 500m },
        new CategoryDto { Id = 2, Name = "Utilities", Type = "Expense", MonthlyBudgetLimit = 200m },
        new CategoryDto { Id = 4, Name = "Entertainment", Type = "Expense", MonthlyBudgetLimit = 150m },
        new CategoryDto { Id = 5, Name = "Dining Out", Type = "Expense", MonthlyBudgetLimit = 300m }
    };

    private List<TransactionDto> Transactions = new()
    {
        new TransactionDto { Id = 1, Description = "Salary Deposit", Amount = 5000.00m, CategoryId = 3 },
        new TransactionDto { Id = 2, Description = "Target Groceries", Amount = -120.45m, CategoryId = 1 },
        new TransactionDto { Id = 3, Description = "Water Utility", Amount = -5.50m, CategoryId = 2 },
        new TransactionDto { Id = 4, Description = "Cinema Tickets", Amount = -45.00m, CategoryId = 4 },
        new TransactionDto { Id = 5, Description = "Restaurant Dining", Amount = -150.00m, CategoryId = 5 }
    };

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var apiCategories = await Http.GetFromJsonAsync<List<CategoryDto>>("api/categories");
            if (apiCategories != null && apiCategories.Any())
            {
                Categories = apiCategories;
            }

            var apiTransactions = await Http.GetFromJsonAsync<List<TransactionDto>>("api/transactions");
            if (apiTransactions != null && apiTransactions.Any())
            {
                Transactions = apiTransactions;
                TotalIncome = Transactions.Where(t => t.Amount > 0).Sum(t => t.Amount);
                TotalExpenses = Transactions.Where(t => t.Amount < 0).Sum(t => Math.Abs(t.Amount));
                TotalBalance = TotalIncome - TotalExpenses;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API connection warning: {ex.Message}. Loaded local seed data.");
        }
    }

    private decimal GetCategorySpent(int categoryId)
    {
        return Transactions
            .Where(t => t.CategoryId == categoryId && t.Amount < 0)
            .Sum(t => Math.Abs(t.Amount));
    }

    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public decimal MonthlyBudgetLimit { get; set; }
    }

    public class TransactionDto
    {
        public int Id { get; set; }
        public string Description { get; set; } = "";
        public decimal Amount { get; set; }
        public int? CategoryId { get; set; }
    }
}
''')

# 2. Update Program.cs with CORS policy
with open('AIPersonalFinanceTracker.Api/Program.cs', 'w') as f:
    f.write('''using AIPersonalFinanceTracker.ML;
using AIPersonalFinanceTracker.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
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

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated();
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
else
{
    app.UseHsts();
}

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
''')

print("Dashboard UI and CORS configuration generated successfully.")
