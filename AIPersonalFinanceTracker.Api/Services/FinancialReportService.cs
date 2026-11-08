using AIPersonalFinanceTracker.Shared.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AIPersonalFinanceTracker.Api.Services;

public class FinancialReportService
{
    public byte[] GenerateMonthlyReport(
        IReadOnlyCollection<Transaction> transactions,
        IReadOnlyCollection<SavingsGoal> goals,
        DateTime month)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var monthTransactions = transactions
            .Where(transaction => transaction.Date.Year == month.Year && transaction.Date.Month == month.Month)
            .OrderBy(transaction => transaction.Date)
            .ToList();
        var income = monthTransactions.Where(transaction => transaction.Amount > 0).Sum(transaction => transaction.Amount);
        var expenses = monthTransactions.Where(transaction => transaction.Amount < 0).Sum(transaction => Math.Abs(transaction.Amount));

        return Document.Create(document => document.Page(page =>
        {
            page.Margin(40);
            page.Header().Column(column =>
            {
                column.Item().Text("Personal Finance Report").FontSize(24).Bold().FontColor(Colors.Blue.Darken2);
                column.Item().Text(month.ToString("MMMM yyyy")).FontSize(12).FontColor(Colors.Grey.Darken1);
            });
            page.Content().PaddingTop(20).Column(column =>
            {
                column.Spacing(8);
                column.Item().Text($"Income: {income:N2}");
                column.Item().Text($"Expenses: {expenses:N2}");
                column.Item().Text($"Net balance: {(income - expenses):N2}").Bold();
                column.Item().PaddingTop(12).Text("Savings goals").FontSize(16).Bold();
                foreach (var goal in goals)
                {
                    column.Item().Text($"{goal.Name}: {goal.CurrentAmount:N2} / {goal.TargetAmount:N2}");
                }
                column.Item().PaddingTop(12).Text("Flagged transactions").FontSize(16).Bold();
                var anomalies = monthTransactions.Where(transaction => transaction.IsAnomaly).ToList();
                if (anomalies.Count == 0)
                    column.Item().Text("No flagged transactions.");
                foreach (var transaction in anomalies)
                    column.Item().Text($"{transaction.Date:yyyy-MM-dd} - {transaction.Description}: {transaction.AnomalyReason}");
            });
            page.Footer().AlignCenter().Text(text =>
            {
                text.Span("Generated ");
                text.Span(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm UTC"));
            });
        })).GeneratePdf();
    }
}