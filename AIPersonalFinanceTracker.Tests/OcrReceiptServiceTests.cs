using AIPersonalFinanceTracker.Api.Services;
using AIPersonalFinanceTracker.ML;
using Microsoft.AspNetCore.Http;
using System.Text;

namespace AIPersonalFinanceTracker.Tests;

public class OcrReceiptServiceTests
{
    [Fact]
    public async Task ParseUploadedText_ExtractsMerchantTotalTaxAndDate()
    {
        var content = new StringContent("Corner Market\nTax: $2.00\nTotal: $22.00\n2026-11-08");
        var file = new FormFile(await content.ReadAsStreamAsync(), 0, content.Headers.ContentLength ?? 0, "receipt", "receipt.txt")
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/plain"
        };

        var result = await new OcrReceiptService(new CategorizationService()).ParseUploadedTextAsync(file);

        Assert.Equal("Corner Market", result.Merchant);
        Assert.Equal(22m, result.Amount);
        Assert.Equal(2m, result.TaxAmount);
        Assert.Equal(new DateTime(2026, 11, 8), result.Date.Date);
    }
}