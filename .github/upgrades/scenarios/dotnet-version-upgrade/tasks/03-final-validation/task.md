# 03-final-validation: Validate build and tests on .NET 10

Run full solution validation after the upgrade pass is complete. This includes a clean build and execution of relevant test projects to verify that migrated projects compile and run correctly under .NET 10.

This task confirms the upgrade outcome is stable and ready for follow-up work.

## Research Findings

- All project files were retargeted to `net10.0` / `net10.0-windows` in task `02-upgrade-projects` and prior full build succeeded.
- Existing warning baseline remains high (currently 85 warnings, mostly `CS0649` and `xUnit1013` in `PhotoTests`), which impacts strict warning-free validation criteria.
- Prior test run on the upgraded code reported 17 failing tests in `PhotoTests` with a mix of missing local data dependencies (`P:\...`) and assertion/runtime failures in prototype/integration-style tests.

**Done when**: Solution build completes without errors, test execution completes successfully, and no upgrade-blocking warnings or dependency conflicts remain.
