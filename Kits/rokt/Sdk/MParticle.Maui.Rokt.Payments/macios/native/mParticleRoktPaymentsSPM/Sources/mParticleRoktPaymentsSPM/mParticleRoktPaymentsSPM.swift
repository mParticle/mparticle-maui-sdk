//
//  mParticleRoktPaymentsSPM.swift
//  mParticleRoktPaymentsSPM
//
//  Copyright 2024 Rokt Pte Ltd
//
//  Licensed under the Rokt Software Development Kit (SDK) Terms of Use
//  Version 2.0 (the "License");
//
//  You may not use this file except in compliance with the License.
//
//  You may obtain a copy of the License at https://rokt.com/sdk-license-2-0/

import Foundation
@_exported import mParticle_Apple_SDK
@_exported import mParticle_Rokt_Swift
@_exported import RoktContracts
@_exported import RoktStripePaymentExtension

/// SPM wrapper for the Rokt Payments kit.
///
/// Re-exports the full native stack needed at runtime so that a single
/// xcframework produced from this package contains every symbol the
/// managed Xamarin.iOS bindings reference:
///
/// - `mParticle-Apple-SDK` — core mParticle SDK (MParticle, MPEvent,
///   MPKitExecStatus, MPForwardRecord, ...).
/// - `mParticle-Rokt` — Rokt kit implementation (MPKitRokt, MPRokt).
/// - `RoktContracts` — shared Rokt payment/DCUI contracts.
/// - `RoktStripePaymentExtension` — Stripe / Apple Pay payment extension.
///
/// Bundling everything under one umbrella lets the consuming application
/// ship a single xcframework (while still letting NuGet resolve the
/// `MParticle.Maui` and `MParticle.Maui.Kits.Rokt` C# bindings), avoiding
/// duplicate Objective-C class definitions at runtime.
public enum MParticleRoktPaymentsSPM {
    // Umbrella module — intentionally empty.
}
