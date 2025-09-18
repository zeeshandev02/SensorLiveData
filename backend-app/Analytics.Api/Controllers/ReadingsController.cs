using Analytics.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Analytics.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReadingsController : ControllerBase
{
    private readonly ISensorDataService _sensorDataService;
    private readonly ILogger<ReadingsController> _logger;

    public ReadingsController(ISensorDataService sensorDataService, ILogger<ReadingsController> logger)
    {
        _sensorDataService = sensorDataService;
        _logger = logger;
    }

    [HttpGet("latest")]
    public async Task<IActionResult> GetLatestReadings([FromQuery] int limit = 1000)
    {
        try
        {
            var readings = await _sensorDataService.GetLatestReadingsAsync(limit);
            return Ok(readings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting latest readings");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("aggregates")]
    public async Task<IActionResult> GetAggregates([FromQuery] int windowSeconds = 60)
    {
        try
        {
            var aggregates = await _sensorDataService.GetAggregatesAsync(windowSeconds);
            return Ok(aggregates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting aggregates");
            return StatusCode(500, "Internal server error");
        }
    }
}
