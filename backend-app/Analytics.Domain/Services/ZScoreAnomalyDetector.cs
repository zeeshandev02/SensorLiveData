using Analytics.Domain.Entities;

namespace Analytics.Domain.Services;

public class ZScoreAnomalyDetector : IAnomalyDetector
{
    private const double ZScoreThreshold = 2.5; // Standard deviation threshold
    private const int WindowSize = 100; // Number of recent readings to consider

    public bool IsAnomaly(SensorReading reading, IEnumerable<SensorReading> recentReadings)
    {
        var readings = recentReadings
            .Where(r => r.SensorId == reading.SensorId)
            .OrderByDescending(r => r.Timestamp)
            .Take(WindowSize)
            .Select(r => r.Value)
            .ToList();

        if (readings.Count < 10) // Need minimum readings for statistical analysis
            return false;

        var mean = readings.Average();
        var variance = readings.Select(x => Math.Pow(x - mean, 2)).Average();
        var standardDeviation = Math.Sqrt(variance);

        if (standardDeviation == 0)
            return false;

        var zScore = Math.Abs(reading.Value - mean) / standardDeviation;
        return zScore > ZScoreThreshold;
    }

    public Alert? CreateAlert(SensorReading reading, double threshold, string reason)
    {
        return new Alert
        {
            SensorId = reading.SensorId,
            Type = "Anomaly",
            Message = reason,
            Value = reading.Value,
            Threshold = threshold,
            Timestamp = reading.Timestamp,
            IsResolved = false
        };
    }
}
