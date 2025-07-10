# mParticle MAUI Android Sample

This is a sample application demonstrating the use of the mParticle MAUI SDK for Android.

## Prerequisites

1.  Make sure you have the .NET MAUI workload installed:

```bash
dotnet workload install maui
```

2.  Open `SampleCalls.cs` and replace the placeholder API key and secret with your mParticle credentials. As a more secure alternative, you can set the `MPARTICLE_ANDROID_KEY` and `MPARTICLE_ANDROID_SECRET` environment variables.

To set the environment variables in your shell for the current session, use:

```bash
export MPARTICLE_ANDROID_KEY="your-android-app-key"
export MPARTICLE_ANDROID_SECRET="your-android-app-secret"
```

The application will prioritize these environment variables over the hardcoded values.

## Running the Sample

1.  Make sure you have an Android emulator running or a device connected.

2.  Build and run the application from the root of the `mparticle-maui-sdk` repository:

```bash
dotnet build Samples/mParticle.MAUI.Android.Sample/mParticle.MAUI.Android.Sample.csproj -t:Run -f net8.0-android
```
