// swift-tools-version: 5.9
// The swift-tools-version declares the minimum version of Swift required to build this package.

import PackageDescription

let package = Package(
    name: "mParticleSPMWrapper",
    platforms: [
        .iOS(.v12)
    ],
    products: [
        // Products define the executables and libraries a package produces, making them visible to other packages.
        .library(
            name: "mParticleSPMWrapper",
            type: .static,
            targets: ["mParticleSPMWrapper"]),
    ],
    dependencies: [
        // Dependencies declare other packages that this package depends on.
        .package(url: "https://github.com/mParticle/mparticle-apple-sdk", exact: "8.35.0")
    ],
    targets: [
        // Targets are the basic building blocks of a package, defining a module or a test suite.
        // Targets can depend on other targets in this package and products from dependencies.
        .target(
            name: "mParticleSPMWrapper",
            dependencies: [
                .product(name: "mParticle-Apple-SDK", package: "mparticle-apple-sdk")
            ],
            path: "mParticleSPMWrapper",
            sources: ["mParticleSPMWrapper.swift"]
        )
    ]
)
