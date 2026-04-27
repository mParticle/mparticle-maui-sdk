# Rokt Payments .NET MAUI Kit

## Overview

Optional payment extension for the Rokt MAUI kit. Enables Apple Pay / Stripe
flows inside Rokt Shoppable Ads. Depends on `mParticle.Maui.Kits.Rokt`.

## Getting Started

```csharp
using mParticle.MAUI.Rokt.Payments;

// Call once after MParticle.Instance.Initialize(options)
RoktStripePaymentExtension.Register("merchant.com.yourapp.rokt");
```

## Supported Targets

- .NET iOS (Apple Pay via Stripe)
- .NET Android (no-op)
