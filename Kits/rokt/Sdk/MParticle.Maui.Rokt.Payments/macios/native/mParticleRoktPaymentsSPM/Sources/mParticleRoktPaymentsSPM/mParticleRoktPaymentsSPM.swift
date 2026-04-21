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
@_exported import RoktContracts
@_exported import RoktStripePaymentExtension

/// SPM wrapper for the Rokt payment extension dependencies.
/// Re-exports RoktContracts and RoktStripePaymentExtension so that the
/// Xcode binding project can consume them through a single module.
public struct MParticleRoktPaymentsSPM {
    // Umbrella module — intentionally empty.
}
