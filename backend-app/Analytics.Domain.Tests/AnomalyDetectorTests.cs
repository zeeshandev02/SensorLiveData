using Analytics.Domain.Entities;
using Analytics.Domain.Services;
using Xunit;

namespace Analytics.Domain.Tests;

public class AnomalyDetectorTests
{
    [Fact]
    public void IsAnomaly_WithNormalValue_ShouldReturnFalse()
    {
        // Arrange
        var detector = new ZScoreAnomalyDetector();
        var reading = new SensorReading
        {
            SensorId = "TEST_001",
            Value = 25.0,
            Timestamp = DateTime.UtcNow,
            Unit = "°C",
            Location = "Test Room"
        };

        var recentReadings = GenerateNormalReadings("TEST_001", 50);

        // Act
        var isAnomaly = detector.IsAnomaly(reading, recentReadings);

        // Assert
        Assert.False(isAnomaly);
    }

    [Fact]
    public void IsAnomaly_WithAnomalousValue_ShouldReturnTrue()
    {
        // Arrange
        var detector = new ZScoreAnomalyDetector();
        var reading = new SensorReading
        {
            SensorId = "TEST_001",
            Value = 100.0, // Very high value
            Timestamp = DateTime.UtcNow,
            Unit = "°C",
            Location = "Test Room"
        };

        var recentReadings = GenerateNormalReadings("TEST_001", 50);

        // Act
        var isAnomaly = detector.IsAnomaly(reading, recentReadings);

        // Assert
        Assert.True(isAnomaly);
    }

    [Fact]
    public void CreateAlert_ShouldReturnValidAlert()
    {
        // Arrange
        var detector = new ZScoreAnomalyDetector();
        var reading = new SensorReading
        {
            SensorId = "TEST_001",
            Value = 100.0,
            Timestamp = DateTime.UtcNow,
            Unit = "°C",
            Location = "Test Room"
        };

        // Act
        var alert = detector.CreateAlert(reading, 30.0, "Temperature too high");

        // Assert
        Assert.NotNull(alert);
        Assert.Equal("TEST_001", alert.SensorId);
        Assert.Equal("Anomaly", alert.Type);
        Assert.Equal("Temperature too high", alert.Message);
        Assert.Equal(100.0, alert.Value);
        Assert.Equal(30.0, alert.Threshold);
        Assert.False(alert.IsResolved);
    }

    private static List<SensorReading> GenerateNormalReadings(string sensorId, int count)
    {
        var readings = new List<SensorReading>();
        var random = new Random(42); // Fixed seed for reproducible tests

        for (int i = 0; i < count; i++)
        {
            readings.Add(new SensorReading
            {
                Id = i,
                SensorId = sensorId,
                Value = 20.0 + random.NextDouble() * 10, // 20-30 range
                Timestamp = DateTime.UtcNow.AddMinutes(-i),
                Unit = "°C",
                Location = "Test Room"
            });
        }

        return readings;
    }
}
