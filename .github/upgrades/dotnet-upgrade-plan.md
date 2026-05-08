# .NET 8.0 Upgrade Plan

## Execution Steps

Execute steps below sequentially one by one in the order they are listed.

1. Validate that a .NET 8.0 SDK required for this upgrade is installed on the machine and if not, help to get it installed.
2. Ensure that the SDK version specified in global.json files is compatible with the .NET 8.0 upgrade.
3. Upgrade CommonUtilties\CommonUtilties.csproj
4. Upgrade Router\Router.csproj
5. Upgrade OGARequestAccountDisable\OGARequestAccountDisable.csproj
6. Upgrade StartOutlook\StartOutlook.csproj
7. Upgrade AddSuppEmailer\AddSuppEmailer.csproj
8. Upgrade AddSuppProd\AddSuppProd.csproj
9. Upgrade AddSuppVoteCollection\AddSuppVoteCollection.csproj
10. Upgrade DocManEmail\DocManEmail.csproj
11. Upgrade EGrantsAcmAuditReport\EGrantsAcmAuditReport.csproj
12. Upgrade ExchangeFixed\ExchangeFixed.csproj
13. Upgrade LoadPfr\LoadPfr.csproj
14. Upgrade LoadSuppPfr\LoadSuppPfr.csproj
15. Upgrade EmailTests\EmailTests.csproj
16. Run unit tests to validate upgrade in the projects listed below:
    - EmailTests\EmailTests.csproj

## Settings

This section contains settings and data used by execution steps.

### Excluded projects

| Project name   | Description         |
|:-----------------------------------------------|:---------------------------:|
| (none)  | No projects excluded        |

### Aggregate NuGet packages modifications across all projects

NuGet packages used across all selected projects or their dependencies that need version update in projects that reference them.

| Package Name         | Current Version | New Version | Description       |
|:------------------------------------------|:---------------:|:-----------:|:---------------------------------------------------|
| Microsoft.Office.Interop.Outlook    |                 | 15.0.4797.1004 | Required for Outlook COM interop in .NET 8      |
| MSTest.TestAdapter   | 2.2.10        | 3.6.3       | Recommended for .NET 8.0           |
| MSTest.TestFramework    | 2.2.10   | 3.6.3       | Recommended for .NET 8.0    |
| System.Data.SqlClient             || 4.8.6       | Required for SQL data access in .NET 8  |

### Project upgrade details

This section contains details about each project upgrade and modifications that need to be done in the project.

#### CommonUtilties modifications

Project properties changes:
  - Convert from old-style project format to SDK-style
  - Target framework should be changed from `net472` to `net8.0`
  - Output type: Library

Other changes:
- Remove AssemblyInfo.cs (attributes moved to project file)
  - Remove packages.config if present

#### Router modifications

Project properties changes:
  - Convert from old-style project format to SDK-style
  - Target framework should be changed from `net472` to `net8.0-windows`
  - Output type: Exe

NuGet packages changes:
  - Add Microsoft.Office.Interop.Outlook `15.0.4797.1004` (replacing COM reference)
  - Add System.Data.SqlClient `4.8.6` (for SqlConnection usage)

Other changes:
  - Remove COM references (Microsoft.Office.Core, Microsoft.Office.Interop.Outlook, stdole)
  - Remove AssemblyInfo.cs
  - Update config.csv to be copied to output

#### OGARequestAccountDisable modifications

Project properties changes:
  - Convert from old-style project format to SDK-style
  - Target framework should be changed from `net472` to `net8.0-windows`
  - Output type: Exe

NuGet packages changes:
  - Add Microsoft.Office.Interop.Outlook `15.0.4797.1004` (replacing COM reference)

Other changes:
  - Remove COM references
  - Remove AssemblyInfo.cs

#### StartOutlook modifications

Project properties changes:
  - Convert from old-style project format to SDK-style
  - Target framework should be changed from `net472` to `net8.0-windows`
  - Output type: Exe

NuGet packages changes:
  - Add Microsoft.Office.Interop.Outlook `15.0.4797.1004` (replacing package reference)

Other changes:
  - Remove AssemblyInfo.cs

#### AddSuppEmailer modifications

Project properties changes:
  - Convert from old-style project format to SDK-style
  - Target framework should be changed from `net472` to `net8.0-windows`
  - Output type: Exe

NuGet packages changes:
  - Add Microsoft.Office.Interop.Outlook `15.0.4797.1004` (replacing COM reference)

Other changes:
  - Remove COM references
  - Remove AssemblyInfo.cs

#### AddSuppProd modifications

Project properties changes:
  - Convert from old-style project format to SDK-style
  - Target framework should be changed from `net472` to `net8.0-windows`
  - Output type: Exe

NuGet packages changes:
  - Add Microsoft.Office.Interop.Outlook `15.0.4797.1004` (replacing COM reference)

Other changes:
  - Remove COM references
  - Remove AssemblyInfo.cs

#### AddSuppVoteCollection modifications

Project properties changes:
  - Convert from old-style project format to SDK-style
  - Target framework should be changed from `net472` to `net8.0-windows`
  - Output type: Exe

NuGet packages changes:
  - Add Microsoft.Office.Interop.Outlook `15.0.4797.1004` (replacing COM reference)

Other changes:
  - Remove COM references
  - Remove AssemblyInfo.cs

#### DocManEmail modifications

Project properties changes:
  - Convert from old-style project format to SDK-style
  - Target framework should be changed from `net472` to `net8.0-windows`
  - Output type: Exe

NuGet packages changes:
  - Add Microsoft.Office.Interop.Outlook `15.0.4797.1004` (replacing COM reference)

Other changes:
  - Remove COM references
  - Remove AssemblyInfo.cs

#### EGrantsAcmAuditReport modifications

Project properties changes:
  - Convert from old-style project format to SDK-style
  - Target framework should be changed from `net472` to `net8.0`
  - Output type: Exe

Other changes:
  - Remove AssemblyInfo.cs

#### ExchangeFixed modifications

Project properties changes:
  - Convert from old-style project format to SDK-style
  - Target framework should be changed from `net472` to `net8.0-windows`
  - Output type: Exe

NuGet packages changes:
  - Add Microsoft.Office.Interop.Outlook `15.0.4797.1004` (replacing COM reference)

Other changes:
  - Remove COM references
  - Remove AssemblyInfo.cs

#### LoadPfr modifications

Project properties changes:
  - Convert from old-style project format to SDK-style
  - Target framework should be changed from `net472` to `net8.0`
  - Output type: Exe

Other changes:
  - Remove AssemblyInfo.cs

#### LoadSuppPfr modifications

Project properties changes:
  - Convert from old-style project format to SDK-style
  - Target framework should be changed from `net472` to `net8.0`
  - Output type: Exe

Other changes:
  - Remove AssemblyInfo.cs

#### EmailTests modifications

Project properties changes:
  - Convert from old-style project format to SDK-style
  - Target framework should be changed from `net472` to `net8.0-windows`
  - Output type: Library (Test project)

NuGet packages changes:
  - MSTest.TestAdapter should be updated from `2.2.10` to `3.6.3`
  - MSTest.TestFramework should be updated from `2.2.10` to `3.6.3`
  - Add Microsoft.Office.Interop.Outlook `15.0.4797.1004` (replacing COM reference)

Other changes:
  - Remove COM references
  - Remove packages.config
  - Remove AssemblyInfo.cs
