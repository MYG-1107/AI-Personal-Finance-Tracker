using Microsoft.AspNetCore.Mvc;
using AIPersonalFinanceTracker.Api.Services;
using AIPersonalFinanceTracker.Shared.Models;

namespace AIPersonalFinanceTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OcrController : ControllerBase
{
    private readonly OcrReceiptService _ocrService;

    public OcrController(OcrReceiptService ocrService)
    {
        _ocrService = ocrService;
    }

    [HttpPost("scan")]
    public ActionResult<ReceiptScanResultDto> ScanReceipt([FromBody] string? rawText)
    {
        var result = _ocrService.ParseReceiptText(rawText ?? "Target Superstore Purchase Total: $84.50");
        return Ok(result);
    }
}
