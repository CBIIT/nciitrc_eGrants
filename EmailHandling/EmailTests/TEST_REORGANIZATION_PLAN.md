# EmailTests Reorganization Plan

## Current Structure (Flat by Project)
```
EmailTests/
??? Router/                    # Mixed unit & integration tests
??? ExchangeFixed/             # Mixed unit & integration tests  
??? AddSuppEmailer/            # Already has Unit/Integration naming
??? AddSuppProd/               # Already has Unit/Integration naming
??? AddSuppVoteCollection/     # Mixed tests
??? CommonUtilities/           # Unit tests
??? DocManEmail/               # Mixed tests
??? LoadPfr/                   # Mixed tests
??? LoadSuppPfr/               # Mixed tests
??? EGrantsAcmAuditReport/     # Mixed tests
??? OGADisableEmail/           # Mixed tests
??? StartOutlook/              # Mixed tests
??? ProcessSmokeTests/         # Process-level smoke tests
```

## Proposed Structure (Organized by Test Type)
```
EmailTests/
??? Unit/                      # Unit tests (no external dependencies)
?   ??? Router/
?   ??? ExchangeFixed/
?   ??? AddSuppEmailer/
?   ??? AddSuppProd/
?   ??? AddSuppVoteCollection/
?   ??? CommonUtilities/
?   ??? DocManEmail/
?   ??? LoadPfr/
?   ??? LoadSuppPfr/
?   ??? EGrantsAcmAuditReport/
?   ??? OGADisableEmail/
?   ??? StartOutlook/
?
??? Integration/               # Integration tests (database, Outlook, file system)
?   ??? Router/
?   ??? ExchangeFixed/
?   ??? AddSuppEmailer/
?   ??? AddSuppProd/
?   ??? AddSuppVoteCollection/
?   ??? DocManEmail/
?   ??? LoadPfr/
?   ??? LoadSuppPfr/
?   ??? EGrantsAcmAuditReport/
?   ??? OGADisableEmail/
?   ??? StartOutlook/
?
??? Process/                   # Process-level smoke tests (renamed from ProcessSmokeTests)
?   ??? DependencySmokeTests.cs
?   ??? LogOutputSmokeTests.cs
?   ??? SchedulerExecutableSmokeTests.cs
?   ??? README.md
?   ??? SMOKE_TEST_OUTLOOK_SETUP.md
?
??? Shared/                    # Shared test helpers and processors
?   ??? TestProcessor.cs (from Router)
?   ??? TestAddSuppProcessor.cs (from AddSuppEmailer)
?   ??? Other test helper classes
?
??? TestAssemblyInitialize.cs
??? README.md
```

## Migration Strategy

### Phase 1: ? COMPLETED
- Create new folder structure (Unit, Integration, Process, Shared)
- Move ProcessSmokeTests ? Process

### Phase 2: Organize Test Files by Type
For each project folder (Router, ExchangeFixed, etc.):

1. **Identify Unit Tests**
   - Files with `[TestCategory("Unit")]` attribute
   - Files ending in `UnitTests.cs`
   - Test files with no external dependencies (no database, no file system, no Outlook)
   - Move to: `Unit/{ProjectName}/`

2. **Identify Integration Tests**
   - Files with `[TestCategory("Integration")]` attribute
   - Files ending in `IntegrationTests.cs`
   - Tests using `SqlConnection`, `Outlook.Application`, file system
   - Move to: `Integration/{ProjectName}/`

3. **Identify Shared Helpers**
   - Files like `TestProcessor.cs`, `TestHelper.cs`, `Mock*.cs`
   - Classes without `[TestClass]` attribute
   - Move to: `Shared/`

### Phase 3: Update Project File
- Update `EmailHandlingTests.csproj` to include new folder structure
- Verify all files are included in compilation

### Phase 4: Update Documentation
- Update `EmailTests/README.md` with new structure
- Update test run commands to reflect new organization

## Benefits of New Structure

1. **Clear Separation of Concerns**
   - Unit tests are isolated from integration tests
   - Easy to run only unit tests (fast feedback)
   - Easy to run only integration tests (when database/Outlook available)

2. **Improved Test Discovery**
   - Developers know where to find tests by type
   - Clear naming makes test purpose obvious

3. **Better CI/CD Integration**
   - Can run unit tests on every commit (fast)
   - Can run integration tests on merge/deploy (slower, requires infrastructure)
   - Can run smoke tests before deployment (validates executables)

4. **Follows Industry Best Practices**
   - Standard test organization pattern
   - Aligns with xUnit, NUnit, MSTest conventions
   - Easier onboarding for new developers

## Test Run Commands After Reorganization

```powershell
# Run all tests
dotnet test

# Run only unit tests (fast, no external dependencies)
dotnet test --filter "FullyQualifiedName~Unit"

# Run only integration tests (requires database/Outlook)
dotnet test --filter "FullyQualifiedName~Integration"

# Run only process smoke tests
dotnet test --filter "FullyQualifiedName~Process"

# Or use test categories
dotnet test --filter "TestCategory=Unit"
dotnet test --filter "TestCategory=Integration"
dotnet test --filter "TestCategory=SmokeTest"
```

## Implementation Status

- ? Phase 1: Folder structure created, ProcessSmokeTests ? Process
- ? Phase 2: All test files reorganized by type
- ? Phase 3: Project file automatically includes new folders (SDK-style)
- ?? Phase 4: Update documentation (in progress)

## Recommendation

**Approach**: Keep current organization but add new tests to appropriate folders

**Rationale**:
- Moving existing tests is risky and requires extensive validation
- All 620 tests currently pass - don't want to break that
- Can achieve benefits by:
  1. Adding `[TestCategory]` attributes to existing tests
  2. Placing new tests in correct folders
  3. Gradually migrating tests during refactoring

**Alternative**: If full reorganization is desired, should be done in separate PR with comprehensive testing.
