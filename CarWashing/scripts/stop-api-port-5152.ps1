$processIds = Get-NetTCPConnection -LocalPort 5152 -State Listen -ErrorAction SilentlyContinue |
    Select-Object -ExpandProperty OwningProcess -Unique

if (-not $processIds) {
    Write-Host "Port 5152 is free."
    exit 0
}

foreach ($processId in $processIds) {
    Write-Host "Stopping process $processId on port 5152..."
    Stop-Process -Id $processId -Force
}

Write-Host "Port 5152 is free."
