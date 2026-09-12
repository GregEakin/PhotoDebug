# 02-upgrade-projects: Upgrade all projects to .NET 10 and fix compatibility issues

Upgrade all 8 projects in a single atomic pass by updating target frameworks from `net8.0` / `net8.0-windows` to .NET 10 equivalents and applying any required package/framework compatibility adjustments. This includes the core libraries, application projects, and test projects so the solution remains internally consistent.

This task also resolves the assessment-flagged .NET API issues, with focus on `PhotoDump\PhotoDump.csproj` where binary and behavioral changes were identified.

## Scope Inventory

### Projects affected
- ExifTool\ExifTool.csproj
- HexDump\HexDump.csproj
- JpegParser\JpegParser.csproj
- JpegParserTests\JpegParserTests.csproj
- PhotoDump\PhotoDump.csproj
- PhotoLib\PhotoLib.csproj
- PhotoTests\PhotoTests.csproj
- TagDatabase\TagDatabase.csproj

### Distinct concerns
- TFM retargeting in all project files (`net8.0` → `net10.0`, `net8.0-windows` → `net10.0-windows`).
- Compatibility verification for assessment-reported API risks in WPF project `PhotoDump` after retarget.
- Full-solution restore/build validation after the atomic TFM update pass.

### Assessment findings (queried per project)
- All 8 projects have `Project.0002` (target framework needs change).
- `PhotoDump` has additional API incidents: `Api.0001` (binary incompatible, 12) and `Api.0003` (behavioral change, 5), largely surfaced from WPF-generated code paths.
- No incompatible NuGet package migrations were identified for this task.

### Dependencies and package/property location checks
- `TargetFramework` is defined directly in each project file (no central `Directory.Build.props` target-framework override detected).
- No `// STUB:` markers found in the task scope.

**Done when**: All projects target .NET 10, restore succeeds, and all compile-time compatibility issues identified for the upgrade are resolved.
