using Analytics.Domain.Collections;
using Analytics.Domain.Entities;
using Xunit;

namespace Analytics.Domain.Tests;

public class RingBufferTests
{
    [Fact]
    public void Add_ShouldAddItemToBuffer()
    {
        // Arrange
        var buffer = new RingBuffer<SensorReading>(10);
        var reading = new SensorReading
        {
            Id = 1,
            SensorId = "TEST_001",
            Value = 25.5,
            Timestamp = DateTime.UtcNow,
            Unit = "°C",
            Location = "Test Room"
        };

        // Act
        buffer.Add(reading);

        // Assert
        Assert.Equal(1, buffer.Count);
    }

    [Fact]
    public void Add_WhenBufferIsFull_ShouldEvictOldestItem()
    {
        // Arrange
        var buffer = new RingBuffer<SensorReading>(3);
        
        // Act
        for (int i = 0; i < 5; i++)
        {
            buffer.Add(new SensorReading
            {
                Id = i,
                SensorId = "TEST_001",
                Value = i,
                Timestamp = DateTime.UtcNow,
                Unit = "°C",
                Location = "Test Room"
            });
        }

        // Assert
        Assert.Equal(3, buffer.Count);
    }

    [Fact]
    public void GetLatest_ShouldReturnLatestItems()
    {
        // Arrange
        var buffer = new RingBuffer<SensorReading>(5);
        
        for (int i = 0; i < 3; i++)
        {
            buffer.Add(new SensorReading
            {
                Id = i,
                SensorId = "TEST_001",
                Value = i,
                Timestamp = DateTime.UtcNow,
                Unit = "°C",
                Location = "Test Room"
            });
        }

        // Act
        var latest = buffer.GetLatest(2);

        // Assert
        Assert.Equal(2, latest.Length);
        Assert.Equal(1, latest[0].Id); // Second to last
        Assert.Equal(2, latest[1].Id); // Last
    }

    [Fact]
    public void GetAll_ShouldReturnAllItemsInOrder()
    {
        // Arrange
        var buffer = new RingBuffer<SensorReading>(5);
        
        for (int i = 0; i < 3; i++)
        {
            buffer.Add(new SensorReading
            {
                Id = i,
                SensorId = "TEST_001",
                Value = i,
                Timestamp = DateTime.UtcNow,
                Unit = "°C",
                Location = "Test Room"
            });
        }

        // Act
        var all = buffer.GetAll();

        // Assert
        Assert.Equal(3, all.Length);
        Assert.Equal(0, all[0].Id);
        Assert.Equal(1, all[1].Id);
        Assert.Equal(2, all[2].Id);
    }
}
