Write-Host "Stopping DKS Hotel Manager service processes..." -ForegroundColor Yellow

$processNames = @(
    "dotnet",
    "python",
    "py",
    "go"
)

foreach ($name in $processNames) {
    Get-Process -Name $name -ErrorAction SilentlyContinue | ForEach-Object {
        Write-Host "Stopping $($_.ProcessName) PID=$($_.Id)" -ForegroundColor DarkYellow
        Stop-Process -Id $_.Id -Force
    }
}

Write-Host "All matched service processes stopped." -ForegroundColor Green