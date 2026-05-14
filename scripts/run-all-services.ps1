$root = Split-Path -Parent $PSScriptRoot

Write-Host "Starting DKS Hotel Manager services..." -ForegroundColor Cyan

Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$root\services\auth-service'; dotnet run"
Start-Sleep -Seconds 1

Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$root\services\room-service'; dotnet run"
Start-Sleep -Seconds 1

Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$root\services\booking-service'; dotnet run"
Start-Sleep -Seconds 1

Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$root\services\payment-service'; dotnet run"
Start-Sleep -Seconds 1

Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$root\services\notification-vervice'; dotnet run"
Start-Sleep -Seconds 1

Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$root\gateway\DKS.Gateway'; dotnet run"
Start-Sleep -Seconds 1

Write-Host "Core .NET services and gateway started." -ForegroundColor Green
Write-Host "AI service and ds2api should be started manually if needed." -ForegroundColor Yellow