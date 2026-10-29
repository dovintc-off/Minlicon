# Build & Run Minlicon

This guide explains how to build and run the Minlicon project on Windows (Desktop) and Android devices.

---

## PC (Windows)

To build and launch the desktop version, open PowerShell in the project root directory and run:

```powershell
# Build the project and generate the .exe file
dotnet build .\desktop\desktop.csproj -c Release

# Run the desktop application
dotnet run --project .\desktop\desktop.csproj -c Release
```

The compiled binaries will be available in: `.\desktop\bin\Release\net9.0\`

---

## Android (Build & Deploy via ADB)

### ⚠️ Prerequisites
Before proceeding, ensure you have:
1. **Android SDK** & **.NET Android workload** installed on your PC.
2. **USB Debugging** enabled on your Android phone.
3. The phone connected to your PC via USB (and the debugging prompt accepted on the device screen).

> **Note:** The Release APK is built with optimization enabled. Always use the Release build for standalone device installation instead of the Debug APK, as the latter relies on Fast Deployment and may fail to launch without an active IDE debugger connection.

### Build and Installation Steps

Run the following commands from PowerShell in the project root:

```powershell
# 1. Set paths to Android SDK and ADB tool
sdk = "env:LOCALAPPDATA\Android\Sdk"
adb = Join-Path sdk "platform-tools\adb.exe"

# 2. Build the signed Release APK
dotnet build .\android\android.csproj -c Release "-p:AndroidSdkDirectory=\$sdk" -m:1
if (\$LASTEXITCODE -ne 0) { throw "Android build failed with errors!" }

# 3. Verify your connected device
& \$adb devices
```

**Stop here and check the console output:** Your phone must appear in the device list with the `device` status. If it's listed, proceed to install and automatically launch the application:

```powershell
# 4. Install the signed APK to your phone (overwriting any previous version)
\$apk = ".\android\bin\Release\net9.0-android\Minlicon.Android.Minlicon.Android-Signed.apk"
& adb install -r apk
if (\$LASTEXITCODE -ne 0) { throw "Failed to install the APK." }

# 5. Launch the application on the device automatically
& \$adb shell monkey -p Minlicon.Android.Minlicon.Android 1
```
