using Analytics.Application.Services;
using Analytics.Domain.Collections;
using Analytics.Domain.Services;
using Analytics.Infrastructure.Data;
using Analytics.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/analytics-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Configure SQLite
builder.Services.AddDbContext<AnalyticsDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register domain services
builder.Services.AddSingleton<RingBuffer<Analytics.Domain.Entities.SensorReading>>(provider => 
    new RingBuffer<Analytics.Domain.Entities.SensorReading>(100000));
builder.Services.AddSingleton<RingBuffer<Analytics.Domain.Entities.Alert>>(provider => 
    new RingBuffer<Analytics.Domain.Entities.Alert>(10000));
builder.Services.AddSingleton<IAnomalyDetector, ZScoreAnomalyDetector>();

// Register application services
builder.Services.AddSingleton<ISensorDataService, SensorDataService>();
builder.Services.AddHostedService<SensorDataService>(provider => 
    (SensorDataService)provider.GetRequiredService<ISensorDataService>());

// Register infrastructure services
builder.Services.AddHostedService<DataPersistenceService>();
builder.Services.AddHostedService<DataRetentionService>();
builder.Services.AddHostedService<DatabaseSeeder>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngular");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();
    context.Database.EnsureCreated();
}

try
{
    Log.Information("Starting Analytics API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
