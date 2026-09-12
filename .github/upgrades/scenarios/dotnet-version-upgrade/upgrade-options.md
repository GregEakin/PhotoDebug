# Upgrade Options — PhotoDebug

Assessment: 8 SDK-style modern .NET projects (net8.0/net8.0-windows) with API breaking-change findings concentrated in PhotoDump.

## Strategy

### Upgrade Strategy
All projects are already on modern .NET with a small dependency graph and low overall migration complexity, so a single-pass upgrade is the best fit.

| Value | Description |
|-------|-------------|
| **All-at-Once** (selected) | Upgrade all projects together in one atomic pass, then validate the solution. |
| Top-Down | Upgrade entry-point apps first with temporary multi-targeting for shared libraries, then consolidate later. |

## Compatibility

### Unsupported API Handling
The assessment reports .NET 10 API compatibility incidents, so handling strategy for non-mechanical API fixes must be set.

| Value | Description |
|-------|-------------|
| **Fix Inline** (selected) | Resolve all API changes during the owning upgrade task, including complex ones. |
| Defer Complex Changes | Apply simple fixes now and create stubs/subtasks for complex replacements. |
