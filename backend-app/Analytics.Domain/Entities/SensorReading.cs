namespace Analytics.Domain.Entities;

public class SensorReading
{
    public long Id { get; set; }
    public string SensorId { get; set; } = string.Empty;
    public double Value { get; set; }
    public DateTime Timestamp { get; set; }
    public string Unit { get; set; } = "°C";
    public string Location { get; set; } = string.Empty;
}
