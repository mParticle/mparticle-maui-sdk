<!-- markdownlint-disable MD024 -->

# Migration Guides

This document describes upgrade steps for breaking changes in the mParticle MAUI SDK. It only covers changes that require action on the MAUI side (csproj settings, public C# API, or the iOS binding surface consumed by your host app).

For changes in the underlying native iOS SDK (database migration, deprecated `UIApplicationDelegate` methods, removed `AppDelegateProxy`, regional routing / ATS, Rokt Swift/Objective-C type renames, etc.), refer to the [mParticle Apple SDK 9 migration guide](https://github.com/mParticle/mparticle-apple-sdk/blob/main/MIGRATING.md#migrating-from-versions--900).

## Unreleased migration notes

### MAUI payments helper rename

The public MAUI payments helper was renamed:

| Before                                     | After                                |
| ------------------------------------------ | ------------------------------------ |
| `RoktStripePaymentExtension.Register(...)` | `RoktPaymentExtension.Register(...)` |

If you register the iOS payment extension from MAUI code, update your call sites to the new type name.

### Android CNAME support

`MParticleOptions.NetworkOptions.CustomBaseUrl` is now honored on Android as well as iOS. No source changes are required if you already set this option.

## Migrating from versions < 5.0.0

Version 5.0.0 wraps the mParticle Apple SDK 9 on iOS and the matching `mp-apple-integration-rokt` 9.0.0 Rokt integration. Android behavior is unchanged. No C# source changes are required for most apps, but the iOS build configuration and one MAUI option (`LocationTracking`) behave differently.

### iOS deployment target raised to 15

The binding now requires iOS 15+. Update the following in your MAUI app:

- In your `.csproj`, raise `SupportedOSPlatformVersion` for iOS:

  ```xml
  <SupportedOSPlatformVersion Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'ios'">15.0</SupportedOSPlatformVersion>
  ```

- If your app's `Info.plist` sets `MinimumOSVersion`, raise it to `15.0`.

### iOS native dependencies updated

The iOS SPM manifests that ship with the binding now pull:

| Package                          | Before (4.x)                             | After (5.0.0)                     |
| -------------------------------- | ---------------------------------------- | --------------------------------- |
| mParticle Apple SDK              | `mparticle-apple-sdk` 8.40.0             | `mparticle-apple-sdk` 9.0.0       |
| Rokt integration (Rokt kit only) | `mparticle-apple-integration-rokt` 8.3.2 | `mp-apple-integration-rokt` 9.0.0 |

Two things to note:

1. **The Rokt integration repository was renamed** from `mparticle-apple-integration-rokt` to `mp-apple-integration-rokt`. If your iOS project pins this package directly (outside of the MAUI binding), update the URL.
2. The iOS binding is now built as a **static** SPM product, and `mParticle_Apple_SDK.xcframework` / `Rokt_Widget.xcframework` are no longer copied as separate resources — their symbols are statically linked into `mParticleBindingiOS.xcframework` and `mParticleRoktBindingiOS.xcframework` respectively. If you had custom MSBuild targets that referenced those side-by-side xcframeworks, remove them.

### `MParticleOptions.LocationTracking` is now Android-only

The `LocationTracking` option on `MParticleOptions` still exists for API compatibility, but on iOS it is now a **no-op** — the underlying `beginLocationTracking:` / `endLocationTracking` selectors were removed in mParticle Apple SDK 9.

Before (4.x) — worked on both platforms:

```csharp
var options = new MParticleOptions
{
    LocationTracking = new LocationTracking("GPS", 100, 350, 22),
    // ...
};
```

After (5.0.0) — same code compiles and runs, but `LocationTracking` is only honored on Android. No action is required unless your app specifically relies on iOS location tracking via this API; in that case, opt into Core Location directly from your iOS host and forward any relevant data as user attributes.

### iOS binding renamed: `MPRoktEmbeddedView` → `RoktEmbeddedView`

This only matters if you referenced `Bindings.iOS` types directly in your C# code (most apps don't — they go through `MParticle.Maui.Sdk.RoktEmbeddedView`).

| Before (4.x)                                               | After (5.0.0)                       |
| ---------------------------------------------------------- | ----------------------------------- |
| `iOSBinding.MPRoktEmbeddedView`                            | `iOSBinding.RoktEmbeddedView`       |
| `iOSBinding.MPRoktEmbeddedView.CreateMPRoktEmbeddedView()` | `new iOSBinding.RoktEmbeddedView()` |

`MParticle.Maui.Sdk.RoktEmbeddedView` (the cross-platform MAUI view) is unchanged — keep using it directly in XAML / code as before.

### iOS `MPProduct.UnitPrice` binding removed

Again, only relevant if you used `Bindings.iOS.MPProduct` directly.

- `iOSBinding.MPProduct.UnitPrice` was removed. Use `iOSBinding.MPProduct.Price` (an `NSNumber`) instead.
- The cross-platform `MParticle.Maui.Sdk.Product.UnitPrice` is unchanged.
