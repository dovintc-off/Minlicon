$sdk = "$env:LOCALAPPDATA\Android\Sdk"
$adb = Join-Path $sdk "platform-tools\adb.exe"
$package = "Minlicon.Android.Minlicon.Android"
$apk = ".\android\bin\Release\net9.0-android\Minlicon.Android.Minlicon.Android-Signed.apk"

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

Write-Host "Delete old version..." -ForegroundColor Yellow -NoNewline
& $adb uninstall $package 2>&1 > $null
[Console]::SetCursorPosition(0, [Console]::CursorTop)
Write-Host "Delete old version: Done                  " -ForegroundColor Green

Write-Host "Install new version" -ForegroundColor Cyan -NoNewline
$installResult = & $adb install -r $apk 2>&1

if ($installResult -match "Success") {
    [Console]::SetCursorPosition(0, [Console]::CursorTop)
    Write-Host "Install new version: Success              " -ForegroundColor Green
    
    foreach ($i in 5..1) {
        [Console]::SetCursorPosition(0, [Console]::CursorTop)
        Write-Host "Waiting  $i sec   " -ForegroundColor Gray -NoNewline
        Start-Sleep -Seconds 1
    }
    [Console]::SetCursorPosition(0, [Console]::CursorTop)
    Write-Host "Waiting: Done                             " -ForegroundColor Green
    
    Write-Host "Run app" -ForegroundColor Cyan -NoNewline
    & $adb shell monkey -p $package 1 2>&1 > $null
    [Console]::SetCursorPosition(0, [Console]::CursorTop)
    Write-Host "Run app: Done                             " -ForegroundColor Green
} else {
    [Console]::SetCursorPosition(0, [Console]::CursorTop)
    Write-Host "Install new version: Failed               " -ForegroundColor Red
    Write-Error "Error install"
    $installResult | Out-String | Write-Host -ForegroundColor Red
}