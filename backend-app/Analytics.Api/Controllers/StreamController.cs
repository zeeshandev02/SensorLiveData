using Analytics.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace Analytics.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StreamController : ControllerBase
{
    private readonly ISensorDataService _sensorDataService;
    private readonly ILogger<StreamController> _logger;

    public StreamController(ISensorDataService sensorDataService, ILogger<StreamController> logger)
    {
        _sensorDataService = sensorDataService;
        _logger = logger;
    }

    [HttpGet("readings")]
    public async Task GetReadingsStream(CancellationToken cancellationToken)
    {
        Response.Headers.Add("Content-Type", "text/event-stream");
        Response.Headers.Add("Cache-Control", "no-cache");
        Response.Headers.Add("Connection", "keep-alive");
        Response.Headers.Add("Access-Control-Allow-Origin", "*");
        Response.Headers.Add("Access-Control-Allow-Headers", "Cache-Control");

        try
        {
            await foreach (var reading in _sensorDataService.StreamReadingsAsync(cancellationToken))
            {
                var json = JsonSerializer.Serialize(reading);
                var data = $"event: reading\ndata: {json}\n\n";
                var bytes = Encoding.UTF8.GetBytes(data);
                
                await Response.Body.WriteAsync(bytes, cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Readings stream connection closed by client");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in readings stream");
        }
    }

    [HttpGet("alerts")]
    public async Task GetAlertsStream(CancellationToken cancellationToken)
    {
        Response.Headers.Add("Content-Type", "text/event-stream");
        Response.Headers.Add("Cache-Control", "no-cache");
        Response.Headers.Add("Connection", "keep-alive");
        Response.Headers.Add("Access-Control-Allow-Origin", "*");
        Response.Headers.Add("Access-Control-Allow-Headers", "Cache-Control");

        try
        {
            await foreach (var alert in _sensorDataService.StreamAlertsAsync(cancellationToken))
            {
                var json = JsonSerializer.Serialize(alert);
                var data = $"event: alert\ndata: {json}\n\n";
                var bytes = Encoding.UTF8.GetBytes(data);
                
                await Response.Body.WriteAsync(bytes, cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Alerts stream connection closed by client");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in alerts stream");
        }
    }
}
