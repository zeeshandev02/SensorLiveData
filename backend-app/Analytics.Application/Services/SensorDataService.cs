using Analytics.Domain.Collections;
using Analytics.Domain.Entities;
using Analytics.Domain.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Analytics.Application.Services;

public class SensorDataService : ISensorDataService, IHostedService
{
    private readonly RingBuffer<SensorReading> _readingsBuffer;
    private readonly RingBuffer<Alert> _alertsBuffer;
    private readonly IAnomalyDetector _anomalyDetector;
    private readonly ILogger<SensorDataService> _logger;
    private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
    private readonly SemaphoreSlim _simulationSemaphore = new(1, 1);
    private readonly ConcurrentDictionary<string, Channel<SensorReading>> _readingChannels = new();
    private readonly ConcurrentDictionary<string, Channel<Alert>> _alertChannels = new();
    
    private Timer? _simulationTimer;
    private bool _isSimulationRunning = false;
    private readonly Random _random = new();
    private readonly string[] _sensorIds = { "TEMP_001", "TEMP_002", "TEMP_003", "PRESSURE_001", "HUMIDITY_001" };
    private readonly string[] _locations = { "Room A", "Room B", "Room C", "Outdoor", "Basement" };

    public SensorDataService(
        ILogger<SensorDataService> logger,
        Microsoft.Extensions.Configuration.IConfiguration configuration,
        IAnomalyDetector anomalyDetector)
    {
        _logger = logger;
        _configuration = configuration;
        _anomalyDetector = anomalyDetector;
        _readingsBuffer = new RingBuffer<SensorReading>(100000); // 100k capacity
        _alertsBuffer = new RingBuffer<Alert>(10000); // 10k alerts capacity
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await StartSimulationAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await StopSimulationAsync();
    }

    public async Task StartSimulationAsync()
    {
        await _simulationSemaphore.WaitAsync();
        try
        {
            if (_isSimulationRunning)
                return;

            var readingsPerSecond = 1000;
            var intervalMs = 1000 / readingsPerSecond;

            _simulationTimer = new Timer(GenerateReading, null, 0, intervalMs);
            _isSimulationRunning = true;

            _logger.LogInformation("Sensor simulation started with {ReadingsPerSecond} readings/second", readingsPerSecond);
        }
        finally
        {
            _simulationSemaphore.Release();
        }
    }

    public async Task StopSimulationAsync()
    {
        await _simulationSemaphore.WaitAsync();
        try
        {
            _simulationTimer?.Dispose();
            _simulationTimer = null;
            _isSimulationRunning = false;

            _logger.LogInformation("Sensor simulation stopped");
        }
        finally
        {
            _simulationSemaphore.Release();
        }
    }

    private void GenerateReading(object? state)
    {
        try
        {
            var sensorId = _sensorIds[_random.Next(_sensorIds.Length)];
            var location = _locations[_random.Next(_locations.Length)];
            var baseValue = sensorId.StartsWith("TEMP") ? 20.0 : sensorId.StartsWith("PRESSURE") ? 1013.25 : 50.0;
            var value = baseValue + (_random.NextDouble() - 0.5) * 10; // ±5 variation
            var unit = sensorId.StartsWith("TEMP") ? "°C" : sensorId.StartsWith("PRESSURE") ? "hPa" : "%";

            var reading = new SensorReading
            {
                SensorId = sensorId,
                Value = Math.Round(value, 2),
                Timestamp = DateTime.UtcNow,
                Unit = unit,
                Location = location
            };

            // Add to ring buffer
            _readingsBuffer.Add(reading);

            // Check for anomalies
            var recentReadings = _readingsBuffer.GetLatest(100);
            if (_anomalyDetector.IsAnomaly(reading, recentReadings))
            {
                var alert = _anomalyDetector.CreateAlert(reading, 0, $"Anomaly detected: {reading.Value} {reading.Unit}");
                if (alert != null)
                {
                    _alertsBuffer.Add(alert);
                    _logger.LogWarning("Anomaly detected for sensor {SensorId}: {Value} {Unit}", sensorId, reading.Value, reading.Unit);
                }
            }

            // Broadcast to SSE channels
            BroadcastReading(reading);
            if (_anomalyDetector.IsAnomaly(reading, recentReadings))
            {
                var alert = _anomalyDetector.CreateAlert(reading, 0, $"Anomaly detected: {reading.Value} {reading.Unit}");
                if (alert != null)
                {
                    BroadcastAlert(alert);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating sensor reading");
        }
    }

    private void BroadcastReading(SensorReading reading)
    {
        foreach (var channel in _readingChannels.Values)
        {
            if (channel.Writer.TryWrite(reading))
            {
                // Successfully written
            }
        }
    }

    private void BroadcastAlert(Alert alert)
    {
        foreach (var channel in _alertChannels.Values)
        {
            if (channel.Writer.TryWrite(alert))
            {
                // Successfully written
            }
        }
    }

    public async Task<SensorReading[]> GetLatestReadingsAsync(int limit = 1000)
    {
        return await Task.FromResult(_readingsBuffer.GetLatest(limit));
    }

    public async Task<ReadingAggregate[]> GetAggregatesAsync(int windowSeconds = 60)
    {
        var cutoff = DateTime.UtcNow.AddSeconds(-windowSeconds);
        var recentReadings = _readingsBuffer.GetRange(cutoff, DateTime.UtcNow);
        
        var aggregates = recentReadings
            .GroupBy(r => r.SensorId)
            .Select(g => new ReadingAggregate
            {
                SensorId = g.Key,
                Min = g.Min(r => r.Value),
                Max = g.Max(r => r.Value),
                Average = g.Average(r => r.Value),
                Count = g.Count(),
                WindowStart = cutoff,
                WindowEnd = DateTime.UtcNow,
                Throughput = g.Count() / (double)windowSeconds
            })
            .ToArray();

        return await Task.FromResult(aggregates);
    }

    public async Task<Alert[]> GetRecentAlertsAsync(int limit = 100)
    {
        return await Task.FromResult(_alertsBuffer.GetLatest(limit));
    }

    public async IAsyncEnumerable<SensorReading> StreamReadingsAsync([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var channel = Channel.CreateUnbounded<SensorReading>();
        var channelId = Guid.NewGuid().ToString();
        _readingChannels[channelId] = channel;

        try
        {
            await foreach (var reading in channel.Reader.ReadAllAsync(cancellationToken))
            {
                yield return reading;
            }
        }
        finally
        {
            _readingChannels.TryRemove(channelId, out _);
        }
    }

    public async IAsyncEnumerable<Alert> StreamAlertsAsync([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var channel = Channel.CreateUnbounded<Alert>();
        var channelId = Guid.NewGuid().ToString();
        _alertChannels[channelId] = channel;

        try
        {
            await foreach (var alert in channel.Reader.ReadAllAsync(cancellationToken))
            {
                yield return alert;
            }
        }
        finally
        {
            _alertChannels.TryRemove(channelId, out _);
        }
    }
}
