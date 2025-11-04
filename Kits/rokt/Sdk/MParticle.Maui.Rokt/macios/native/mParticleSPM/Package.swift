// swift-tools-version: 5.7
// The swift-tools-version declares the minimum version of Swift required to build this package.

import PackageDescription

let package = Package(
    name: "mParticleSPM",
    platforms: [
        .iOS(.v11)
    ],
    products: [
        // Products define the executables and libraries a package produces, making them visible to other packages.
        .library(
            name: "mParticleSPM",
            type: .static,
            targets: ["mParticleSPM"])
    ],
    dependencies: [
        // Dependencies declare other packages that this package depends on.
        .package(url: "https://github.com/mparticle-integrations/mparticle-apple-integration-rokt.git", exact: "8.3.2"),
    ],
    targets: [
        // Targets are the basic building blocks of a package, defining a module or a test suite.
        .target(
            name: "mParticleSPM",
            dependencies: [
                .product(name: "mParticle-Rokt", package: "mparticle-apple-integration-rokt")
            ]
        )
    ]
)
