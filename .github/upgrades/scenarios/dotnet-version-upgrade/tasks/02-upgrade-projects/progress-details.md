# Progress Details — 02-upgrade-projects

## Summary
Completed atomic project retargeting from .NET 8 to .NET 10 across all 8 projects and verified solution compile on the upgraded target frameworks.

## Files Modified
- `ExifTool/ExifTool.csproj`
- `HexDump/HexDump.csproj`
- `JpegParser/JpegParser.csproj`
- `JpegParserTests/JpegParserTests.csproj`
- `PhotoDump/PhotoDump.csproj`
- `PhotoLib/PhotoLib.csproj`
- `PhotoTests/PhotoTests.csproj`
- `TagDatabase/TagDatabase.csproj`
- `.github/upgrades/scenarios/dotnet-version-upgrade/tasks/02-upgrade-projects/task.md`

## Validation Performed
- `dotnet build PhotoDebug.sln` after retargeting → **Succeeded**.
- `dotnet test` CLI encountered a .NET 10 test-host handshake crash (`KeyNotFoundException` key `8`), so validation switched to Visual Studio Test Explorer runner.
- Test Explorer run for `JpegParserTests` + `PhotoTests` → 412 total, 395 passed, 17 failed.

## Test Failure Notes
Observed failures are concentrated in `PhotoTests` and are primarily runtime/data-environment dependent (missing local file paths under `P:\...`, IO access/content assertions in prototype tests). No compile-time upgrade regressions were introduced by the framework retargeting step.

## Outcome
- All project TFMs are now set to `net10.0` / `net10.0-windows`.
- Restore/build succeeded on .NET 10.
- Remaining test failures require project-specific data/runtime triage and are carried into final validation reporting.
