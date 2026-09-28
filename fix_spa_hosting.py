import os

program_path = 'AIPersonalFinanceTracker.Api/Program.cs'
program_code = """using Microsoft.EntityFrameworkCore;
using AIPersonalFinanceTracker.Api.Data;
using AIPersonalFinanceTracker.Api.Services;
using AIPersonalFinanceTracker.ML;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=finance.db"));

builder.Services.AddScoped<CategorizationService>();
builder.Services.AddScoped<AnomalyDetectionService>();
builder.Services.AddScoped<FinancialHealthEngine>();
builder.Services.AddScoped<ForecastService>();
builder.Services.AddScoped<OcrReceiptService>();

builder.Services.AddControllers().AddJsonOptions(options => {
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", policy => {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseCors("AllowAll");

// Serve Blazor WASM client static files
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapControllers();
// Route all non-API requests to Blazor index.html
app.MapFallbackToFile("index.html");

app.Run();
"""

with open(program_path, 'w', encoding='utf-8') as f:
    f.write(program_code)

print("Updated Program.cs with Blazor WASM static file middleware and fallback routing.")
