# PowerShell deployment script for Analytics API
param(
    [string]$Environment = "Development",
    [string]$ConnectionString = "Data Source=analytics.db",
    [switch]$SkipSeeding = $false
)

Write-Host "=== Analytics API Deployment Script ===" -ForegroundColor Cyan
Write-Host "Environment: $Environment" -ForegroundColor Yellow
Write-Host "Connection String: $ConnectionString" -ForegroundColor Yellow
Write-Host ""

# Check if .NET is installed
Write-Host "Checking .NET installation..." -ForegroundColor Green
try {
    $dotnetVersion = dotnet --version
    Write-Host "✓ .NET found: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ .NET not found. Please install .NET 8 SDK" -ForegroundColor Red
    exit 1
}

# Restore packages
Write-Host "Restoring NuGet packages..." -ForegroundColor Green
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Package restore failed" -ForegroundColor Red
    exit 1
}

# Build solution
Write-Host "Building solution..." -ForegroundColor Green
dotnet build --configuration Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Build failed" -ForegroundColor Red
    exit 1
}

# Run tests
Write-Host "Running tests..." -ForegroundColor Green
dotnet test --configuration Release --no-build
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Tests failed" -ForegroundColor Red
    exit 1
}

# Create logs directory
Write-Host "Creating logs directory..." -ForegroundColor Green
if (!(Test-Path "logs")) {
    New-Item -ItemType Directory -Path "logs" | Out-Null
}

# Update appsettings for deployment
Write-Host "Updating configuration..." -ForegroundColor Green
$appsettingsPath = "Analytics.Api\appsettings.$Environment.json"
if (!(Test-Path $appsettingsPath)) {
    $appsettingsPath = "Analytics.Api\appsettings.json"
}

# Set environment variable
$env:ASPNETCORE_ENVIRONMENT = $Environment

Write-Host ""
Write-Host "=== Deployment Complete ===" -ForegroundColor Green
Write-Host "To start the API, run:" -ForegroundColor White
Write-Host "  dotnet run --project Analytics.Api --configuration Release" -ForegroundColor Cyan
Write-Host ""
Write-Host "API will be available at:" -ForegroundColor White
Write-Host "  HTTPS: https://localhost:5000" -ForegroundColor Cyan
Write-Host "  HTTP:  http://localhost:5001" -ForegroundColor Cyan
Write-Host "  Swagger: https://localhost:5000/swagger" -ForegroundColor Cyan
Write-Host ""

if (!$SkipSeeding) {
    Write-Host "Database will be automatically seeded with sample data on first run." -ForegroundColor Yellow
}
