//
//  MPRoktStripePaymentExtension.swift
//  mParticleRoktPaymentsSPM
//
//  Copyright 2024 Rokt Pte Ltd
//
//  Licensed under the Rokt Software Development Kit (SDK) Terms of Use
//  Version 2.0 (the "License");
//

import Foundation
import RoktContracts
import RoktStripePaymentExtension

/// Objective-C shim exposing a factory for `RoktStripePaymentExtension`
/// so the Xamarin.iOS binding generator (which only sees ObjC) can
/// instantiate the pure-Swift extension and hand it back to C#.
///
/// The returned object conforms to the `RoktPaymentExtension` protocol
/// and is meant to be passed to `-[MPRokt registerPaymentExtension:]`.
@objc(MPRoktStripePaymentExtension)
public final class MPRoktStripePaymentExtension: NSObject {

    /// Factory method. Returns the payment extension as an opaque object
    /// so C# can forward it to `MPRokt.registerPaymentExtension:` without
    /// knowing about the Swift-only `RoktPaymentExtension` protocol.
    ///
    /// - Parameters:
    ///   - applePayMerchantId: Apple Pay merchant identifier
    ///     (e.g. `merchant.com.yourapp.rokt`).
    ///   - countryCode: ISO country code, defaults to `"US"`.
    /// - Returns: The payment extension instance, or `nil` if initialization fails.
    @objc(createWithApplePayMerchantId:countryCode:)
    public static func create(applePayMerchantId: String,
                              countryCode: String) -> AnyObject? {
        return RoktStripePaymentExtension(applePayMerchantId: applePayMerchantId,
                                          countryCode: countryCode)
    }
}
