$ErrorActionPreference = "Stop"

$projectRoot = $PSScriptRoot
$processes = [System.Collections.Generic.List[System.Diagnostics.Process]]::new()

function Start-Component {
    param(
        [string]$Name,
        [string[]]$Arguments
    )

    $process = Start-Process `
        -FilePath "dotnet" `
        -ArgumentList $Arguments `
        -WorkingDirectory $projectRoot `
        -NoNewWindow `
        -PassThru

    $processes.Add($process)
    Write-Host "$Name started (PID $($process.Id))"
}

try {
    $apiProject = Join-Path $projectRoot "Api\DatabaseAPI.csproj"
    $gatewayProject = Join-Path $projectRoot "Gateway\Gateway.csproj"
    $webAppProject = Join-Path $projectRoot "WebApp\BlazorApp.csproj"

    Start-Component "API 1" @(
        "run",
        "--project", ('"' + $apiProject + '"'),
        "--no-build",
        "--urls", "http://localhost:5081",
        "--",
        "--ApiInstance=api-1"
    )

    Start-Component "API 2" @(
        "run",
        "--project", ('"' + $apiProject + '"'),
        "--no-build",
        "--urls", "http://localhost:5082",
        "--",
        "--ApiInstance=api-2"
    )

    Start-Component "Gateway" @(
        "run",
        "--project", ('"' + $gatewayProject + '"'),
        "--no-build",
        "--urls", "http://localhost:5000"
    )

    Start-Component "WebApp" @(
        "run",
        "--project", ('"' + $webAppProject + '"'),
        "--no-build",
        "--launch-profile", "http"
    )

    Write-Host ""
    Write-Host "SearchSystem is running at http://localhost:5002"
    Write-Host "Press Ctrl+C to stop all components."
    Write-Host ""

    Wait-Process -Id $processes.Id
}
finally {
    foreach ($process in $processes) {
        if (-not $process.HasExited) {
            Stop-Process -Id $process.Id -Force
        }
    }
}
