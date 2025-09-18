using Analytics.Domain.Collections;
using Analytics.Domain.Entities;
using Analytics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Analytics.Infrastructure.Services;

public class DataRetentionService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DataRetentionService> _logger;
    private readonly IConfiguration _configuration;
    private readonly RingBuffer<SensorReading> _readingsBuffer;
    private readonly RingBuffer<Alert> _alertsBuffer;

    public DataRetentionService(
        IServiceProvider serviceProvider,
        ILogger<DataRetentionService> logger,
        IConfiguration configuration,
        RingBuffer<SensorReading> readingsBuffer,
        RingBuffer<Alert> alertsBuffer)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
        _readingsBuffer = readingsBuffer;
        _alertsBuffer = alertsBuffer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var retentionHours = 24;
        var intervalHours = 1;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupOldDataAsync(retentionHours);
                await Task.Delay(TimeSpan.FromHours(intervalHours), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in data retention service");
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken); // Wait before retrying
            }
        }
    }

    private async Task CleanupOldDataAsync(int retentionHours)
    {
        var cutoffTime = DateTime.UtcNow.AddHours(-retentionHours);

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();

        // Clean up database
        var deletedReadings = await context.SensorReadings
            .Where(r => r.Timestamp < cutoffTime)
            .ExecuteDeleteAsync();

        var deletedAlerts = await context.Alerts
            .Where(a => a.Timestamp < cutoffTime)
            .ExecuteDeleteAsync();

        _logger.LogInformation("Cleaned up {ReadingsCount} old readings and {AlertsCount} old alerts from database", 
            deletedReadings, deletedAlerts);

        // Note: Ring buffer automatically handles eviction based on capacity
        // No need to manually clean up in-memory data
    }
}
