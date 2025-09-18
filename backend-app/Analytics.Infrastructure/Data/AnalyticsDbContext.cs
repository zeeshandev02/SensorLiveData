using Analytics.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Infrastructure.Data;

public class AnalyticsDbContext : DbContext
{
    public AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : base(options)
    {
    }

    public DbSet<SensorReading> SensorReadings { get; set; }
    public DbSet<Alert> Alerts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SensorReading>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SensorId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Value).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Unit).HasMaxLength(10);
            entity.Property(e => e.Location).HasMaxLength(100);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.SensorId);
        });

        modelBuilder.Entity<Alert>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SensorId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Message).HasMaxLength(500);
            entity.Property(e => e.Value).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Threshold).HasColumnType("decimal(18,2)");
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.SensorId);
        });
    }
}
