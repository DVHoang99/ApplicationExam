param(
    [int]$TimeoutSeconds = 180
)

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$compose = Join-Path $root 'WebAppExam\docker-compose.yml'

if (-not (Test-Path $compose)) {
    Write-Error "docker-compose.yml not found at $compose"
    exit 1
}

Write-Host "Starting docker-compose (this may take a while)..." -ForegroundColor Cyan
& docker-compose -f $compose up -d --build
if ($LASTEXITCODE -ne 0) {
    Write-Error "docker-compose failed to start"
    exit $LASTEXITCODE
}

$services = @('db','kafka','mongodb','webapp','log_consumer','redis')
$deadline = (Get-Date).AddSeconds($TimeoutSeconds)

Write-Host "Waiting for containers to be running (timeout: $TimeoutSeconds seconds)..." -ForegroundColor Cyan
while ((Get-Date) -lt $deadline) {
    $allUp = $true
    foreach ($s in $services) {
        try {
            $cid = & docker-compose -f $compose ps -q $s 2>$null
        }
        catch {
            $cid = $null
        }

        if ([string]::IsNullOrWhiteSpace($cid)) {
            Write-Host "Service '$s': container not found yet" -ForegroundColor Yellow
            $allUp = $false
            continue
        }

        try {
            $running = (& docker inspect -f '{{.State.Running}}' $cid) 2>$null
        }
        catch {
            $running = $null
        }

        if ($running -ne 'true') {
            Write-Host "Service '$s': not running yet" -ForegroundColor Yellow
            $allUp = $false
            continue
        }

        Write-Host "Service '$s': running" -ForegroundColor Green
    }

    if ($allUp) {
        Write-Host "All monitored services are running" -ForegroundColor Green
        break
    }

    Start-Sleep -Seconds 3
}

if ((Get-Date) -ge $deadline) {
    Write-Warning "Timed out waiting for services to become healthy. Check logs for details."
}

Write-Host "Tailing logs for webapp, log_consumer, kafka and mongodb. Press Ctrl+C to quit." -ForegroundColor Cyan
& docker-compose -f $compose logs -f webapp log_consumer kafka mongodb
