$gateway = "http://localhost:6000"

Write-Host "Testing API Gateway health..." -ForegroundColor Cyan
Invoke-RestMethod "$gateway/gateway/health"

Write-Host "`nTesting room-service..." -ForegroundColor Cyan
Invoke-RestMethod "$gateway/api/room/test"

Write-Host "`nTesting booking-service..." -ForegroundColor Cyan
Invoke-RestMethod "$gateway/api/booking/test"

Write-Host "`nTesting payment-service..." -ForegroundColor Cyan
Invoke-RestMethod "$gateway/api/payment/test"

Write-Host "`nTesting notification-service..." -ForegroundColor Cyan
Invoke-RestMethod "$gateway/api/notification/test"

Write-Host "`nAll gateway routes tested." -ForegroundColor Green