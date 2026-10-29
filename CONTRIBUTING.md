# Contributing

Thanks for your interest in Minlicon.

## Before making changes

1. Open an issue describing a significant bug or feature before starting large changes.
2. Keep pull requests focused and explain the user-visible effect.
3. Do not include generated `bin`, `obj`, `.vs`, APK, or other build output.

## Dependencies

Project dependencies are declared in the `.csproj` files and restored from NuGet by the .NET SDK. You do not need to download package files manually. After changing a package reference, restore and build the affected project:

```powershell
dotnet restore .\desktop\desktop.csproj
dotnet restore .\android\android.csproj
```

The desktop project uses MonoGame DesktopGL. Android uses MonoGame Android and additionally requires the Android SDK, Java, and the .NET Android workload. Keep package changes in the relevant project file and do not commit local NuGet caches.

## Validate changes

Install the .NET 9 SDK. For Android changes, also install the Android workload and Android SDK. From the repository root, run the relevant builds:

```powershell
dotnet build .\desktop\desktop.csproj -c Release
dotnet build .\android\android.csproj -c Release -p:AndroidSdkDirectory="$env:LOCALAPPDATA\Android\Sdk" -m:1
```

If test projects are present, run each test project directly:

```powershell
dotnet test .\path\to\Your.Tests.csproj -c Release
```

The GitHub Actions workflow builds both platforms and runs discovered test projects.

## Make a change and open a pull request

Fork the repository on GitHub, then clone your fork and create a focused branch:

```powershell
git clone https://github.com/dovintc-off/Minlicon.git
cd Minlicon
git switch -c <short-change-name>
```

Make the change, build the affected platform(s), and commit it:

```powershell
git add <changed-files>
git commit -m "Describe the change"
git push -u origin <short-change-name>
```

Open a Pull Request from your branch to `dovintc-off/Minlicon` on GitHub. Describe the problem and approach, include reproduction or manual test steps where applicable, and mention platform-specific behavior or setup requirements. Check that the GitHub Actions CI workflow passes.
