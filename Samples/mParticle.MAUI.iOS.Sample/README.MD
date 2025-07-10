# mParticle MAUI iOS Sample

This is a sample application demonstrating the use of the mParticle MAUI SDK for iOS.

## Prerequisites

1.  Make sure you have the .NET MAUI workload installed:

```bash
dotnet workload install maui
```

2.  Open `SampleCalls.cs` and replace the placeholder API key and secret with your mParticle credentials.

## Running the Sample

1.  Build and run the application from the root of the `mparticle-maui-sdk` repository on an iOS simulator:

```bash
dotnet build Samples/mParticle.MAUI.iOS.Sample/mParticle.MAUI.iOS.Sample.csproj -t:Run -f net8.0-ios18.0
```

To target a specific simulator, find its UDID by running `xcrun simctl list devices available` and use the `-p:_DeviceName` property. For example:
```bash
dotnet build Samples/mParticle.MAUI.iOS.Sample/mParticle.MAUI.iOS.Sample.csproj -t:Run -f net8.0-ios18.0 -p:_DeviceName=:v2:udid=1670E7E9-94C6-4508-8C51-CF765B1AA68D
```
