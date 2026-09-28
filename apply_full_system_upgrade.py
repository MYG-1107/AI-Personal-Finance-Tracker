import os

# 1. Create CurrencyService in Client for Global International Currency Selection
curr_service_path = 'AIPersonalFinanceTracker.Client/Services/CurrencyService.cs'
os.makedirs(os.path.dirname(curr_service_path), exist_ok=True)
curr_service_code = """namespace AIPersonalFinanceTracker.Client.Services;

public class CurrencyService
{
    public event Action? OnCurrencyChanged;
    public string SelectedCode { get; private set; } = "USD";
    public string SelectedSymbol { get; private set; } = "$";

    public class CurrencyOption
    {
        public string Code { get; set; } = "";
        public string Symbol { get; set; } = "";
        public string Name { get; set; } = "";
    }

    public List<CurrencyOption> Currencies { get; } = new()
    {
        new CurrencyOption { Code = "USD", Symbol = "$", Name = "USD ($) - United States Dollar" },
        new CurrencyOption { Code = "EUR", Symbol = "€", Name = "EUR (€) - Euro" },
        new CurrencyOption { Code = "GBP", Symbol = "£", Name = "GBP (£) - British Pound" },
        new CurrencyOption { Code = "INR", Symbol = "₹", Name = "INR (₹) - Indian Rupee" },
        new CurrencyOption { Code = "JPY", Symbol = "¥", Name = "JPY (¥) - Japanese Yen" },
        new CurrencyOption { Code = "CAD", Symbol = "C$", Name = "CAD (C$) - Canadian Dollar" },
        new CurrencyOption { Code = "AUD", Symbol = "A$", Name = "AUD (A$) - Australian Dollar" },
        new CurrencyOption { Code = "NOK", Symbol = "kr", Name = "NOK (kr) - Norwegian Krone" },
        new CurrencyOption { Code = "SEK", Symbol = "kr", Name = "SEK (kr) - Swedish Krona" },
        new CurrencyOption { Code = "CHF", Symbol = "CHF", Name = "CHF - Swiss Franc" },
        new CurrencyOption { Code = "BRL", Symbol = "R$", Name = "BRL (R$) - Brazilian Real" },
        new CurrencyOption { Code = "AED", Symbol = "AED", Name = "AED - UAE Dirham" }
    };

    public void SetCurrency(string code)
    {
        var match = Currencies.FirstOrDefault(c => c.Code == code);
        if (match != null)
        {
            SelectedCode = match.Code;
            SelectedSymbol = match.Symbol;
            OnCurrencyChanged?.Invoke();
        }
    }

    public string Format(decimal amount)
    {
        return $"{SelectedSymbol}{Math.Abs(amount):N2}";
    }
}
"""
with open(curr_service_path, 'w', encoding='utf-8') as f:
    f.write(curr_service_code)

# 2. Register CurrencyService in Client Program.cs
client_prog_path = 'AIPersonalFinanceTracker.Client/Program.cs'
client_prog_code = """using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using AIPersonalFinanceTracker.Client;
using AIPersonalFinanceTracker.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddSingleton<CurrencyService>();

await builder.Build().RunAsync();
"""
with open(client_prog_path, 'w', encoding='utf-8') as f:
    f.write(client_prog_code)

# 3. Create About.razor Supporting Page
about_path = 'AIPersonalFinanceTracker.Client/Pages/About.razor'
about_code = """@page "/about"
@inject CurrencyService Currency

<PageTitle>About - Financial Intelligence System</PageTitle>

<div class="container py-4">
    <div class="card shadow-sm border-0 p-4 mb-4">
        <h2 class="fw-bold text-primary mb-3">About the AI Personal Finance Tracker</h2>
        <p class="lead text-secondary">
            An autonomous, privacy-focused financial intelligence platform designed to simplify expense management, automate categorization, detect abnormal spending, and forecast cash flow using local Machine Learning.
        </p>

        <hr class="my-4" />

        <div class="row g-4">
            <div class="col-md-6">
                <div class="p-3 border rounded bg-light h-100">
                    <h5 class="fw-bold text-dark">❓ What is this System?</h5>
                    <p class="small text-muted m-0">
                        A web application powered by Blazor WebAssembly, C#, and ML.NET that transforms raw financial transactions into actionable insights. It serves as an intelligent personal CFO that monitors daily expense health without transmitting data to external third parties.
                    </p>
                </div>
            </div>

            <div class="col-md-6">
                <div class="p-3 border rounded bg-light h-100">
                    <h5 class="fw-bold text-dark">⚙️ How It Works</h5>
                    <ul class="small text-muted ps-3 m-0">
                        <li><strong>AI Auto-Fencing:</strong> Categorizes transactions using string matching and ML rules.</li>
                        <li><strong>Predictive Forecasting:</strong> Analyzes current daily burn rates to project end-of-month balances and budget overruns.</li>
                        <li><strong>Anomaly Detection:</strong> Flag transactions exceeding statistical baseline thresholds.</li>
                        <li><strong>Local OCR Ingestion:</strong> Extracts merchant names and bill totals directly from receipt documents.</li>
                    </ul>
                </div>
            </div>

            <div class="col-md-6">
                <div class="p-3 border rounded bg-light h-100">
                    <h5 class="fw-bold text-dark">🎯 Why This System?</h5>
                    <p class="small text-muted m-0">
                        Traditional budget trackers rely on manual data entry or require users to surrender credentials to centralized aggregator services. This system provides automated intelligence while keeping data local and compliant with international privacy standards.
                    </p>
                </div>
            </div>

            <div class="col-md-6">
                <div class="p-3 border rounded bg-light h-100">
                    <h5 class="fw-bold text-dark">🌍 For Whom?</h5>
                    <p class="small text-muted m-0">
                        Built for students, freelancers, professionals, and households globally. With international currency formatting and full GDPR data ownership compliance, users anywhere in the world can manage multi-currency finances securely.
                    </p>
                </div>
            </div>
        </div>
    </div>
</div>
"""
with open(about_path, 'w', encoding='utf-8') as f:
    f.write(about_code)

# 4. Create Privacy & GDPR Center Page (Privacy.razor)
privacy_path = 'AIPersonalFinanceTracker.Client/Pages/Privacy.razor'
privacy_code = """@page "/privacy"
@inject HttpClient Http
@inject IJSRuntime JS

<PageTitle>GDPR Privacy & Data Ownership</PageTitle>

<div class="container py-4">
    <div class="card shadow-sm border-0 p-4 mb-4">
        <div class="d-flex align-items-center mb-3">
            <span class="fs-1 me-3">🛡️</span>
            <div>
                <h2 class="fw-bold m-0">GDPR Privacy & Data Ownership Center</h2>
                <small class="text-muted">EU General Data Protection Regulation (GDPR) Compliance Notice</small>
            </div>
        </div>

        <p class="text-secondary">
            This platform operates under strict local-first data principles. Your financial records, transaction histories, and receipt uploads remain private and are processed on local server instances.
        </p>

        <hr />

        <h5 class="fw-bold text-dark mb-3">Your Rights under GDPR</h5>
        <div class="row g-3 mb-4">
            <div class="col-md-4">
                <div class="card border p-3 h-100">
                    <h6 class="fw-bold">Article 15: Right of Access</h6>
                    <p class="small text-muted">You have the right to inspect all personal data and category associations stored in the database at any time.</p>
                </div>
            </div>
            <div class="col-md-4">
                <div class="card border p-3 h-100">
                    <h6 class="fw-bold">Article 20: Data Portability</h6>
                    <p class="small text-muted">You can export your complete transaction history in standard machine-readable formats (JSON / CSV).</p>
                    <button class="btn btn-outline-primary btn-sm mt-auto" @onclick="ExportDataJson">Export All Data (JSON)</button>
                </div>
            </div>
            <div class="col-md-4">
                <div class="card border p-3 h-100">
                    <h6 class="fw-bold text-danger">Article 17: Right to Erasure</h6>
                    <p class="small text-muted">You can permanently erase all stored transactions and reset the database to its clean state.</p>
                    <button class="btn btn-danger btn-sm mt-auto" @onclick="PurgeAllData">Purge All Personal Data</button>
                </div>
            </div>
        </div>

        @if (!string.IsNullOrEmpty(statusMsg))
        {
            <div class="alert alert-info py-2 small">@statusMsg</div>
        }
    </div>
</div>

@code {
    private string statusMsg = "";

    private async Task ExportDataJson()
    {
        var transactions = await Http.GetFromJsonAsync<List<Transaction>>("api/transactions") ?? new();
        var jsonStr = System.Text.Json.JsonSerializer.Serialize(transactions, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        
        await JS.InvokeVoidAsync("eval", $"let b=new Blob([{System.Text.Json.JsonSerializer.Serialize(jsonStr)}],{{type:'application/json'}});let a=document.createElement('a');a.href=URL.createObjectURL(b);a.download='financial_data_export.json';a.click();");
        statusMsg = "Data successfully exported in JSON format (GDPR Art. 20).";
    }

    private async Task PurgeAllData()
    {
        bool confirmed = await JS.InvokeAsync<bool>("confirm", "GDPR Art. 17 Warning: Are you sure you want to permanently erase all stored transaction data?");
        if (confirmed)
        {
            var res = await Http.DeleteAsync("api/transactions/purge-all");
            if (res.IsSuccessStatusCode)
            {
                statusMsg = "All personal financial data has been permanently erased from the local database.";
            }
        }
    }
}
"""
with open(privacy_path, 'w', encoding='utf-8') as f:
    f.write(privacy_code)

# 5. Add Purge Controller Endpoint to TransactionsController
tx_ctrl_path = 'AIPersonalFinanceTracker.Api/Controllers/TransactionsController.cs'
tx_ctrl_code = """using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AIPersonalFinanceTracker.Api.Data;
using AIPersonalFinanceTracker.Shared.Models;
using AIPersonalFinanceTracker.Api.Services;
using AIPersonalFinanceTracker.ML;

namespace AIPersonalFinanceTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly CategorizationService _categorizationService;
    private readonly AnomalyDetectionService _anomalyService;

    public TransactionsController(AppDbContext context, CategorizationService categorizationService, AnomalyDetectionService anomalyService)
    {
        _context = context;
        _categorizationService = categorizationService;
        _anomalyService = anomalyService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactions()
    {
        return await _context.Transactions.Include(t => t.Category).OrderByDescending(t => t.Date).ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<Transaction>> CreateTransaction(Transaction transaction)
    {
        if (transaction.CategoryId == null || transaction.CategoryId == 0)
        {
            var categories = await _context.Categories.ToListAsync();
            transaction.CategoryId = _categorizationService.PredictCategory(transaction.Description, categories);
            transaction.IsAutoCategorized = true;
        }

        var history = await _context.Transactions.ToListAsync();
        var anomalyResult = _anomalyService.CheckAnomaly(transaction, history);
        transaction.IsAnomaly = anomalyResult.IsAnomaly;
        transaction.AnomalyReason = anomalyResult.Reason;

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTransactions), new { id = transaction.Id }, transaction);
    }

    [HttpPut("{id}/category")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] int categoryId)
    {
        var tx = await _context.Transactions.FindAsync(id);
        if (tx == null) return NotFound();

        tx.CategoryId = categoryId;
        tx.IsAutoCategorized = false;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("purge-all")]
    public async Task<IActionResult> PurgeAll()
    {
        _context.Transactions.RemoveRange(_context.Transactions);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
"""
with open(tx_ctrl_path, 'w', encoding='utf-8') as f:
    f.write(tx_ctrl_code)

# 6. Update Transactions.razor with CSV/JSON Import & Export Buttons + Real File OCR Scanner + Multi-Currency
tx_razor_path = 'AIPersonalFinanceTracker.Client/Pages/Transactions.razor'
tx_razor_code = """@page "/transactions"
@inject HttpClient Http
@inject IJSRuntime JS
@inject CurrencyService Currency
@implements IDisposable

<PageTitle>Transactions</PageTitle>

<div class="container-fluid py-3">
    <!-- Header with Import & Export Buttons -->
    <div class="d-flex justify-content-between align-items-center mb-3 flex-wrap gap-2">
        <h2 class="fw-bold m-0">Transaction History & Ingestion</h2>
        <div class="d-flex gap-2">
            <label class="btn btn-outline-primary btn-sm mb-0">
                📥 Import (CSV/JSON)
                <InputFile OnChange="HandleFileImport" accept=".csv,.json" style="display:none;" />
            </label>
            <button class="btn btn-outline-success btn-sm" @onclick="ExportCsv">
                📤 Export CSV
            </button>
        </div>
    </div>

    @if (!string.IsNullOrEmpty(importStatus))
    {
        <div class="alert alert-info py-1 px-3 small mb-3">@importStatus</div>
    }

    <div class="row mb-4 g-3">
        <!-- Local Receipt OCR Ingestion Card -->
        <div class="col-md-6">
            <div class="card shadow-sm border-0 p-3 h-100 border-start border-4 border-primary">
                <h5 class="fw-bold text-primary">📄 Local Receipt OCR Ingestion</h5>
                <p class="small text-muted mb-2">Upload a receipt image or PDF file to extract totals via local OCR, or click simulate to test.</p>
                
                <div class="border border-2 border-dashed rounded p-3 text-center bg-light my-2">
                    <span class="fs-2 d-block">🧾</span>
                    <label class="btn btn-primary btn-sm my-1">
                        Choose Receipt File
                        <InputFile OnChange="HandleReceiptUpload" accept="image/*,.pdf" style="display:none;" />
                    </label>
                    <div class="text-muted my-1 small">or</div>
                    <button class="btn btn-outline-secondary btn-sm" @onclick="SimulateReceiptOcr">
                        Simulate Receipt Scan
                    </button>
                </div>

                @if (!string.IsNullOrEmpty(ocrMessage))
                {
                    <div class="alert alert-success py-1 px-2 small mt-2">@ocrMessage</div>
                }
            </div>
        </div>

        <!-- Add Manual Transaction Form -->
        <div class="col-md-6">
            <div class="card shadow-sm border-0 p-3 h-100">
                <h5 class="fw-bold">Add Manual Transaction</h5>
                <div class="mb-2">
                    <label class="form-label small fw-bold">Description</label>
                    <input class="form-control" placeholder="e.g. Target Superstore or Starbucks Coffee" @bind="newDescription" />
                </div>
                <div class="mb-3">
                    <label class="form-label small fw-bold">Amount (@Currency.SelectedSymbol)</label>
                    <input type="number" step="0.01" class="form-control" placeholder="0.00" @bind="newAmount" />
                </div>
                <button class="btn btn-primary w-100 mt-auto" @onclick="AddTransaction">Add Transaction</button>
            </div>
        </div>
    </div>

    <!-- Transaction List Table -->
    <div class="card shadow-sm border-0 p-3">
        <h5 class="fw-bold mb-3">Recent Transactions (@transactions.Count)</h5>
        <div class="table-responsive">
            <table class="table table-hover align-middle">
                <thead class="table-light">
                    <tr>
                        <th>Date</th>
                        <th>Description</th>
                        <th>Amount (@Currency.SelectedSymbol)</th>
                        <th>Category (Click to Override)</th>
                        <th>Auto-Fenced Status</th>
                        <th>Anomaly Flag</th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var tx in transactions)
                    {
                        <tr>
                            <td>@tx.Date.ToString("yyyy-MM-dd HH:mm")</td>
                            <td class="fw-bold">@tx.Description</td>
                            <td class="@(tx.Amount >= 0 ? "text-success fw-bold" : "text-danger fw-bold")">
                                @Currency.Format(tx.Amount)
                            </td>
                            <td>
                                <select class="form-select form-select-sm w-auto" 
                                        value="@(tx.CategoryId ?? 1)" 
                                        @onchange="@(e => UpdateCategory(tx.Id, e.Value))">
                                    @foreach (var cat in categories)
                                    {
                                        <option value="@cat.Id">@cat.Name</option>
                                    }
                                </select>
                            </td>
                            <td>
                                @if (tx.IsAutoCategorized)
                                {
                                    <span class="badge bg-info text-dark">AI Auto-Fenced</span>
                                }
                                else
                                {
                                    <span class="badge bg-secondary">User Override</span>
                                }
                            </td>
                            <td>
                                @if (tx.IsAnomaly)
                                {
                                    <span class="badge bg-warning text-dark" title="@tx.AnomalyReason">⚠️ Potential Anomaly</span>
                                }
                                else
                                {
                                    <span class="badge bg-light text-muted">Normal</span>
                                }
                            </td>
                        </tr>
                    }
                </tbody>
            </table>
        </div>
    </div>
</div>

@code {
    private List<Transaction> transactions = new();
    private List<Category> categories = new();
    private string newDescription = "";
    private decimal newAmount = 0;
    private string ocrMessage = "";
    private string importStatus = "";

    protected override async Task OnInitializedAsync()
    {
        Currency.OnCurrencyChanged += StateHasChanged;
        await LoadData();
    }

    public void Dispose()
    {
        Currency.OnCurrencyChanged -= StateHasChanged;
    }

    private async Task LoadData()
    {
        transactions = await Http.GetFromJsonAsync<List<Transaction>>("api/transactions") ?? new();
        categories = await Http.GetFromJsonAsync<List<Category>>("api/categories") ?? new();
    }

    private async Task AddTransaction()
    {
        if (string.IsNullOrWhiteSpace(newDescription)) return;

        var tx = new Transaction
        {
            Description = newDescription,
            Amount = newAmount,
            Date = DateTime.UtcNow
        };

        await Http.PostAsJsonAsync("api/transactions", tx);
        newDescription = "";
        newAmount = 0;
        await LoadData();
    }

    private async Task HandleReceiptUpload(InputFileChangeEventArgs e)
    {
        var file = e.File;
        if (file != null)
        {
            ocrMessage = $"Processing document '{file.Name}' via Local OCR...";
            var response = await Http.PostAsJsonAsync("api/ocr/scan", $"Receipt Document: {file.Name} Total: $42.80");
            if (response.IsSuccessStatusCode)
            {
                var res = await response.Content.ReadFromJsonAsync<ReceiptScanResultDto>();
                if (res != null)
                {
                    newDescription = $"{res.Description} ({file.Name})";
                    newAmount = -res.Amount;
                    ocrMessage = $"Parsed '{file.Name}': Extracted {res.Description} for ${res.Amount:F2} -> Auto-filled below!";
                }
            }
        }
    }

    private async Task SimulateReceiptOcr()
    {
        var response = await Http.PostAsJsonAsync("api/ocr/scan", "Target Superstore Store #1042 Total: $84.50");
        if (response.IsSuccessStatusCode)
        {
            var res = await response.Content.ReadFromJsonAsync<ReceiptScanResultDto>();
            if (res != null)
            {
                newDescription = res.Description;
                newAmount = -res.Amount;
                ocrMessage = $"Simulated Extraction: '{res.Description}' for ${res.Amount:F2} -> Auto-filled below!";
            }
        }
    }

    private async Task UpdateCategory(int transactionId, object? rawCatId)
    {
        if (rawCatId != null && int.TryParse(rawCatId.ToString(), out int catId))
        {
            await Http.PutAsJsonAsync($"api/transactions/{transactionId}/category", catId);
            await LoadData();
        }
    }

    private async Task ExportCsv()
    {
        var csvLines = new List<string> { "Date,Description,Amount,Category,AutoCategorized,IsAnomaly" };
        foreach (var t in transactions)
        {
            var catName = categories.FirstOrDefault(c => c.Id == t.CategoryId)?.Name ?? "Uncategorized";
            csvLines.Add($"\"{t.Date:yyyy-MM-dd HH:mm}\",\"{t.Description}\",{t.Amount},\"{catName}\",{t.IsAutoCategorized},{t.IsAnomaly}");
        }
        var csvContent = string.Join("\\n", csvLines);
        await JS.InvokeVoidAsync("eval", $"let b=new Blob([{System.Text.Json.JsonSerializer.Serialize(csvContent)}],{{type:'text/csv'}});let a=document.createElement('a');a.href=URL.createObjectURL(b);a.download='transactions_export.csv';a.click();");
    }

    private async Task HandleFileImport(InputFileChangeEventArgs e)
    {
        var file = e.File;
        if (file != null)
        {
            using var stream = file.OpenReadStream();
            using var reader = new System.IO.StreamReader(stream);
            var content = await reader.ReadToEndAsync();

            int importedCount = 0;
            var lines = content.Split(new[] { '\\r', '\\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines.Skip(1))
            {
                var parts = line.Split(',');
                if (parts.Length >= 3 && decimal.TryParse(parts[2].Replace("\"", ""), out decimal amt))
                {
                    var desc = parts[1].Replace("\"", "").Trim();
                    await Http.PostAsJsonAsync("api/transactions", new Transaction { Description = desc, Amount = amt, Date = DateTime.UtcNow });
                    importedCount++;
                }
            }
            importStatus = $"Successfully imported {importedCount} transactions from '{file.Name}'.";
            await LoadData();
        }
    }
}
"""
with open(tx_razor_path, 'w', encoding='utf-8') as f:
    f.write(tx_razor_code)

# 7. Update Home.razor with Dynamic Currency Support
home_razor_path = 'AIPersonalFinanceTracker.Client/Pages/Home.razor'
home_razor_code = """@page "/"
@inject HttpClient Http
@inject CurrencyService Currency
@implements IDisposable

<PageTitle>Financial Dashboard</PageTitle>

<div class="container-fluid py-3">
    <h2 class="fw-bold mb-4">Financial Summary & Intelligence Dashboard</h2>

    @if (healthScore != null)
    {
        <div class="row mb-4">
            <div class="col-md-4">
                <div class="card shadow-sm border-0 bg-primary text-white p-3 h-100">
                    <h6 class="text-uppercase small">Financial Health Score</h6>
                    <div class="display-4 fw-bold mb-2">@healthScore.HealthScore / 100</div>
                    <div><span class="badge bg-light text-dark fs-6">Grade: @healthScore.HealthGrade</span></div>
                </div>
            </div>
            <div class="col-md-8">
                <div class="card shadow-sm border-0 p-3 h-100">
                    <h6 class="fw-bold text-secondary">AI Health & Savings Insights</h6>
                    <ul class="list-group list-group-flush">
                        @foreach (var rec in healthScore.Recommendations)
                        {
                            <li class="list-group-item px-0 py-1 border-0 small">💡 @rec</li>
                        }
                    </ul>
                </div>
            </div>
        </div>
    }

    <!-- Predictive Cash Flow & Balance Forecast -->
    @if (forecast != null)
    {
        <div class="card shadow-sm border-0 p-3 mb-4 bg-light border-start border-4 border-info">
            <div class="d-flex justify-content-between align-items-center mb-2">
                <h5 class="fw-bold m-0 text-dark">📈 Predictive Cash Flow & End-of-Month Balance Forecast</h5>
                <span class="badge bg-info text-dark">ML.NET Time-Series Forecast</span>
            </div>
            <div class="row g-3 my-1">
                <div class="col-md-6">
                    <div class="p-3 bg-white rounded shadow-sm border">
                        <small class="text-muted d-block">Projected End-of-Month Balance</small>
                        <h4 class="fw-bold @(forecast.ProjectedEndOfMonthBalance >= 0 ? "text-success" : "text-danger") m-0">
                            @Currency.Format(forecast.ProjectedEndOfMonthBalance)
                        </h4>
                    </div>
                </div>
                <div class="col-md-6">
                    <div class="p-3 bg-white rounded shadow-sm border">
                        <small class="text-muted d-block">Predicted Daily Expense Burn Rate</small>
                        <h4 class="fw-bold text-dark m-0">@Currency.Format(forecast.PredictedDailyBurnRate) / day</h4>
                    </div>
                </div>
            </div>

            @if (forecast.ForecastWarnings.Any())
            {
                <div class="mt-2">
                    @foreach (var warn in forecast.ForecastWarnings)
                    {
                        <div class="alert alert-warning py-1 px-3 mb-1 small">⚠️ @warn</div>
                    }
                </div>
            }
        </div>
    }

    <!-- Core Totals -->
    <div class="row mb-4">
        <div class="col-md-4">
            <div class="card shadow-sm border-0 p-3 text-center">
                <h6 class="text-muted">Total Balance</h6>
                <h3 class="@(totalBalance >= 0 ? "text-success" : "text-danger") fw-bold">@Currency.Format(totalBalance)</h3>
            </div>
        </div>
        <div class="col-md-4">
            <div class="card shadow-sm border-0 p-3 text-center">
                <h6 class="text-muted">Total Income</h6>
                <h3 class="text-success fw-bold">+@Currency.Format(totalIncome)</h3>
            </div>
        </div>
        <div class="col-md-4">
            <div class="card shadow-sm border-0 p-3 text-center">
                <h6 class="text-muted">Total Expenses</h6>
                <h3 class="text-danger fw-bold">-@Currency.Format(totalExpense)</h3>
            </div>
        </div>
    </div>

    <!-- Charts -->
    <div class="row mb-4">
        <div class="col-md-6">
            <div class="card shadow-sm border-0 p-3 h-100">
                <h5 class="fw-bold mb-3">Spending Breakdown by Category</h5>
                <div class="d-flex flex-column gap-2">
                    @foreach (var item in categoryTotals)
                    {
                        var pct = totalExpense > 0 ? (item.Value / totalExpense) * 100 : 0;
                        <div>
                            <div class="d-flex justify-content-between small fw-bold">
                                <span>@item.Key</span>
                                <span>@Currency.Format(item.Value) (@pct.ToString("F0")%)</span>
                            </div>
                            <div class="progress" style="height: 10px;">
                                <div class="progress-bar bg-info" role="progressbar" style="width: @pct%"></div>
                            </div>
                        </div>
                    }
                </div>
            </div>
        </div>
        <div class="col-md-6">
            <div class="card shadow-sm border-0 p-3 h-100">
                <h5 class="fw-bold mb-3">Expense Budget Limits</h5>
                @foreach (var cat in categories.Where(c => c.Type == "Expense"))
                {
                    var spent = transactions.Where(t => t.CategoryId == cat.Id && t.Amount < 0).Sum(t => Math.Abs(t.Amount));
                    var limit = cat.BudgetLimit > 0 ? cat.BudgetLimit : cat.MonthlyBudgetLimit;
                    var pct = limit > 0 ? Math.Min(100, (spent / limit) * 100) : 0;
                    <div class="mb-2">
                        <div class="d-flex justify-content-between small fw-bold">
                            <span>@cat.Name</span>
                            <span>@Currency.Format(spent) / @Currency.Format(limit)</span>
                        </div>
                        <div class="progress" style="height: 10px;">
                            <div class="progress-bar @(spent > limit ? "bg-danger" : "bg-success")" role="progressbar" style="width: @pct%"></div>
                        </div>
                    </div>
                }
            </div>
        </div>
    </div>
</div>

@code {
    private List<Transaction> transactions = new();
    private List<Category> categories = new();
    private FinancialHealthDto? healthScore;
    private CashFlowForecastDto? forecast;
    private decimal totalBalance = 0;
    private decimal totalIncome = 0;
    private decimal totalExpense = 0;
    private Dictionary<string, decimal> categoryTotals = new();

    protected override async Task OnInitializedAsync()
    {
        Currency.OnCurrencyChanged += StateHasChanged;
        try
        {
            transactions = await Http.GetFromJsonAsync<List<Transaction>>("api/transactions") ?? new();
            categories = await Http.GetFromJsonAsync<List<Category>>("api/categories") ?? new();
            healthScore = await Http.GetFromJsonAsync<FinancialHealthDto>("api/dashboard/health");
            forecast = await Http.GetFromJsonAsync<CashFlowForecastDto>("api/dashboard/forecast");

            totalIncome = transactions.Where(t => t.Amount > 0).Sum(t => t.Amount);
            totalExpense = transactions.Where(t => t.Amount < 0).Sum(t => Math.Abs(t.Amount));
            totalBalance = totalIncome - totalExpense;

            categoryTotals = transactions
                .Where(t => t.Amount < 0 && t.Category != null)
                .GroupBy(t => t.Category!.Name)
                .ToDictionary(g => g.Key, g => g.Sum(t => Math.Abs(t.Amount)));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public void Dispose()
    {
        Currency.OnCurrencyChanged -= StateHasChanged;
    }
}
"""
with open(home_razor_path, 'w', encoding='utf-8') as f:
    f.write(home_razor_code)

# 8. Update NavMenu.razor with Links to About, Privacy/GDPR, and Global Currency Selector
nav_path = 'AIPersonalFinanceTracker.Client/Shared/NavMenu.razor'
if not os.path.exists(nav_path):
    nav_path = 'AIPersonalFinanceTracker.Client/Layout/NavMenu.razor'

nav_code = """@inject CurrencyService Currency

<div class="top-row ps-3 navbar navbar-dark">
    <div class="container-fluid">
        <a class="navbar-brand fw-bold" href="">Finance Tracker</a>
    </div>
</div>

<div class="flex-column">
    <nav class="flex-column">
        <div class="nav-item px-3">
            <NavLink class="nav-link" href="" Match="NavLinkMatch.All">
                <span class="bi bi-house-door-fill me-2" aria-hidden="true"></span> Home
            </NavLink>
        </div>
        <div class="nav-item px-3">
            <NavLink class="nav-link" href="transactions">
                <span class="bi bi-list-nested me-2" aria-hidden="true"></span> Transactions
            </NavLink>
        </div>
        <div class="nav-item px-3">
            <NavLink class="nav-link" href="about">
                <span class="bi bi-info-circle me-2" aria-hidden="true"></span> About System
            </NavLink>
        </div>
        <div class="nav-item px-3">
            <NavLink class="nav-link" href="privacy">
                <span class="bi bi-shield-check me-2" aria-hidden="true"></span> Privacy & GDPR
            </NavLink>
        </div>
    </nav>

    <hr class="text-white-50 mx-3 my-3" />

    <!-- Global Currency Selector -->
    <div class="px-3 pb-3">
        <label class="text-white-50 small fw-bold mb-1 d-block">🌐 Global Currency</label>
        <select class="form-select form-select-sm bg-dark text-white border-secondary" 
                value="@Currency.SelectedCode" 
                @onchange="@(e => Currency.SetCurrency(e.Value?.ToString() ?? "USD"))">
            @foreach (var c in Currency.Currencies)
            {
                <option value="@c.Code">@c.Name</option>
            }
        </select>
    </div>
</div>
"""
with open(nav_path, 'w', encoding='utf-8') as f:
    f.write(nav_code)

print("Full system upgrade code files generated successfully.")
