using AIPersonalFinanceTracker.Api.Controllers;
using AIPersonalFinanceTracker.ML;
using Microsoft.EntityFrameworkCore;
using AIPersonalFinanceTracker.Api.Data;
using AIPersonalFinanceTracker.Shared.Models;

namespace AIPersonalFinanceTracker.Tests;

public class SmartAddTests
{
    [Fact]
    public void SmartAdd_ExtractsMerchantCurrencyAndDate()
    {
        var context = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var controller = new TransactionsController(context, new CategoryPredictorService(), new AnomalyDetectionService());

        var result = controller.SmartAdd(new SmartAddRequest
        {
            Text = "Paid 1200 INR for electricity at Power Utility yesterday"
        });

        var response = Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(result.Result);
        var preview = Assert.IsType<SmartAddPreview>(response.Value);
        Assert.Equal("INR", preview.CurrencyCode);
        Assert.Equal(-1200m, preview.Amount);
        Assert.Contains("Power Utility", preview.Merchant);
        Assert.Equal(DateTime.UtcNow.Date.AddDays(-1), preview.Date.Date);
    }
}