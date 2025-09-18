# PowerShell script to run tests
Write-Host "Running Analytics Domain Tests..." -ForegroundColor Green
dotnet test Analytics.Domain.Tests/Analytics.Domain.Tests.csproj --verbosity normal

if ($LASTEXITCODE -eq 0) {
    Write-Host "All tests passed!" -ForegroundColor Green
} else {
    Write-Host "Some tests failed!" -ForegroundColor Red
    exit 1
}
