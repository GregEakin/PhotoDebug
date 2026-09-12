# .NET Version Upgrade

## Preferences
- **Flow Mode**: Automatic
- **Target Framework**: net10.0

## Source Control
- **Source Branch**: dev
- **Working Branch**: upgrade-dotnet-10
- **Commit Strategy**: Single Commit at End
- **Branch Sync**: Auto (Merge)

## Upgrade Options
**Source**: .github/upgrades/scenarios/dotnet-version-upgrade/upgrade-options.md

### Strategy
- Upgrade Strategy: All-at-Once

### Compatibility
- Unsupported API Handling: Fix Inline

## Strategy
**Selected**: All-At-Once
**Rationale**: 8 projects, all already on modern .NET (net8.0/net8.0-windows), SDK-style project format throughout, shallow dependency graph, and no incompatible package migrations.

### Execution Constraints
- Perform a single atomic upgrade pass across all projects (no tiered or phased sequencing).
- Update project TFMs and package references solution-wide before attempting code-level API fixes.
- Complete compilation remediation in one bounded pass after restore, then run full validation.
- Keep deferred work to a minimum by fixing API compatibility changes inline during the owning upgrade task.
