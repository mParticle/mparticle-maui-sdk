#!/bin/bash
set -e

# Configuration
AAR_VERSION="5.70.2"
AAR_DOWNLOAD_FILENAME="android-core-${AAR_VERSION}.aar"
AAR_URL="https://repo1.maven.org/maven2/com/mparticle/android-core/${AAR_VERSION}/${AAR_DOWNLOAD_FILENAME}"
AAR_DIR="Bindings/mParticle.MAUI.AndroidBinding/Jars"
AAR_PATH="${AAR_DIR}/android-core.aar"

# Clean old AARs from Android binding Jars directory
echo "Cleaning old AAR files..."
rm -f ${AAR_DIR}/*.aar

# Create Jars directory if it doesn't exist
mkdir -p ${AAR_DIR}

# Download mParticle Android Core AAR
echo "Downloading ${AAR_DOWNLOAD_FILENAME}..."
echo "URL: ${AAR_URL}"
echo "Destination: ${AAR_PATH}"

if ! curl -L --fail -o "${AAR_PATH}" "${AAR_URL}"; then
    echo "Error: Failed to download AAR file"
    exit 1
fi

if [ ! -f "${AAR_PATH}" ]; then
    echo "Error: AAR file not found at ${AAR_PATH} after download"
    exit 1
fi

# .NET MAUI
# 

# Build xcframework first (required for iOS binding)
echo "Building xcframework..."
chmod +x Bindings/build_xcframework_spm.sh
./Bindings/build_xcframework_spm.sh

# Restore packages
dotnet restore

# Build bindings
dotnet build Bindings/mParticle.MAUI.AndroidBinding/mParticle.MAUI.AndroidBinding.csproj /p:Configuration=Release /t:Rebuild
dotnet build Bindings/mParticle.MAUI.iOSBinding/mParticle.MAUI.iOSBinding.csproj /p:Configuration=Release /t:Rebuild

# Build Libraries
dotnet build Library/mParticle.MAUI.Abstractions/mParticle.MAUI.Abstractions.csproj /p:Configuration=Release /t:Rebuild
dotnet build Library/mParticle.MAUI.Android/mParticle.MAUI.Android.csproj /p:Configuration=Release /t:Rebuild
dotnet build Library/mParticle.MAUI.iOS/mParticle.MAUI.iOS.csproj /p:Configuration=Release /t:Rebuild

# Build Sample Apps
dotnet build Samples/mParticle.MAUI.Android.Sample/mParticle.MAUI.Android.Sample.csproj /p:Configuration=Debug /t:Rebuild
dotnet build Samples/mParticle.MAUI.iOS.Sample/mParticle.MAUI.iOS.Sample.csproj /p:Configuration=Debug /t:Rebuild


# Package for nuget
#

nuget pack mparticle.nuspec
