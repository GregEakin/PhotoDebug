# 01-prerequisites: Verify SDK and upgrade prerequisites

Validate that the repository is ready for a .NET 10 upgrade by confirming SDK/toolchain compatibility and aligning repo-level SDK pinning. This task covers `global.json` and any baseline restore/build preconditions needed before changing project target frameworks.

The goal is to ensure the upgrade proceeds from a clean, reproducible environment and avoid mid-upgrade failures caused by SDK mismatch.

## Research Findings

- `global.json` pinned SDK version `8.0.403` with `rollForward: latestFeature`; this blocks major-version roll-forward to SDK 10.
- `validate_dotnet_sdk_installation(net10.0)` confirmed a compatible .NET 10 SDK is installed on this machine.
- `validate_dotnet_sdk_in_globaljson(net10.0)` updated SDK roll-forward policy to `latestMajor`, which allows using installed .NET 10 SDK while keeping current pinning.
- Baseline precheck `dotnet build PhotoDebug.sln` completed successfully (warnings exist in existing test code and will be addressed in later upgrade/validation tasks if touched).

**Done when**: .NET 10 SDK compatibility is confirmed, prerequisite repo-level SDK configuration is updated if required, and baseline restore/build prechecks are complete.
