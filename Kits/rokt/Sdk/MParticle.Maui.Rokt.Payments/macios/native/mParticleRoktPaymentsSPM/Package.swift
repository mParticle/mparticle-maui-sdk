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
        .package(url: "https://github.com/mparticle-integrations/mp-apple-integration-rokt.git", exact: "9.1.0"),
        .package(url: "https://github.com/ROKT/rokt-stripe-payment-extension-ios.git", exact: "2.0.1")
    ],
    targets: [
        .target(
            name: "mParticleRoktPaymentsSPM",
            dependencies: [
                .product(name: "mParticle-Rokt", package: "mp-apple-integration-rokt"),
                .product(name: "RoktStripePaymentExtension", package: "rokt-stripe-payment-extension-ios")
            ]
        )
    ]
)
