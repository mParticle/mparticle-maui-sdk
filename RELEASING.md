# Releasing

## How to Release the SDK

To create a new release version:

1. Navigate to the "Actions" tab in the GitHub repository
2. Select the "Create draft release" workflow
3. Click "Run workflow" and use the dropdown to select the `main` branch
4. Enter the version number in X.Y.Z format (e.g., 1.2.0)
5. If needed, add a suffix (e.g., -alpha.1)
6. Click "Run workflow" to start the process

This workflow will:

- Create a release PR with the specified version allowing you to review
- Publish the specified version of the SDK to the [NuGet Test Gallery](https://int.nugettest.org/).
- Generate release notes based on CHANGELOG.md

Once you have reviewed the generated PR, merge it to main. This will trigger
the "Release .NET MAUI SDK" workflow which will publish the SDK to [nuget.org](https://nuget.org/). You may have to wait up to an hour for the package to be indexed.
