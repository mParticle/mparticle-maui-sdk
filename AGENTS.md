# AGENTS.md

.NET MAUI bindings over the native mParticle Apple and Android SDKs, published to NuGet. This file
holds only what the repo cannot tell you itself: the traps, what the gates cover, and the
conventions no config enforces. Versions, target frameworks and layout are deliberately absent —
read `global.json`, the `.csproj` files and `.github/workflows/pull-request.yml`, which cannot go
stale. API usage is in [`README.MD`](README.MD), releasing in [`RELEASING.md`](RELEASING.md).

## Scope of change

- Public NuGet SDK, not an app: keep the C# surface additive and deprecate rather than remove.
- Never edit a binding definition without checking it against the native header or Java API it
  wraps — a wrong signature compiles and fails at runtime.
- Do not change `.github/`, `global.json` or NuGet configuration unless asked.

## Toolchain

`global.json` pins the .NET SDK with `rollForward: latestPatch`, so an older SDK — or a newer
feature band — does not satisfy it and **every** `dotnet` command in the tree fails with
`A compatible .NET SDK was not found`, including `dotnet --version`. That reads like a broken
install rather than a version mismatch, so check `global.json` first.

The MAUI workload is required (`dotnet workload install maui`) and iOS work needs macOS with Xcode.
CI pins the SDK, workload and Xcode versions in `.github/workflows/pull-request.yml`; take them
from there, not from this file or `README.MD`.

## Commands

That workflow is the source of truth for the build sequence: `trunk check`, then per project
resolve SPM → build the native Xcode project → `dotnet build` → `dotnet pack`. Lint with
`trunk check --all`, build one project with `dotnet build <path-to-csproj>`.

### Command traps

1. **A kit's `.csproj` filename does not match its directory.**
   `Kits/rokt/Sdk/MParticle.Maui.Rokt/` contains `mParticle.Maui.Kits.Rokt.csproj`, not
   `MParticle.Maui.Rokt.csproj`; the Payments kit has the same shape. Guessing the path fails.
2. **`trunk check` with no arguments checks only changed files**, while CI runs it with
   `check-mode: all`. A clean local run can still fail the `trunk-check` gate.
3. **The root `mParticle.MAUI.sln` is not the whole repo.** It omits the Payments kit and
   `Kits/rokt/VerifyApp`, so a solution-wide build does not cover them. CI builds each project by
   path; do the same.
4. **The native Xcode binding has to exist before the iOS target framework builds.** CI runs
   `xcodebuild -resolvePackageDependencies` then `xcodebuild ... build` against each
   `macios/native/*/*.xcodeproj` before `dotnet build`. If an iOS build cannot find the
   `.xcframework`, run those two steps first.
5. **NuGet package IDs are not the project names** (`MParticle.Maui.Sdk` ships as
   `mParticle.MAUI`); check `<PackageId>` before referencing a package by name.

## Native SDK version coupling

- **iOS** — one `exact:` pin per `macios/native/*/Package.swift`. Dependabot watches those
  directories (`.github/dependabot.yml`).
- **Android** — the version lives in `android/native/*/gradle/libs.versions.toml`, but the AAR
  filename is hardcoded twice more: in the `copyDeps` rename in `build.gradle.kts`, and in the
  `<AndroidLibrary Include="...">` path in the `.csproj`. No gradle entry exists in
  `.github/dependabot.yml`, so these bumps are manual.

**Trap:** that `copyDeps` rename is a literal string. Bump `libs.versions.toml` alone and gradle
renames the _new_ AAR to the _old_ filename, the `.csproj` reference still resolves, and the build
stays green while the filename misstates the version that ships. Change all three together.
Transitive AARs keep their real version, so those fail loudly on a missing file instead.

## What the gates cover

The default branch requires two checks, `build` and `trunk-check`.

- **There are no test projects in this repo** and no test step in CI, so green means it compiled
  and packed. Exercise behaviour changes by hand in `SampleApp/` or `Kits/rokt/SampleApp/`.
- `Kits/rokt/VerifyApp/` is not built by CI.
- No project sets `TreatWarningsAsErrors`, so warnings do not fail the build.

## Conventions

- **XML documentation on public APIs is expected but unenforced** — nothing sets
  `GenerateDocumentationFile`, so a missing `/// <summary>` produces no warning.
- Add code comments only where they earn their place.
- Base work on `main`. `development` sits behind it and is not where PRs merge.
- Branch names follow `<type>/<description>` and PR titles Conventional Commits, by convention; no
  ruleset enforces either. `main` merges by squash only, so the PR title becomes the commit message.
- Describe PRs using `.github/PULL_REQUEST_TEMPLATE.md`. Code-owner review is required
  (`.github/CODEOWNERS`), threads must be resolved, and a push dismisses approvals.

## Changelog, migration notes and versioning

- Record user-visible changes in `CHANGELOG.md` under `## [Unreleased]`, using the Keep a
  Changelog categories. The release workflow promotes that section, so an entry written elsewhere
  is not picked up. Never invent an entry; leave it for review.
- Breaking or behaviour-changing binding edits also need a note in `MIGRATING.md` under
  `## Unreleased migration notes`.
- **Do not hand-edit `VERSION` or the `<Version>` elements in the `.csproj` files.** The "Create
  draft release" workflow bumps them and opens the release PR
  (`.github/workflows/draft-release-publish.yml`).

## External resources

- [mParticle documentation](https://docs.mparticle.com/)
- [Rokt mParticle integration guide](https://docs.rokt.com/developers/integration-guides/rokt-ads/customer-data-platforms/mparticle/)
