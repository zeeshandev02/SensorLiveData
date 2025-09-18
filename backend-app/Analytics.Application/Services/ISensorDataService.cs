using Analytics.Domain.Entities;

namespace Analytics.Application.Services;

public interface ISensorDataService
{
    Task StartSimulationAsync();
    Task StopSimulationAsync();
    Task<SensorReading[]> GetLatestReadingsAsync(int limit = 1000);
    Task<ReadingAggregate[]> GetAggregatesAsync(int windowSeconds = 60);
    Task<Alert[]> GetRecentAlertsAsync(int limit = 100);
    IAsyncEnumerable<SensorReading> StreamReadingsAsync(CancellationToken cancellationToken);
    IAsyncEnumerable<Alert> StreamAlertsAsync(CancellationToken cancellationToken);
}
