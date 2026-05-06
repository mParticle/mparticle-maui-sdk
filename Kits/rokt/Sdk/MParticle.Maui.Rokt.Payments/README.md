# Rokt Payments .NET MAUI Kit

## Overview

Optional payment extension for the Rokt MAUI kit. Enables Apple Pay / Stripe
flows inside Rokt Shoppable Ads. Depends on `mParticle.Maui.Kits.Rokt`.

> `mParticle.Maui.Kits.Rokt.Payments` already includes `mParticle.Maui.Kits.Rokt`
> as a dependency. You do not need to add the Rokt kit package separately.

## Getting Started

```csharp
using mParticle.MAUI;
using mParticle.MAUI.Rokt.Payments;

// Call once after MParticle.Instance.Initialize(options)
RoktPaymentExtension.Register("merchant.com.yourapp.rokt");
```

## Rokt API (included via dependency)

`mParticle.Maui.Kits.Rokt.Payments` exposes the core Rokt API through
`MParticle.Instance.Rokt`, including:

- `SelectPlacements(identifier, attributes, embeddedViews, config)`
- `SelectShoppableAds(identifier, attributes, config)`
- `Events(identifier, onEvent)` for placement-specific event subscription
- `GlobalEvents(onEvent)` for all Rokt events

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

Example shoppable ads selection:

```csharp
MParticle.Instance.Rokt.SelectShoppableAds(
    identifier: "StgRoktShoppableAds",
    attributes: attributes,
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

- .NET iOS (Apple Pay via Stripe)
- .NET Android (no-op)
