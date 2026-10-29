# Minlicon

Minlicon is a small MonoGame tile-map prototype targeting Windows desktop and Android.

## Requirements

- .NET 9 SDK
- For Android: Android SDK, the .NET Android workload, and a Java Development Kit
- For installing to a phone: USB debugging enabled and Android Platform-Tools (`adb`)

## Build and run on Windows

From the repository root:

```powershell
dotnet build .\desktop\desktop.csproj -c Release
dotnet run --project .\desktop\desktop.csproj -c Release
```

The desktop output is written to `desktop\bin\Release\net9.0\`.

## Build and install on Android

Install the Android workload once:

```powershell
dotnet workload install android
```

Build a Release APK and install it on a connected, authorized device:

```powershell
$sdk = "$env:LOCALAPPDATA\Android\Sdk"
$adb = Join-Path $sdk "platform-tools\adb.exe"

dotnet build .\android\android.csproj -c Release "-p:AndroidSdkDirectory=$sdk" -m:1
if ($LASTEXITCODE -ne 0) { throw "Android build failed." }

& $adb devices
$apk = ".\android\bin\Release\net9.0-android\Minlicon.Android.Minlicon.Android-Signed.apk"
& $adb install -r $apk
if ($LASTEXITCODE -ne 0) { throw "APK installation failed." }
& $adb shell monkey -p Minlicon.Android.Minlicon.Android 1
```

Enable USB debugging on the device and accept its authorization prompt. Use the Release APK for direct installation; Debug builds may depend on Fast Deployment.

More detailed commands are in [BUILD.md](BUILD.md).

## Controls

- **Desktop:** WASD moves the camera, hold and drag the left mouse button to pan, use the mouse wheel to zoom, and press F3 to toggle debug details.
- **Android:** drag with one finger to pan, pinch with two fingers to zoom, and rotate the device to use the landscape layout.

## Content

Shared source assets are in `core/Content`. The MonoGame content manifests compile them for desktop and Android.

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for the contribution and validation workflow.

## License

This project is distributed under the GNU General Public License, version 3. See [LICENSE](LICENSE).
