# PowerShell script to start the entire analytics system
Write-Host "=== Real-time Analytics Dashboard Startup ===" -ForegroundColor Cyan
Write-Host ""

# Check if .NET 8 is installed
Write-Host "Checking .NET 8 installation..." -ForegroundColor Green
try {
    $dotnetVersion = dotnet --version
    if ($dotnetVersion -match "^8\.") {
        Write-Host "✓ .NET 8 found: $dotnetVersion" -ForegroundColor Green
    } else {
        Write-Host "✗ .NET 8 not found. Please install .NET 8 SDK" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "✗ .NET not found. Please install .NET 8 SDK" -ForegroundColor Red
    exit 1
}

# Check if Node.js is installed
Write-Host "Checking Node.js installation..." -ForegroundColor Green
try {
    $nodeVersion = node --version
    Write-Host "✓ Node.js found: $nodeVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ Node.js not found. Please install Node.js 18+" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Starting Backend..." -ForegroundColor Yellow
Write-Host "Backend will be available at: https://localhost:5000" -ForegroundColor Cyan
Write-Host "Swagger UI: https://localhost:5000/swagger" -ForegroundColor Cyan
Write-Host ""

# Start backend in background
$backendJob = Start-Job -ScriptBlock {
    Set-Location "backend-app"
    dotnet run --project Analytics.Api
}

# Wait a moment for backend to start
Start-Sleep -Seconds 5

Write-Host "Starting Frontend..." -ForegroundColor Yellow
Write-Host "Frontend will be available at: http://localhost:4200" -ForegroundColor Cyan
Write-Host ""

# Start frontend in background
$frontendJob = Start-Job -ScriptBlock {
    Set-Location "frontend-app"
    npm install
    npm start
}

Write-Host ""
Write-Host "=== System Started Successfully ===" -ForegroundColor Green
Write-Host "Backend: https://localhost:5000" -ForegroundColor White
Write-Host "Frontend: http://localhost:4200" -ForegroundColor White
Write-Host ""
Write-Host "Press any key to stop both services..." -ForegroundColor Yellow
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

Write-Host ""
Write-Host "Stopping services..." -ForegroundColor Yellow
Stop-Job $backendJob, $frontendJob
Remove-Job $backendJob, $frontendJob

Write-Host "System stopped." -ForegroundColor Green
