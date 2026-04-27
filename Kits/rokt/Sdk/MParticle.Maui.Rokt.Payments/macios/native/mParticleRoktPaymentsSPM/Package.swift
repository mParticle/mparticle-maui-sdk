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
        .package(url: "https://github.com/mParticle/mparticle-apple-sdk", exact: "9.0.0"),
        .package(url: "https://github.com/mparticle-integrations/mp-apple-integration-rokt.git", exact: "9.0.0"),
        .package(url: "https://github.com/ROKT/rokt-contracts-apple.git", .upToNextMinor(from: "0.1.3")),
        .package(url: "https://github.com/ROKT/rokt-stripe-payment-extension-ios.git", exact: "0.1.2")
    ],
    targets: [
        .target(
            name: "mParticleRoktPaymentsSPM",
            dependencies: [
                .product(name: "mParticle-Apple-SDK", package: "mparticle-apple-sdk"),
                .product(name: "mParticle-Rokt", package: "mp-apple-integration-rokt"),
                .product(name: "RoktContracts", package: "rokt-contracts-apple"),
                .product(name: "RoktStripePaymentExtension", package: "rokt-stripe-payment-extension-ios")
            ]
        )
    ]
)
