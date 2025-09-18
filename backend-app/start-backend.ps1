# PowerShell script to start the backend
Write-Host "Starting Analytics API..." -ForegroundColor Green
Write-Host "Backend will be available at: https://localhost:5000" -ForegroundColor Yellow
Write-Host "Swagger UI will be available at: https://localhost:5000/swagger" -ForegroundColor Yellow
Write-Host "Press Ctrl+C to stop the server" -ForegroundColor Cyan

dotnet run --project Analytics.Api
