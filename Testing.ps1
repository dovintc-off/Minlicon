$ErrorActionPreference = "Stop"
$sdk = "$env:LOCALAPPDATA\Android\Sdk"

function Run-BuildProcess {
    param (
        [string]$Name,
        [string]$Arguments
    )

    $psi = [System.Diagnostics.ProcessStartInfo]::new()
    $psi.FileName = "dotnet"
    $psi.Arguments = $Arguments
    $psi.WorkingDirectory = $PWD.Path
    $psi.RedirectStandardOutput = $true
    $psi.RedirectStandardError = $true
    $psi.UseShellExecute = $false
    $psi.CreateNoWindow = $true

    $process = [System.Diagnostics.Process]::new()
    $process.StartInfo = $psi

    $logBuilder = [System.Collections.Generic.List[string]]::new()

    $outEvent = Register-ObjectEvent -InputObject $process -EventName "OutputDataReceived" -Action {
        if ($EventArgs.Data) { $EventSubscriber.MessageData.Add($EventArgs.Data) }
    } -MessageData $logBuilder

    $errEvent = Register-ObjectEvent -InputObject $process -EventName "ErrorDataReceived" -Action {
        if ($EventArgs.Data) { $EventSubscriber.MessageData.Add($EventArgs.Data) }
    } -MessageData $logBuilder

    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    [void]$process.Start()
    
    $process.BeginOutputReadLine()
    $process.BeginErrorReadLine()

    while (-not $process.HasExited) {
        $elapsed = $sw.Elapsed.TotalSeconds
        Write-Host -NoNewline "`rBuilding $Name started ($([math]::Round($elapsed, 1)) sec)"
        Start-Sleep -Milliseconds 100
    }

    $sw.Stop()
    $finalTime = $sw.Elapsed.TotalSeconds
    $exitCode = $process.ExitCode

    Unregister-Event -SourceIdentifier $outEvent.Name
    Unregister-Event -SourceIdentifier $errEvent.Name
    $process.Dispose()

    $fullLog = $logBuilder -join "`n"

    Write-Host -NoNewline "`r$([char]13)" + (" " * 60) + "`r"

    return [PSCustomObject]@{
        Time   = $finalTime
        Failed = $exitCode -ne 0
        Log    = $fullLog
    }
}

$winResult = Run-BuildProcess -Name "Windows" -Arguments "build .\desktop\desktop.csproj -c Release"
$androidResult = Run-BuildProcess -Name "Android" -Arguments "build .\android\android.csproj -c Release `"-p:AndroidSdkDirectory=$sdk`" -m:1"

if ($winResult.Failed) {
    $shortError = ($winResult.Log -split "`n" | Where-Object { $_ -match "error|failed" }) -replace '^\s+|\s+$', ''
    if (!$shortError) { $shortError = ($winResult.Log -split "`n" | Where-Object { $_ -match '\S' } | Select-Object -First 1) }
    if (!$shortError) { $shortError = "Unknown dotnet error" }
    Write-Host "windows | failed ($([math]::Round($winResult.Time, 1)) sec) Error: $shortError" -ForegroundColor Red
} else {
    Write-Host "windows | $([math]::Round($winResult.Time, 1)) sec" -ForegroundColor Green
}

# Android Summary
if ($androidResult.Failed) {
    $shortError = ($androidResult.Log -split "`n" | Where-Object { $_ -match "error|failed" }) -replace '^\s+|\s+$', ''
    if (!$shortError) { $shortError = ($androidResult.Log -split "`n" | Where-Object { $_ -match '\S' } | Select-Object -First 1) }
    if (!$shortError) { $shortError = "Unknown dotnet error" }
    Write-Host "android | failed ($([math]::Round($androidResult.Time, 1)) sec) Error: $shortError" -ForegroundColor Red
} else {
    Write-Host "android | $([math]::Round($androidResult.Time, 1)) sec" -ForegroundColor Green
}
