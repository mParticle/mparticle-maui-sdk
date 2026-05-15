<!-- markdownlint-disable MD024 -->

# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

This release upgrades the iOS binding to mParticle Apple SDK 9 and contains breaking changes. See [MIGRATING.md](./MIGRATING.md) for the full 4.x → 5.0 upgrade guide.

### Added

- iOS-only `RoktApi.SelectShoppableAds` C# API, wrapping the `selectShoppableAds:attributes:config:onEvent:` selector from mParticle Apple SDK 9. Android exposes a no-op bridge for API compatibility until native support lands, matching the Flutter and React Native SDKs. Requires a payment extension to be registered on the native iOS side.
- `Show Rokt Shoppable Ads (iOS)` demo button in the Rokt sample app.
- iOS CNAME routing support via `MParticleOptions.NetworkOptions.CustomBaseUrl`, mapped to Apple SDK `MPNetworkOptions.customBaseURL`.
- Android CNAME routing support via `MParticleOptions.NetworkOptions.CustomBaseUrl`, mapped to Android `com.mparticle.networking.NetworkOptions`.

### Changed

- **BREAKING**: iOS minimum deployment target raised from `11.0` to `15.0`.
- **BREAKING**: Updated iOS SPM dependency from `mparticle-apple-sdk` 8.40.0 to 9.0.0 in both the core SDK and Rokt kit.
- **BREAKING**: Rokt kit now depends on the renamed `mp-apple-integration-rokt` 9.0.0 package (was `mparticle-apple-integration-rokt` 8.3.2).
- Update iOS SPM dependency versions to Apple SDK `9.2.0` and Rokt integration `9.2.0`.
- **BREAKING**: `MParticleOptions.LocationTracking` is now Android-only; it is silently ignored on iOS because Apple SDK 9 removed `beginLocationTracking:` / `endLocationTracking`.
- **BREAKING** (iOS binding surface): Renamed `iOSBinding.MPRoktEmbeddedView` to `iOSBinding.RoktEmbeddedView`; the factory `CreateMPRoktEmbeddedView()` was removed in favor of direct construction.
- **BREAKING** (iOS binding surface): Removed `iOSBinding.MPProduct.UnitPrice`; use `iOSBinding.MPProduct.Price` (`NSNumber`) instead. The cross-platform `MParticle.Maui.Sdk.Product.UnitPrice` is unchanged.
- **BREAKING**: Renamed MAUI payments helper from `RoktStripePaymentExtension` to `RoktPaymentExtension`.
- iOS binding is now built as a static SPM product; `mParticle_Apple_SDK.xcframework` and `Rokt_Widget.xcframework` are no longer copied as separate resources.
- Updated Android dependencies to `android-core`/`android-rokt-kit`/`android-kit-base` `5.79.0` and aligned Rokt transitive AAR versions (`4.14.3` / `0.9.3`).

### Fixed

- iOS `RoktPlacementClosed` now forwards the placement identifier to the `RoktEventCallback.OnUnLoad` handler instead of a hardcoded `"Unknown"`, matching the pattern used for `RoktEmbeddedSizeChanged` and the behavior of the sibling Flutter and React Native SDKs.

## [4.1.1] - 2026-03-20

### Fixed

- Missing mParticleRoktBindingiOS.xcframework in the NuGet package
- Issue with ProxyAppDelegate by disabling ProxyAppDelegate in iOS binding (NSProxy incompatible with MAUI)

## [4.1.0] - 2026-02-16

### Changed

- Upgraded target frameworks from .NET 8.0 to .NET 10.0 MAUI for all SDK and sample projects
- Updated Android dependencies

### Fixed

- Resolved XA4212 build error with AndroidX.Navigation.Compose on .NET 10 MAUI

## [4.0.1] - 2025-11-18

### Fixed

- Corrected filename for Rokt kit CSPROJ file from `MParticle.Maui.Kits.Rokt` to `mParticle.Maui.Kits.Rokt`

## [4.0.0] - 2025-11-11

### Changed

- Restructured project to not hardcode XCFrameworks and AAR files

## [3.0.0] - 2025-01-31

### Changed

- First release of the new MAUI only package: mParticle.MAUI

[unreleased]: https://github.com/mParticle/mparticle-maui-sdk/compare/4.1.1...HEAD
[4.1.1]: https://github.com/mParticle/mparticle-maui-sdk/compare/4.1.0...4.1.1
[4.1.0]: https://github.com/mParticle/mparticle-maui-sdk/compare/4.0.1...4.1.0
[4.0.1]: https://github.com/mParticle/mparticle-maui-sdk/compare/4.0.0...4.0.1
[4.0.0]: https://github.com/mParticle/mparticle-maui-sdk/compare/3.0.0...4.0.0
[3.0.0]: https://github.com/mParticle/mparticle-maui-sdk/compare/08b6a85a91b4f1cf98607e3eb91ccb4b9eea6548...3.0.0
