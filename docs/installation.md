# Installation

## NuGet Package

MAUI Controls Extras is available as a NuGet package.

### Package Manager Console

```powershell
Install-Package StefK.MauiControlsExtras
```

### .NET CLI

```bash
dotnet add package StefK.MauiControlsExtras
```

### PackageReference

Add to your `.csproj` file:

```xml
<PackageReference Include="StefK.MauiControlsExtras" Version="1.0.0" />
```

## Requirements

- **.NET 10.0** or later
- **.NET MAUI** workload installed

## Supported Platforms

| Platform | Minimum Version |
|----------|-----------------|
| Android | 5.0 (API 21) |
| iOS | 15.0 |
| macOS Catalyst | 15.0 |
| Windows | 10.0.17763.0 |

## Verify Installation

After installing, you should be able to reference the controls in your XAML:

```xml
xmlns:extras="clr-namespace:MauiControlsExtras.Controls;assembly=MauiControlsExtras"
```

If IntelliSense doesn't recognize the namespace immediately, try:
1. Rebuilding the solution
2. Closing and reopening Visual Studio
