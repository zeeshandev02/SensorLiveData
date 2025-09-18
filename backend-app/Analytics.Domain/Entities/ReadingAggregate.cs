namespace Analytics.Domain.Entities;

public class ReadingAggregate
{
    public string SensorId { get; set; } = string.Empty;
    public double Min { get; set; }
    public double Max { get; set; }
    public double Average { get; set; }
    public int Count { get; set; }
    public DateTime WindowStart { get; set; }
    public DateTime WindowEnd { get; set; }
    public double Throughput { get; set; } // readings per second
}
