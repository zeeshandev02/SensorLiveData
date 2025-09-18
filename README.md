# Real-time Analytics Dashboard

A high-performance real-time analytics dashboard built with .NET 8 Web API and Angular 16+ that simulates sensor data streaming and provides live visualization with anomaly detection.

## Features

### Backend (.NET 8 Web API)
- **Clean Architecture** with Api, Application, Domain, and Infrastructure layers
- **Server-Sent Events (SSE)** for real-time data streaming
- **In-memory Ring Buffer** (100k capacity) for O(1) insert/evict operations
- **SQLite Database** with EF Core for data persistence
- **Background Services** for data persistence and retention cleanup
- **Anomaly Detection** using rolling z-score algorithm
- **Configurable Settings** via appsettings.json
- **Structured Logging** with Serilog

### Frontend (Angular 16+)
- **Real-time Chart** with data decimation for smooth performance
- **Live Statistics Panel** showing min/max/avg/throughput
- **Alerts Panel** displaying anomaly alerts in real-time
- **EventSource Integration** for SSE streams
- **Responsive Design** with modern UI/UX
- **Bounded Client Buffer** (100k max readings)

## Quick Start

### Prerequisites
- .NET 8 SDK
- Node.js 18+ and npm
- Visual Studio Code or Visual Studio 2022

### Backend Setup

1. Navigate to the backend directory:
```bash
cd backend-app
```

2. Restore packages and build:
```bash
dotnet restore
dotnet build
```

3. Run the API:
```bash
dotnet run --project Analytics.Api
```

The API will start at `https://localhost:5000` (or `http://localhost:5000` if HTTPS is disabled).

### Frontend Setup

1. Navigate to the frontend directory:
```bash
cd frontend-app
```

2. Install dependencies:
```bash
npm install
```

3. Start the development server:
```bash
npm start
```

The Angular app will start at `http://localhost:4200`.

## Configuration

### Backend Configuration (appsettings.json)

```json
{
  "Simulation": {
    "ReadingsPerSecond": 1000
  },
  "Persistence": {
    "BatchSize": 1000,
    "IntervalMs": 5000
  },
  "Retention": {
    "Hours": 24,
    "CleanupIntervalHours": 1
  }
}
```

### Development vs Production

- **Development**: 100 readings/second (configurable in appsettings.Development.json)
- **Production**: 1000 readings/second (configurable in appsettings.json)

## API Endpoints

### REST Endpoints
- `GET /api/readings/latest?limit=1000` - Get latest sensor readings
- `GET /api/readings/aggregates?windowSeconds=60` - Get reading aggregates (min/max/avg/throughput)
- `GET /api/alerts?limit=100` - Get recent anomaly alerts

### SSE Endpoints
- `GET /api/stream/readings` - Continuous stream of new readings
- `GET /api/stream/alerts` - Continuous stream of alerts

## Architecture

### Backend Architecture
```
Analytics.Api/          # Web API layer
├── Controllers/        # REST and SSE controllers
└── Program.cs         # Application startup

Analytics.Application/  # Application layer
├── Services/          # Business logic services
└── Interfaces/        # Service contracts

Analytics.Domain/       # Domain layer
├── Entities/          # Domain models
├── Collections/       # Ring buffer implementation
└── Services/          # Domain services (anomaly detection)

Analytics.Infrastructure/ # Infrastructure layer
├── Data/              # EF Core DbContext
└── Services/          # Background services
```

### Frontend Architecture
```
src/app/
├── components/        # UI components
│   ├── live-chart/    # Real-time chart component
│   ├── stats-panel/   # Statistics display
│   └── alerts-panel/  # Alerts display
├── services/          # Angular services
│   ├── api.service.ts     # REST API calls
│   ├── sse.service.ts     # SSE connections
│   └── state.service.ts   # State management
└── models/            # TypeScript interfaces
```

## Performance Features

### Data Decimation
The frontend implements data decimation to handle up to 100k data points smoothly:
- Simple decimation algorithm reduces data points for chart rendering
- Maintains data integrity while improving UI performance
- Configurable decimation factor

### Memory Management
- **Backend**: Ring buffer with 100k capacity, automatic eviction
- **Frontend**: Bounded client buffer with automatic cleanup
- **Database**: 24-hour retention policy with background cleanup

### Real-time Streaming
- **SSE**: Low-latency streaming for real-time updates
- **Fallback Polling**: REST API polling as backup mechanism
- **Connection Management**: Automatic reconnection on connection loss

## Monitoring and Logging

### Structured Logging
- **Serilog** with console and file sinks
- **Log Levels**: Information, Warning, Error
- **Log Rotation**: Daily log files with retention

### Health Monitoring
- Connection status indicators
- Real-time throughput metrics
- Alert generation and tracking

## Testing

### Backend Tests
```bash
cd backend-app
dotnet test
```

Tests include:
- Ring buffer operations
- Anomaly detection algorithms
- SSE endpoint connectivity

### Frontend Tests
```bash
cd frontend-app
npm test
```

## Demo Instructions

1. **Start the Backend**: Run `dotnet run` in the backend-app directory
2. **Start the Frontend**: Run `npm start` in the frontend-app directory
3. **Open Browser**: Navigate to `http://localhost:4200`
4. **Observe**: Watch real-time sensor data, statistics, and alerts

### What to Look For
- **Live Chart**: Real-time sensor readings plotted on a chart
- **Statistics Panel**: Min/max/average values and throughput metrics
- **Alerts Panel**: Anomaly alerts appearing in real-time
- **Connection Status**: Green indicator showing SSE connection is active

## Troubleshooting

### Common Issues

1. **CORS Errors**: Ensure the backend CORS policy allows `http://localhost:4200`
2. **SSE Connection Issues**: Check that the backend is running on the correct port
3. **High CPU Usage**: Reduce `ReadingsPerSecond` in development settings
4. **Memory Issues**: Check ring buffer capacity and retention settings

### Logs
- Backend logs: `backend-app/logs/analytics-*.txt`
- Frontend logs: Browser developer console

## Development Notes

### Adding New Sensor Types
1. Update sensor ID generation in `SensorDataService`
2. Add new sensor configuration in `appsettings.json`
3. Update frontend display logic if needed

### Modifying Anomaly Detection
1. Adjust `ZScoreThreshold` in `ZScoreAnomalyDetector`
2. Modify `WindowSize` for different sensitivity
3. Add new detection algorithms in the Domain layer

### Scaling Considerations
- **Database**: Consider PostgreSQL for production
- **Caching**: Add Redis for distributed caching
- **Load Balancing**: Multiple API instances with shared state
- **Message Queue**: Use RabbitMQ or Azure Service Bus for high throughput

## License

This project is for demonstration purposes. Feel free to use and modify as needed.
