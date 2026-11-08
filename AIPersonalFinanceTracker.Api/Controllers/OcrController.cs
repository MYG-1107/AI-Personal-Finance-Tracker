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

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ReceiptScanResultDto>> UploadReceipt(IFormFile? file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "Upload a non-empty receipt file." });
        if (file.Length > 10 * 1024 * 1024)
            return BadRequest(new { message = "Receipt files must be 10 MB or smaller." });

        var textTypes = new[] { "text/plain", "text/csv", "application/json" };
        if (!textTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
            return StatusCode(StatusCodes.Status501NotImplemented, new
            {
                message = "Image and PDF OCR requires an OCR provider. Configure OCR_ENDPOINT and OCR_API_KEY before uploading binary receipts."
            });

        return Ok(await _ocrService.ParseUploadedTextAsync(file));
    }
}
