using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AIPersonalFinanceTracker.Api.Data;
using AIPersonalFinanceTracker.Shared.Models;
using AIPersonalFinanceTracker.ML;
using System.Globalization;
using System.Text.RegularExpressions;
using System.IO.Compression;
using System.Xml.Linq;

namespace AIPersonalFinanceTracker.Api.Controllers;

public class TransactionCreateDto
{
    public string Description { get; set; } = "";
    public decimal Amount { get; set; }
    public int? CategoryId { get; set; }
    public DateTime? Date { get; set; }
    public bool IsRecurring { get; set; }
    public int? RecurrenceDay { get; set; }
}

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly CategoryPredictorService _categoryPredictor;
    private readonly AnomalyDetectionService _anomalyDetector;

    public TransactionsController(
        AppDbContext context,
        CategoryPredictorService categoryPredictor,
        AnomalyDetectionService anomalyDetector)
    {
        _context = context;
        _categoryPredictor = categoryPredictor ?? new CategoryPredictorService();
        _anomalyDetector = anomalyDetector ?? new AnomalyDetectionService();
    }

    private readonly AppDbContext _context;

    [HttpGet]
    public async Task<ActionResult<List<Transaction>>> GetTransactions()
    {
        return Ok(await _context.Transactions
            .AsNoTracking()
            .OrderByDescending(t => t.Date)
            .ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> AddTransaction([FromBody] TransactionCreateDto dto)
    {
        try
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Description))
                return BadRequest(new { message = "A transaction description is required." });

            if (dto.Amount == 0)
                return BadRequest(new { message = "A transaction amount must be non-zero." });

            var desc = dto.Description.Trim();
            var amt = dto.Amount;
            var catId = dto.CategoryId;

            var transaction = new Transaction
            {
                Description = desc,
                Amount = amt,
                Date = dto.Date ?? DateTime.UtcNow,
                CategoryId = catId,
                IsRecurring = dto.IsRecurring,
                RecurrenceDay = dto.RecurrenceDay,
                NextOccurrence = dto.IsRecurring
                    ? (dto.Date ?? DateTime.UtcNow).AddMonths(1)
                    : null
            };

            if (amt < 0 && (!transaction.CategoryId.HasValue || transaction.CategoryId == 0))
            {
                var predId = _categoryPredictor.PredictCategory(transaction.Description);
                transaction.CategoryId = predId > 0 ? predId : null;
                transaction.IsAutoCategorized = predId > 0;
            }

            var transactionHistory = await _context.Transactions
                .AsNoTracking()
                .Where(item => item.Date >= transaction.Date.AddDays(-90))
                .ToListAsync();
            var anomaly = _anomalyDetector.DetectAnomaly(transaction, transactionHistory);
            transaction.IsAnomaly = anomaly.IsAnomaly;
            transaction.AnomalyReason = anomaly.Reason;

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            if (transaction.Amount > 0)
            {
                var allocationPercent = await _context.UserProfiles
                    .Where(p => p.Id == 1)
                    .Select(p => p.AutoGoalAllocationPercent)
                    .SingleOrDefaultAsync();
                var activeGoals = await _context.SavingsGoals
                    .Where(g => g.CurrentAmount < g.TargetAmount)
                    .ToListAsync();
                var allocation = Math.Min(transaction.Amount * allocationPercent / 100m,
                    activeGoals.Sum(g => Math.Max(0, g.TargetAmount - g.CurrentAmount)));
                if (allocation > 0 && activeGoals.Count > 0)
                {
                    var remaining = allocation;
                    foreach (var goal in activeGoals)
                    {
                        var contribution = Math.Min(
                            Math.Round(allocation / activeGoals.Count, 2),
                            goal.TargetAmount - goal.CurrentAmount);
                        contribution = Math.Min(contribution, remaining);
                        goal.CurrentAmount += contribution;
                        remaining -= contribution;
                    }

                    if (remaining > 0)
                    {
                        var first = activeGoals.First();
                        first.CurrentAmount = Math.Min(first.TargetAmount, first.CurrentAmount + remaining);
                    }

                    await _context.SaveChangesAsync();
                }
            }

            return Ok(transaction);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPost("smart-add")]
    public ActionResult<SmartAddPreview> SmartAdd(SmartAddRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Text))
            return BadRequest(new { message = "Describe the transaction first." });

        var text = request.Text.Trim();
        var amountMatch = Regex.Match(text, @"(?<amount>\d+(?:[.,]\d{1,2})?)");
        if (!amountMatch.Success ||
            !decimal.TryParse(amountMatch.Groups["amount"].Value.Replace(',', '.'),
                NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) ||
            amount <= 0)
            return BadRequest(new { message = "Include a positive amount, for example 'spent 450 INR on groceries'." });

        var currencyMatch = Regex.Match(text, @"\b(USD|INR|EUR|GBP|NOK|SEK|CAD|AUD|JPY|CHF|AED|BRL)\b", RegexOptions.IgnoreCase);
        var currency = currencyMatch.Success ? currencyMatch.Value.ToUpperInvariant() : "USD";
        var dateMatch = Regex.Match(text, @"\b(20\d{2}[-/]\d{1,2}[-/]\d{1,2})\b");
        var date = dateMatch.Success && DateTime.TryParse(dateMatch.Value, out var parsedDate)
            ? parsedDate.ToUniversalTime()
            : text.Contains("yesterday", StringComparison.OrdinalIgnoreCase)
                ? DateTime.UtcNow.Date.AddDays(-1)
                : DateTime.UtcNow;
        var merchantMatch = Regex.Match(text, @"\b(?:at|from)\s+(?<merchant>[A-Za-z][A-Za-z0-9 &'-]{1,40})", RegexOptions.IgnoreCase);
        var merchant = merchantMatch.Success
            ? merchantMatch.Groups["merchant"].Value.Trim(' ', '.', ',', ':')
            : string.Empty;
        var description = Regex.Replace(text, @"\b(spent|paid|bought|purchase|on|for|today|yesterday|at|from|via)\b", "", RegexOptions.IgnoreCase)
            .Trim(' ', ',', '.', ':');
        if (dateMatch.Success)
            description = description.Replace(dateMatch.Value, string.Empty, StringComparison.OrdinalIgnoreCase).Trim(' ', ',', '.', ':');
        var categoryId = _categoryPredictor.PredictCategory(description);

        return Ok(new SmartAddPreview
        {
            Description = description,
            Merchant = merchant,
            Amount = -amount,
            CurrencyCode = currency,
            Date = date,
            SuggestedCategoryId = categoryId > 0 ? categoryId : null,
            SuggestedCategoryName = categoryId switch
            {
                1 => "Groceries & Food",
                2 => "Utilities & Bills",
                3 => "Salary & Income",
                4 => "Entertainment",
                5 => "Transport & Travel",
                _ => "Uncategorized"
            },
            Message = "Review the parsed fields before saving."
        });
    }

    [HttpPost("import")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<List<Transaction>>> Import(IFormFile? file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "Choose a CSV or XLSX transaction history file." });
        if (file.Length > 20 * 1024 * 1024)
            return BadRequest(new { message = "Import files must be 20 MB or smaller." });

        try
        {
            var rows = Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase)
                ? await ReadXlsxAsync(file)
                : await ReadCsvAsync(file);
            if (rows.Count == 0)
                return BadRequest(new { message = "No transaction rows were found. Include Date, Description and Amount columns." });

            var imported = new List<Transaction>();
            foreach (var row in rows)
            {
                if (string.IsNullOrWhiteSpace(row.Description) || row.Amount == 0)
                    continue;
                var transaction = new Transaction
                {
                    Description = row.Description.Trim(),
                    Amount = row.Amount,
                    Date = row.Date ?? DateTime.UtcNow,
                    CategoryId = _categoryPredictor.PredictCategory(row.Description) is var category && category > 0 ? category : null,
                    IsAutoCategorized = true
                };
                var anomaly = _anomalyDetector.DetectAnomaly(transaction, imported);
                transaction.IsAnomaly = anomaly.IsAnomaly;
                transaction.AnomalyReason = anomaly.Reason;
                imported.Add(transaction);
            }

            _context.Transactions.AddRange(imported);
            await _context.SaveChangesAsync();
            return Ok(imported);
        }
        catch (FormatException ex)
        {
            return BadRequest(new { message = $"The import file has an invalid value: {ex.Message}" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = $"Unable to import transaction history: {ex.Message}" });
        }
    }

    private sealed record ImportedRow(string Description, decimal Amount, DateTime? Date);

    private static async Task<List<ImportedRow>> ReadCsvAsync(IFormFile file)
    {
        using var reader = new StreamReader(file.OpenReadStream());
        var rows = new List<ImportedRow>();
        var header = await reader.ReadLineAsync();
        if (header is null) return rows;
        var columns = header.Split(',').Select((name, index) => (Name: name.Trim().ToLowerInvariant(), Index: index)).ToList();
        var descriptionIndex = columns.FirstOrDefault(c => c.Name is "description" or "merchant" or "name").Index;
        var amountIndex = columns.FirstOrDefault(c => c.Name is "amount" or "value" or "total").Index;
        var dateIndex = columns.FirstOrDefault(c => c.Name is "date" or "transaction date").Index;
        while (await reader.ReadLineAsync() is { } line)
        {
            var values = line.Split(',');
            if (descriptionIndex >= values.Length || amountIndex >= values.Length) continue;
            var amount = decimal.Parse(values[amountIndex].Trim().Replace("$", "").Replace("₹", ""), CultureInfo.InvariantCulture);
            var typeIndex = columns.FirstOrDefault(c => c.Name is "type" or "transaction type").Index;
            if (amount > 0 && typeIndex < values.Length &&
                values[typeIndex].Contains("expense", StringComparison.OrdinalIgnoreCase))
                amount = -amount;
            DateTime? date = dateIndex < values.Length && DateTime.TryParse(values[dateIndex], out var parsed) ? parsed : null;
            rows.Add(new ImportedRow(values[descriptionIndex], amount, date));
        }
        return rows;
    }

    private static async Task<List<ImportedRow>> ReadXlsxAsync(IFormFile file)
    {
        await using var input = file.OpenReadStream();
        using var archive = new ZipArchive(input, ZipArchiveMode.Read);
        var shared = new List<string>();
        var sharedEntry = archive.GetEntry("xl/sharedStrings.xml");
        if (sharedEntry is not null)
        {
            using var sharedStream = sharedEntry.Open();
            var sharedXml = XDocument.Load(sharedStream);
            shared.AddRange(sharedXml.Descendants().Where(e => e.Name.LocalName == "si")
                .Select(si => string.Concat(si.Descendants().Where(t => t.Name.LocalName == "t").Select(t => t.Value))));
        }
        var sheet = archive.GetEntry("xl/worksheets/sheet1.xml");
        if (sheet is null) return new();
        using var sheetStream = sheet.Open();
        var xml = XDocument.Load(sheetStream);
        var rows = xml.Descendants().Where(e => e.Name.LocalName == "row")
            .Select(row => row.Elements().Where(e => e.Name.LocalName == "c").Select(cell =>
            {
                var value = cell.Elements().FirstOrDefault(e => e.Name.LocalName == "v")?.Value ?? "";
                return cell.Attribute("t")?.Value == "s" && int.TryParse(value, out var index) && index < shared.Count ? shared[index] : value;
            }).ToList()).ToList();
        if (rows.Count < 2) return new();
        var headers = rows[0].Select((value, index) => (Name: value.Trim().ToLowerInvariant(), Index: index)).ToList();
        var descriptionIndex = headers.FirstOrDefault(c => c.Name is "description" or "merchant" or "name").Index;
        var amountIndex = headers.FirstOrDefault(c => c.Name is "amount" or "value" or "total").Index;
        var dateIndex = headers.FirstOrDefault(c => c.Name is "date" or "transaction date").Index;
        return rows.Skip(1).Where(row => descriptionIndex < row.Count && amountIndex < row.Count)
            .Select(row => new ImportedRow(row[descriptionIndex],
                decimal.Parse(row[amountIndex].Replace("$", "").Replace("₹", ""), CultureInfo.InvariantCulture),
                dateIndex < row.Count && DateTime.TryParse(row[dateIndex], out var date) ? date : null)).ToList();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteTransaction(int id)
    {
        var transaction = await _context.Transactions.FindAsync(id);
        if (transaction == null)
            return NotFound(new { message = "Transaction not found." });

        _context.Transactions.Remove(transaction);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("{id:int}/category/{categoryId:int}")]
    public async Task<IActionResult> UpdateCategory(int id, int categoryId)
    {
        var transaction = await _context.Transactions.FindAsync(id);
        if (transaction == null)
            return NotFound(new { message = "Transaction not found." });

        if (!await _context.Categories.AnyAsync(c => c.Id == categoryId))
            return BadRequest(new { message = "Category not found." });

        transaction.CategoryId = categoryId;
        transaction.IsAutoCategorized = false;
        transaction.IsAnomaly = false;
        transaction.AnomalyReason = null;
        await _context.SaveChangesAsync();
        return Ok(transaction);
    }

    [NonAction]
    public async Task<IActionResult> AddTransaction(Transaction transaction)
    {
        if (transaction == null) return BadRequest("Transaction required.");
        var dto = new TransactionCreateDto
        {
            Description = transaction.Description,
            Amount = transaction.Amount,
            CategoryId = transaction.CategoryId,
            Date = transaction.Date
        };
        return await AddTransaction(dto);
    }

    [NonAction]
    public async Task<IActionResult> PostTransaction(Transaction transaction)
    {
        return await AddTransaction(transaction);
    }
}
