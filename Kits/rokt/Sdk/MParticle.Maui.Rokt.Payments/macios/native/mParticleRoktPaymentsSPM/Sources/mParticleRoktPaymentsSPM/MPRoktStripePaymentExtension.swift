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
import UIKit

/// `@objc NSObject` wrapper around the pure-Swift
/// `RoktStripePaymentExtension`, conforming to the `@objc(RoktPaymentExtension)`
/// protocol (`PaymentExtension`).
///
/// The wrapper exists so a concrete instance can safely cross the Xamarin.iOS
/// bridge (Xamarin can only marshal `NSObject`-derived types). All protocol
/// methods are forwarded to the underlying pure-Swift implementation.
@objc(MPRoktStripePaymentExtension)
public final class MPRoktStripePaymentExtension: NSObject, PaymentExtension {

    private let inner: RoktStripePaymentExtension

    /// Creates a new Stripe payment extension.
    ///
    /// - Parameters:
    ///   - applePayMerchantId: Apple Pay merchant identifier (must be non-empty).
    ///   - countryCode: ISO 3166-1 alpha-2 country code (e.g. `"US"`).
    @objc(initWithApplePayMerchantId:countryCode:)
    public init?(applePayMerchantId: String, countryCode: String) {
        guard let inner = RoktStripePaymentExtension(
            applePayMerchantId: applePayMerchantId,
            countryCode: countryCode
        ) else {
            return nil
        }
        self.inner = inner
        super.init()
    }

    // MARK: - PaymentExtension

    public var id: String { inner.id }
    public var extensionDescription: String { inner.extensionDescription }
    public var supportedMethods: [String] { inner.supportedMethods }

    public func onRegister(parameters: [String: String]) -> Bool {
        inner.onRegister(parameters: parameters)
    }

    public func onUnregister() {
        inner.onUnregister()
    }

    public func presentPaymentSheet(
        item: PaymentItem,
        method: PaymentMethodType,
        from viewController: UIViewController,
        preparePayment: @escaping (
            _ address: ContactAddress,
            _ completion: @escaping (PaymentPreparation?, Error?) -> Void
        ) -> Void,
        completion: @escaping (PaymentSheetResult) -> Void
    ) {
        inner.presentPaymentSheet(
            item: item,
            method: method,
            from: viewController,
            preparePayment: preparePayment,
            completion: completion
        )
    }
}
