# .NET Version Upgrade Plan

## Overview

**Target**: Upgrade the PhotoDebug solution from .NET 8 to .NET 10.
**Scope**: 8 SDK-style projects (libraries, desktop app, and test projects), ~20k LOC, with API compatibility work concentrated in PhotoDump.

### Selected Strategy
**All-At-Once** — All projects upgraded simultaneously in a single operation.
**Rationale**: 8 projects, all already on modern .NET, shallow dependency graph, and no incompatible package migrations.

## Tasks

### 01-prerequisites: Verify SDK and upgrade prerequisites

Validate that the repository is ready for a .NET 10 upgrade by confirming SDK/toolchain compatibility and aligning repo-level SDK pinning. This task covers `global.json` and any baseline restore/build preconditions needed before changing project target frameworks.

The goal is to ensure the upgrade proceeds from a clean, reproducible environment and avoid mid-upgrade failures caused by SDK mismatch.

**Done when**: .NET 10 SDK compatibility is confirmed, prerequisite repo-level SDK configuration is updated if required, and baseline restore/build prechecks are complete.

---

### 02-upgrade-projects: Upgrade all projects to .NET 10 and fix compatibility issues

Upgrade all 8 projects in a single atomic pass by updating target frameworks from `net8.0` / `net8.0-windows` to .NET 10 equivalents and applying any required package/framework compatibility adjustments. This includes the core libraries, application projects, and test projects so the solution remains internally consistent.

This task also resolves the assessment-flagged .NET API issues, with focus on `PhotoDump\PhotoDump.csproj` where binary and behavioral changes were identified.

**Done when**: All projects target .NET 10, restore succeeds, and all compile-time compatibility issues identified for the upgrade are resolved.

---

### 03-final-validation: Validate build and tests on .NET 10

Run full solution validation after the upgrade pass is complete. This includes a clean build and execution of relevant test projects to verify that migrated projects compile and run correctly under .NET 10.

This task confirms the upgrade outcome is stable and ready for follow-up work.

**Done when**: Solution build completes without errors, test execution completes successfully, and no upgrade-blocking warnings or dependency conflicts remain.
