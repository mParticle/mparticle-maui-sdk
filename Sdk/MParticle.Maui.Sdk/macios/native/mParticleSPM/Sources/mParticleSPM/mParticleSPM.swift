//
//  mParticleSPM.swift
//  mParticleSPM
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
import UIKit
@_exported import mParticle_Apple_SDK

/// SPM wrapper for mParticle_Apple_SDK dependency
/// This module provides a clean interface to the Rokt iOS SDK
/// and allows for easy version management through Swift Package Manager
public struct MParticleSPM {
    // This is a wrapper module that re-exports mParticle_Apple_SDK
    // All functionality is available through the imported mParticle_Apple_SDK module
}

/// Extension to RoktEmbeddedView to provide factory method for MAUI SDK
public extension RoktEmbeddedView {
    /// Factory method to create a new RoktEmbeddedView instance.
    /// This is the recommended way to create embedded views for use with Rokt placements in MAUI.
    ///
    /// - Returns: A new instance of RoktEmbeddedView configured for use with Rokt
    @objc static func createRoktEmbeddedView() -> RoktEmbeddedView {

        let embeddedView = RoktEmbeddedView(frame: .zero)
        embeddedView.translatesAutoresizingMaskIntoConstraints = false

        return embeddedView
    }
}
