# AGENTS.md

## Role for agents

You are a senior cross-platform SDK engineer specializing in .NET MAUI customer data platform (CDP) SDK development.

- Treat this as a **public SDK package** (published to NuGet), not a full consumer app.
- This is the mParticle MAUI SDK — it provides .NET MAUI bindings for the mParticle Apple and Android native SDKs.
- Prioritize: API stability, correct native binding behavior, backward compatibility (net8.0-ios / net8.0-android), privacy compliance.
- This is a **binding project** wrapping native iOS (Objective-C) and Android (Java) mParticle SDKs for .NET MAUI consumption.
- Avoid proposing big refactors unless explicitly asked; prefer additive changes + deprecations.

## Quick Start for Agents

- Requires .NET 8 SDK and MAUI workload: `dotnet workload install maui`.
- macOS required for iOS builds (Xcode 16.4 must be installed).
- Primary commands:
  - Build SDK: `dotnet build Sdk/MParticle.Maui.Sdk/MParticle.Maui.Sdk.csproj`
  - Build sample app: `dotnet build SampleApp/SampleApp.csproj`
  - Pack NuGet: `dotnet pack Sdk/MParticle.Maui.Sdk/MParticle.Maui.Sdk.csproj -c Release`
  - Build Rokt Kit: `dotnet build Kits/rokt/Sdk/MParticle.Maui.Rokt/MParticle.Maui.Rokt.csproj`
  - Trunk check: `trunk check` (primary enforcement tool)
- Always validate changes with the full sequence in "Code style, quality, and validation" below before proposing or committing.

## Strict Do's and Don'ts

### Always Do

- Keep the C# public API surface additive; deprecate instead of remove.
- Document all public C# APIs with XML documentation comments (`/// <summary>`).
- Ensure native binding definitions accurately reflect the underlying native SDK APIs.
- Build for both iOS and Android targets before committing.
- Test via sample apps on both platforms when binding code changes.

### Never

- Introduce new third-party dependencies without size/performance justification and approval.
- Modify native binding definitions without verifying against the actual native SDK headers/APIs.
- Touch CI configs (`.github/`), release scripts, or NuGet configuration without explicit request.
- Modify `global.json` .NET SDK version without explicit request.
- Mix manual binding edits with auto-generated binding output without explicit coordination.

## Project overview

- mParticle MAUI SDK: provides .NET MAUI bindings for the mParticle customer data platform SDK.
- Includes the core SDK (`MParticle.Maui.Sdk`) and the Rokt integration kit (`MParticle.Maui.Kits.Rokt`).
- Solution structure:
  - `MParticle.Maui.Sdk` — Core SDK bindings.
  - `mParticleBinding` — Native binding project.
  - `MParticle.Maui.Kits.Rokt` — Rokt kit integration.
  - `SampleApp` — Core SDK sample app.
  - `RoktSampleApp` — Rokt kit sample app.

## Key paths

- `Sdk/MParticle.Maui.Sdk/` — Main SDK project.
  - `MParticle.cs` — Main SDK class.
  - `MParticleAbstractions.cs` — Interface definitions.
  - `macios/` — iOS native bindings (ApiDefinitions.cs, Xcode project, SPM).
  - `android/` — Android native bindings (Gradle project, pre-built AAR).
- `Kits/rokt/Sdk/MParticle.Maui.Rokt/` — Rokt kit integration.
- `SampleApp/` — Core SDK sample application.
- `Kits/rokt/SampleApp/` — Rokt kit sample application.
- `mParticle.MAUI.sln` — Solution file.
- `global.json` — .NET SDK version pinning.
- `.trunk/trunk.yaml` — Trunk.io configuration.
- `CHANGELOG.md` — Release notes.
- `MIGRATING.md` — Migration guide.
- `RELEASING.md` — Release procedures.

## Code style, quality, and validation

- **Lint & format tools**:

  - **Primary enforcement tool**: `trunk check` (via Trunk.io).
  - .NET analyzers via MSBuild.
  - Important: Only add comments if absolutely necessary.

- **Strict post-change validation rule (always follow this)**:
  After **any** code change — even small ones — you **must** run the full validation sequence:

  1. `trunk check` — to lint, format-check, and catch style/quality issues.
  2. `dotnet restore` — Restore dependencies.
  3. Build SDK: `dotnet build Sdk/MParticle.Maui.Sdk/MParticle.Maui.Sdk.csproj`.
  4. Build SampleApp: `dotnet build SampleApp/SampleApp.csproj`.
  5. Pack: `dotnet pack Sdk/MParticle.Maui.Sdk/MParticle.Maui.Sdk.csproj -c Release`.
  6. If Rokt kit changed: build `Kits/rokt/Sdk/MParticle.Maui.Rokt/MParticle.Maui.Rokt.csproj` and its sample app.

  - Only propose / commit changes if all steps pass cleanly.

- **Style preferences**:

  - Follow .NET/C# conventions and Microsoft coding guidelines.
  - Prefer `readonly` fields and immutable types where possible.
  - Use `async`/`await` for asynchronous operations.
  - Write thorough XML documentation for all public APIs.

- **CHANGELOG.md maintenance**:
  - For **substantial changes**, **always add a clear entry** to `CHANGELOG.md`.
  - Use standard categories: `Added`, `Changed`, `Deprecated`, `Fixed`, `Removed`, `Security`.
  - Keep entries concise and written in imperative mood.
  - Never auto-generate or hallucinate changelog entries — flag for human review.

## Pull request and branching

- CODEOWNERS: `* @mParticle/sdk-team`
- Ensure the branch follows the standard feat/\* naming pattern.
- When creating pull requests, use the template located at: `.github/pull_request_template.md` as the basis for the description.

## External Resources

- [mParticle Documentation](https://docs.mparticle.com/)
- [Rokt mParticle Integration Docs](https://docs.rokt.com/developers/integration-guides/rokt-ads/customer-data-platforms/mparticle/)
