#!/bin/sh
set -e

# Clean build directory
rm -rf Bindings/archives
rm -rf Bindings/mParticleSPMWrapper.xcframework

mkdir -p Bindings/archives

# Set custom derived data path for predictable framework location
DERIVED_DATA_PATH="$(pwd)/Bindings/archives/DerivedData"
rm -rf "$DERIVED_DATA_PATH"
mkdir -p "$DERIVED_DATA_PATH"

# Navigate to SPM package directory
cd Bindings/mParticleSPMWrapper

# Build iOS archive using SPM but with xcodebuild for framework creation
# This approach uses Package.swift for dependency resolution but xcodebuild for framework building
xcodebuild archive \
  -scheme mParticleSPMWrapper \
  -destination "generic/platform=iOS" \
  -archivePath "../archives/mParticleSPMWrapper-ios" \
  -derivedDataPath "$DERIVED_DATA_PATH" \
  SKIP_INSTALL=NO \
  BUILD_LIBRARY_FOR_DISTRIBUTION=YES \
  ONLY_ACTIVE_ARCH=NO

# Build iOS simulator archive
xcodebuild archive \
  -scheme mParticleSPMWrapper \
  -destination "generic/platform=iOS Simulator" \
  -archivePath "../archives/mParticleSPMWrapper-ios-simulator" \
  -derivedDataPath "$DERIVED_DATA_PATH" \
  SKIP_INSTALL=NO \
  BUILD_LIBRARY_FOR_DISTRIBUTION=YES \
  ONLY_ACTIVE_ARCH=NO

# Go back to root
cd ../..

echo "Using archived frameworks with SPM dependencies..."

# Use the archived frameworks directly (SPM dependencies should be included in archive)
iOS_FRAMEWORK_PATH="Bindings/archives/mParticleSPMWrapper-ios.xcarchive/Products/Library/Frameworks/mParticleSPMWrapper.framework"
SIMULATOR_FRAMEWORK_PATH="Bindings/archives/mParticleSPMWrapper-ios-simulator.xcarchive/Products/Library/Frameworks/mParticleSPMWrapper.framework"

# Verify frameworks exist
if [ ! -d "$iOS_FRAMEWORK_PATH" ]; then
    echo "Error: iOS framework not found at $iOS_FRAMEWORK_PATH"
    exit 1
fi

if [ ! -d "$SIMULATOR_FRAMEWORK_PATH" ]; then
    echo "Error: Simulator framework not found at $SIMULATOR_FRAMEWORK_PATH"
    exit 1
fi

echo "✅ Both frameworks found successfully"

# Find and bundle the mParticle Apple SDK dependency
echo "Bundling mParticle Apple SDK dependency..."

# Look for mParticle_Apple_SDK.framework in our custom derived data build products
MP_SDK_FRAMEWORK=$(find "$DERIVED_DATA_PATH/Build/Intermediates.noindex/ArchiveIntermediates/mParticleSPMWrapper/BuildProductsPath" -name "mParticle_Apple_SDK.framework" -type d 2>/dev/null | head -1)

# Fallback: search entire custom derived data directory
if [ -z "$MP_SDK_FRAMEWORK" ]; then
    echo "Framework not found in BuildProductsPath, searching entire derived data..."
    MP_SDK_FRAMEWORK=$(find "$DERIVED_DATA_PATH" -name "mParticle_Apple_SDK.framework" -type d 2>/dev/null | head -1)
fi

if [ -n "$MP_SDK_FRAMEWORK" ]; then
    echo "Found mParticle Apple SDK at: $MP_SDK_FRAMEWORK"
    
    # Copy the dependency into each platform framework
    echo "Bundling dependency into iOS framework..."
    iOS_FRAMEWORKS_DIR="${iOS_FRAMEWORK_PATH}/Frameworks"
    mkdir -p "$iOS_FRAMEWORKS_DIR"
    cp -R "$MP_SDK_FRAMEWORK" "$iOS_FRAMEWORKS_DIR/"
    
    echo "Bundling dependency into Simulator framework..."
    SIM_FRAMEWORKS_DIR="${SIMULATOR_FRAMEWORK_PATH}/Frameworks"
    mkdir -p "$SIM_FRAMEWORKS_DIR"
    cp -R "$MP_SDK_FRAMEWORK" "$SIM_FRAMEWORKS_DIR/"
    
    # Update the install_name to point to the correct nested location
    install_name_tool -id "@rpath/mParticleSPMWrapper.framework/Frameworks/mParticle_Apple_SDK.framework/mParticle_Apple_SDK" \
        "$iOS_FRAMEWORKS_DIR/mParticle_Apple_SDK.framework/mParticle_Apple_SDK"
    
    install_name_tool -id "@rpath/mParticleSPMWrapper.framework/Frameworks/mParticle_Apple_SDK.framework/mParticle_Apple_SDK" \
        "$SIM_FRAMEWORKS_DIR/mParticle_Apple_SDK.framework/mParticle_Apple_SDK"
    
    # Update the reference in our wrapper framework to point to the nested dependency
    install_name_tool -change "@rpath/mParticle_Apple_SDK.framework/mParticle_Apple_SDK" \
        "@rpath/mParticleSPMWrapper.framework/Frameworks/mParticle_Apple_SDK.framework/mParticle_Apple_SDK" \
        "${iOS_FRAMEWORK_PATH}/mParticleSPMWrapper"
    
    install_name_tool -change "@rpath/mParticle_Apple_SDK.framework/mParticle_Apple_SDK" \
        "@rpath/mParticleSPMWrapper.framework/Frameworks/mParticle_Apple_SDK.framework/mParticle_Apple_SDK" \
        "${SIMULATOR_FRAMEWORK_PATH}/mParticleSPMWrapper"
    
    # Re-sign the frameworks after modifying them (install_name_tool invalidates code signatures)
    echo "Re-signing frameworks after install_name_tool modifications..."
    
    # Re-sign the bundled mParticle Apple SDK frameworks
    codesign --force --sign - "$iOS_FRAMEWORKS_DIR/mParticle_Apple_SDK.framework/mParticle_Apple_SDK"
    codesign --force --sign - "$SIM_FRAMEWORKS_DIR/mParticle_Apple_SDK.framework/mParticle_Apple_SDK"
    
    # Re-sign our wrapper frameworks
    codesign --force --sign - "${iOS_FRAMEWORK_PATH}/mParticleSPMWrapper"
    codesign --force --sign - "${SIMULATOR_FRAMEWORK_PATH}/mParticleSPMWrapper"
    
    echo "✅ Successfully bundled mParticle Apple SDK dependency with updated install names and re-signed frameworks"
else
    echo "⚠️  Warning: mParticle Apple SDK framework not found in build products"
    echo "Continuing without dependency bundling..."
    exit 1
fi

# Create xcframework 
xcodebuild -create-xcframework \
    -framework "$iOS_FRAMEWORK_PATH" \
    -framework "$SIMULATOR_FRAMEWORK_PATH" \
    -output "Bindings/mParticleSPMWrapper.xcframework"

echo "Successfully created mParticleSPMWrapper.xcframework using SPM" 