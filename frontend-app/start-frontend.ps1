# PowerShell script to start the frontend
Write-Host "Installing dependencies..." -ForegroundColor Green
npm install

Write-Host "Starting Angular development server..." -ForegroundColor Green
Write-Host "Frontend will be available at: http://localhost:4200" -ForegroundColor Yellow
Write-Host "Press Ctrl+C to stop the server" -ForegroundColor Cyan

npm start
