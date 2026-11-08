using AIPersonalFinanceTracker.Api.Services;
using AIPersonalFinanceTracker.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace AIPersonalFinanceTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExchangeRatesController : ControllerBase
{
    private readonly ExchangeRateService _service;

    public ExchangeRatesController(ExchangeRateService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ExchangeRatesDto>> GetRates([FromQuery] string baseCurrency = "USD")
    {
        return Ok(await _service.GetRatesAsync(baseCurrency));
    }

    [HttpGet("convert")]
    public async Task<ActionResult<CurrencyConversionDto>> Convert(
        [FromQuery] decimal amount,
        [FromQuery] string fromCurrency = "USD",
        [FromQuery] string toCurrency = "EUR")
    {
        var result = await _service.ConvertAsync(amount, fromCurrency, toCurrency);
        return result is null ? NotFound(new { message = "Currency rate is unavailable." }) : Ok(result);
    }
}