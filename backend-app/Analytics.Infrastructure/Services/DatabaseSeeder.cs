using Analytics.Domain.Entities;
using Analytics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Analytics.Infrastructure.Services;

public class DatabaseSeeder : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(IServiceProvider serviceProvider, ILogger<DatabaseSeeder> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();

        try
        {
            // Ensure database is created and migrated
            await context.Database.MigrateAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Migration failed, database might already exist. Continuing with seeding...");
        }

        // Seed data
        await SeedDataAsync(context, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task SeedDataAsync(AnalyticsDbContext context, CancellationToken cancellationToken)
    {
        try
        {
            // Check if data already exists
            if (await context.SensorReadings.AnyAsync(cancellationToken))
            {
                _logger.LogInformation("Database already contains data, skipping seeding");
                return;
            }

            _logger.LogInformation("Starting database seeding...");

            // Seed sample sensor readings
            var sampleReadings = GenerateSampleReadings();
            await context.SensorReadings.AddRangeAsync(sampleReadings, cancellationToken);

            // Seed sample alerts
            var sampleAlerts = GenerateSampleAlerts();
            await context.Alerts.AddRangeAsync(sampleAlerts, cancellationToken);

            await context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Database seeding completed successfully. Added {ReadingsCount} readings and {AlertsCount} alerts", 
                sampleReadings.Count, sampleAlerts.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during database seeding");
            throw;
        }
    }

    private static List<SensorReading> GenerateSampleReadings()
    {
        var readings = new List<SensorReading>();
        var random = new Random(42); // Fixed seed for reproducible data
        var sensorIds = new[] { "TEMP_001", "TEMP_002", "TEMP_003", "PRESSURE_001", "HUMIDITY_001" };
        var locations = new[] { "Room A", "Room B", "Room C", "Outdoor", "Basement" };
        var units = new[] { "°C", "°C", "°C", "hPa", "%" };
        var baseValues = new[] { 22.0, 23.5, 21.0, 1013.25, 45.0 };

        // Generate readings for the last 24 hours
        var startTime = DateTime.UtcNow.AddHours(-24);
        var endTime = DateTime.UtcNow;

        for (int i = 0; i < 1000; i++)
        {
            var sensorIndex = i % sensorIds.Length;
            var timestamp = startTime.AddMinutes(i * 1.44); // Spread over 24 hours

            var baseValue = baseValues[sensorIndex];
            var variation = (random.NextDouble() - 0.5) * 10; // ±5 variation
            var value = Math.Round(baseValue + variation, 2);

            readings.Add(new SensorReading
            {
                Id = i + 1,
                SensorId = sensorIds[sensorIndex],
                Value = value,
                Timestamp = timestamp,
                Unit = units[sensorIndex],
                Location = locations[sensorIndex]
            });
        }

        return readings;
    }

    private static List<Alert> GenerateSampleAlerts()
    {
        var alerts = new List<Alert>();
        var random = new Random(42);
        var sensorIds = new[] { "TEMP_001", "TEMP_002", "PRESSURE_001" };
        var alertTypes = new[] { "Anomaly", "Threshold Exceeded", "Sensor Malfunction" };

        // Generate some historical alerts
        for (int i = 0; i < 20; i++)
        {
            var sensorId = sensorIds[random.Next(sensorIds.Length)];
            var alertType = alertTypes[random.Next(alertTypes.Length)];
            var timestamp = DateTime.UtcNow.AddHours(-random.Next(48)); // Last 48 hours

            alerts.Add(new Alert
            {
                Id = i + 1,
                SensorId = sensorId,
                Type = alertType,
                Message = $"{alertType} detected for sensor {sensorId}",
                Value = 25.0 + random.NextDouble() * 10,
                Threshold = 30.0,
                Timestamp = timestamp,
                IsResolved = random.NextDouble() > 0.3 // 70% resolved
            });
        }

        return alerts;
    }
}
