using Analytics.Domain.Entities;

namespace Analytics.Domain.Services;

public interface IAnomalyDetector
{
    bool IsAnomaly(SensorReading reading, IEnumerable<SensorReading> recentReadings);
    Alert? CreateAlert(SensorReading reading, double threshold, string reason);
}
