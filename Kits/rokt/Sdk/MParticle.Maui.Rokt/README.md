# Rokt .NET MAUI SDK

## Overview

The Rokt .NET MAUI SDK contains bindings for the Rokt native iOS and Android mobile SDKs.
It provides a common API interface for ease of integration with iOS and Android targets.

## Project Structure

The project utilizes [Maui Native Library Interop](https://github.com/CommunityToolkit/Maui.NativeLibraryInterop)
to create the bindings. These bindings are used by the Rokt SDK wrapper APIs to expose the native library functionality.

## Getting Started

Read the docs here: <https://docs.rokt.com/developers/integration-guides/maui/overview>

## Rokt API

After `MParticle.Instance.Initialize(options)`, the Rokt API is available via
`MParticle.Instance.Rokt`.

Available methods:

- `SelectPlacements(identifier, attributes, embeddedViews, config)`
- `Events(identifier, onEvent)` for placement-specific event subscription
- `GlobalEvents(onEvent)` for all Rokt events

> `SelectShoppableAds(...)` is not part of this package. It ships as an extension in
> `mParticle.Maui.Kits.Rokt.Payments`, which registers the native payment extension it requires.

Example placement selection:

```csharp
var attributes = new Dictionary<string, string>
{
    ["country"] = "US",
    ["email"] = "jenny.smith@example.com"
};

MParticle.Instance.Rokt.SelectPlacements(
    identifier: "StgRoktShoppableAds",
    attributes: attributes,
    embeddedViews: new Dictionary<string, RoktEmbeddedView>
    {
        ["Location1"] = myEmbeddedView
    },
    config: null
);
```

Example event subscriptions:

```csharp
MParticle.Instance.Rokt.Events("StgRoktShoppableAds", roktEvent =>
{
    Console.WriteLine($"Rokt event: {roktEvent.GetType().Name}");
});

MParticle.Instance.Rokt.GlobalEvents(roktEvent =>
{
    Console.WriteLine($"Global Rokt event: {roktEvent.GetType().Name}");
});
```

## Supported Targets

- .NET iOS
- .NET Android
