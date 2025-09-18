using Analytics.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Analytics.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlertsController : ControllerBase
{
    private readonly ISensorDataService _sensorDataService;
    private readonly ILogger<AlertsController> _logger;

    public AlertsController(ISensorDataService sensorDataService, ILogger<AlertsController> logger)
    {
        _sensorDataService = sensorDataService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetRecentAlerts([FromQuery] int limit = 100)
    {
        try
        {
            var alerts = await _sensorDataService.GetRecentAlertsAsync(limit);
            return Ok(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent alerts");
            return StatusCode(500, "Internal server error");
        }
    }
}
