#!/bin/sh
set -e

# Clean build directory
rm -rf Bindings/archives
rm -rf Bindings/mParticleSPMWrapper.xcframework

mkdir -p Bindings/archives

# Build iOS archive
xcodebuild archive \
  -scheme mParticleSPMWrapper \
  -project Bindings/mParticleSPMWrapper/mParticleSPMWrapper.xcodeproj \
  -destination "generic/platform=iOS" \
  -archivePath "Bindings/archives/mParticleSPMWrapper-ios" \
  SKIP_INSTALL=NO \
  BUILD_LIBRARY_FOR_DISTRIBUTION=YES

# Build iOS simulator arm64 archive
xcodebuild archive \
  -scheme mParticleSPMWrapper \
  -project Bindings/mParticleSPMWrapper/mParticleSPMWrapper.xcodeproj \
  -destination "generic/platform=iOS Simulator" \
  -archivePath "Bindings/archives/mParticleSPMWrapper-sim-arm64" \
  SKIP_INSTALL=NO \
  BUILD_LIBRARY_FOR_DISTRIBUTION=YES \
  ARCHS=arm64

# Build iOS simulator x86_64 archive
xcodebuild archive \
  -scheme mParticleSPMWrapper \
  -project Bindings/mParticleSPMWrapper/mParticleSPMWrapper.xcodeproj \
  -destination "generic/platform=iOS Simulator" \
  -archivePath "Bindings/archives/mParticleSPMWrapper-sim-x86_64" \
  SKIP_INSTALL=NO \
  BUILD_LIBRARY_FOR_DISTRIBUTION=YES \
  ARCHS=x86_64

# Create fat simulator framework
# Copy the framework structure from one of the simulator archives
cp -R "Bindings/archives/mParticleSPMWrapper-sim-arm64.xcarchive/Products/Library/Frameworks/mParticleSPMWrapper.framework" "Bindings/archives/mParticleSPMWrapper.framework"

# Combine the simulator binaries into a single fat binary
lipo -create \
  "Bindings/archives/mParticleSPMWrapper-sim-arm64.xcarchive/Products/Library/Frameworks/mParticleSPMWrapper.framework/mParticleSPMWrapper" \
  "Bindings/archives/mParticleSPMWrapper-sim-x86_64.xcarchive/Products/Library/Frameworks/mParticleSPMWrapper.framework/mParticleSPMWrapper" \
  -output "Bindings/archives/mParticleSPMWrapper.framework/mParticleSPMWrapper"

# Create xcframework
xcodebuild -create-xcframework \
    -framework "Bindings/archives/mParticleSPMWrapper.framework" \
    -framework "Bindings/archives/mParticleSPMWrapper-ios.xcarchive/Products/Library/Frameworks/mParticleSPMWrapper.framework" \
    -output "Bindings/mParticleSPMWrapper.xcframework"

echo "Successfully created mParticleSPMWrapper.xcframework" 