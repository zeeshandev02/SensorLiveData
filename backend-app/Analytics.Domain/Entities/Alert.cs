namespace Analytics.Domain.Entities;

public class Alert
{
    public long Id { get; set; }
    public string SensorId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public double Value { get; set; }
    public double Threshold { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsResolved { get; set; }
}
