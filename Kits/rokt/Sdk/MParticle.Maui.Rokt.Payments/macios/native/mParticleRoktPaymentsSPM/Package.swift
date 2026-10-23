// swift-tools-version: 5.7
// The swift-tools-version declares the minimum version of Swift required to build this package.

import PackageDescription

let package = Package(
    name: "mParticleRoktPaymentsSPM",
    platforms: [
        .iOS(.v15)
    ],
    products: [
        .library(
            name: "mParticleRoktPaymentsSPM",
            type: .static,
            targets: ["mParticleRoktPaymentsSPM"])
    ],
    dependencies: [
        .package(url: "https://github.com/mparticle-integrations/mp-apple-integration-rokt.git", exact: "9.7.3"),
        // Keep the kit's independently resolved core aligned with the MAUI SDK.
        .package(url: "https://github.com/mParticle/mparticle-apple-sdk", exact: "9.7.3"),
        .package(url: "https://github.com/ROKT/rokt-stripe-payment-extension-ios.git", exact: "2.0.3")
    ],
    targets: [
        .target(
            name: "mParticleRoktPaymentsSPM",
            dependencies: [
                .product(name: "mParticle-Rokt", package: "mp-apple-integration-rokt"),
                .product(name: "mParticle-Apple-SDK", package: "mparticle-apple-sdk"),
                .product(name: "RoktPaymentExtension", package: "rokt-stripe-payment-extension-ios")
            ]
        )
    ]
)
