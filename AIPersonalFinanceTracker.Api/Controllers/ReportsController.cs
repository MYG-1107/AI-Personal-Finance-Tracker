using AIPersonalFinanceTracker.Api.Data;
using AIPersonalFinanceTracker.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AIPersonalFinanceTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly FinancialReportService _reports;

    public ReportsController(AppDbContext context, FinancialReportService reports)
    {
        _context = context;
        _reports = reports;
    }

    [HttpGet("monthly")]
    public async Task<IActionResult> Monthly([FromQuery] int? year, [FromQuery] int? month)
    {
        var reportMonth = new DateTime(year ?? DateTime.UtcNow.Year, month ?? DateTime.UtcNow.Month, 1);
        var transactions = await _context.Transactions.AsNoTracking().ToListAsync();
        var goals = await _context.SavingsGoals.AsNoTracking().ToListAsync();
        var bytes = _reports.GenerateMonthlyReport(transactions, goals, reportMonth);
        return File(bytes, "application/pdf", $"finance-report-{reportMonth:yyyy-MM}.pdf");
    }
}