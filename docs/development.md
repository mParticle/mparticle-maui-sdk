<!-- markdownlint-disable MD024 -->

# Getting Started

## Project Structure

| Directory           | Description                                                         |
| ------------------- | ------------------------------------------------------------------- |
| Rokt.Maui.Sdk       | Contains the bindings and wrapper APIs for the Rokt .NET MAUI SDK   |
| Rokt.Maui.SampleApp | Contains a sample application to demonstrate integration of the SDK |
| docs                | Contains the documentation for the project                          |

## Local Setup

### General

This project requires .NET 8 to run. You will also need to install the MAUI workload.  
Check out the official installation guide [here](https://learn.microsoft.com/en-us/dotnet/maui/get-started/installation?view=net-maui-8.0&tabs=visual-studio-code).  
The recommended IDE is Jetbrains Rider. You can build and run the projects from the IDE directly.

### iOS

You will also need XCode and the XCode Command Line Tools.

### Android

To create the Android bindings you will need the [Android SDK](https://developer.android.com/tools) as well as Java 17.  
You can obtain both of these via [Android Studio](https://developer.android.com/studio).  
If you have multiple versions of Java installed, you should ensure that your IDE is pointing to Java 17, or that you set the JAVA_HOME environment variable to point to your
installation of Java 17.  
If you are using Rider you should install the [Android Support Plugin](https://plugins.jetbrains.com/plugin/12056-rider-android-support), and set your Android SDK and Java version in Settings -> Build, Execution, Deployment -> Android.

## Editing the Bindings

### iOS

The iOS bindings consist of two parts: the native project and the .NET project.

#### Updating the Native Rokt iOS SDK

The bindings require the Rokt_Widget.xcframework. Replace the xcframework in the externals directory with your
desired build of the iOS native SDK.

#### Updating the Swift Bindings

The swift bindings are used to expose functionality of the native SDK and generate the interop with .NET. These bindings
are contained in the RoktBinding xcproject. Open the project in XCode and make your desired changes to DotnetRoktBinding.swift.
Make sure to build the project after to ensure your changes work.

#### Updating the .NET Bindings

The .NET bindings are manually maintained in `ApiDefinitions.cs` in the `RoktBinding.MaciOS.Binding` directory. When the Swift binding API changes, you need to manually update the corresponding C# bindings in this file. The [Verify] annotations indicate areas requiring manual review. Carefully evaluate each [Verify] annotation to ensure that the corresponding API is correctly defined. Remove the [Verify] annotation only if you are confident that no further verification is needed.

### Android

#### Updating the Native Rokt Android SDK

Update the version of the SDK in the gradle/libs.versions.toml of the android binding project. This project is located in android/native/RoktBinding.
You then need to update the .aar referenced in the root Rokt.Maui.Sdk.csproj to use the correct version.

**Caution**: if any dependencies of the native SDK have changed, the corresponding changes must be made in the root Rokt.Maui.Sdk.csproj.
You may need to introduce a new library, update a version, or link to a built .aar. Check out the .csproj for how this has been done.

#### Updating the Kotlin Bindings

The kotlin bindings are used to expose functionality of the native SDK and generate the interop with .NET. These bindings
are contained in the RoktBinding android project. Open the project in Android Studio and make your desired changes to RoktSdkBinding.kt.
Make sure to build the project after to ensure your changes work.

#### Updating the .NET Bindings

To generate the .NET bindings you just need to build the Rokt.Maui.Sdk.csproj. There is no corresponding file to update as with iOS.

## Editing the SDK API Interface

The .NET API interfaces that are exposed to consuming applications are located in Rokt.cs. It contains the interfaces for both
android and iOS. Care should be kept to ensure that the interfaces between both platforms remains the same. The code should be
restructured to ensure this is the case, but for now make sure to use conditional compilation macros to isolate code to each
platform that is not shared.

## Testing the SDK locally

### Packing the SDK

To test your changes to the SDK locally, you can use the SampleApp project. Firstly you should build the SDK, either via your IDE's build button or by running  
`dotnet build Sdk/Rokt.Maui.Sdk/Rokt.Maui.Sdk.csproj`  
from the root project directory.  
You should then pack the project via IDE or by running  
`dotnet pack Sdk/Rokt.Maui.Sdk/Rokt.Maui.Sdk.csproj`.  
This will create a .nupkg file in the SDK's bin directory.  
You can then add this directory as a NuGet source via IDE or by editing your `~/.nuget/NuGet/NuGet.Config` file.  
Finally update the sample app's .csproj to point to the version of the SDK you just created locally and run the sample app.

### Running the Sample App

The easist way to run the sample app is via Rider. Select the iOS or Android configuration in the top right, and click the play button. (Make sure the configuration is SampleApp and not Rokt.Maui.Sdk).
