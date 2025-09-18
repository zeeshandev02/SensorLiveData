using Analytics.Domain.Collections;
using Analytics.Domain.Entities;
using Analytics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Analytics.Infrastructure.Services;

public class DataPersistenceService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DataPersistenceService> _logger;
    private readonly IConfiguration _configuration;
    private readonly RingBuffer<SensorReading> _readingsBuffer;
    private readonly RingBuffer<Alert> _alertsBuffer;
    private readonly SemaphoreSlim _persistenceSemaphore = new(1, 1);

    public DataPersistenceService(
        IServiceProvider serviceProvider,
        ILogger<DataPersistenceService> logger,
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
        var batchSize = 1000;
        var intervalMs = 5000;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PersistDataAsync(batchSize);
                await Task.Delay(intervalMs, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in data persistence service");
                await Task.Delay(5000, stoppingToken); // Wait before retrying
            }
        }
    }

    private async Task PersistDataAsync(int batchSize)
    {
        await _persistenceSemaphore.WaitAsync();
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();

            // Get new readings and alerts
            var newReadings = _readingsBuffer.GetLatest(batchSize);
            var newAlerts = _alertsBuffer.GetLatest(batchSize);

            if (newReadings.Length > 0)
            {
                await context.SensorReadings.AddRangeAsync(newReadings);
                _logger.LogDebug("Persisting {Count} sensor readings", newReadings.Length);
            }

            if (newAlerts.Length > 0)
            {
                await context.Alerts.AddRangeAsync(newAlerts);
                _logger.LogDebug("Persisting {Count} alerts", newAlerts.Length);
            }

            if (newReadings.Length > 0 || newAlerts.Length > 0)
            {
                await context.SaveChangesAsync();
                _logger.LogInformation("Successfully persisted {ReadingsCount} readings and {AlertsCount} alerts", 
                    newReadings.Length, newAlerts.Length);
            }
        }
        finally
        {
            _persistenceSemaphore.Release();
        }
    }
}
