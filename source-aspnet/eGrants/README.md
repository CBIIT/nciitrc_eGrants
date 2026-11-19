# eGrants

eGrants is the National Cancer Institute (NCI) grants document management and
administration web application. It is an ASP.NET Core (.NET 8) MVC application
that provides secure document storage, indexing, PDF conversion, institutional
file management, reporting, and grant administration workflows for NCI staff.

This project (`eGrants`) is the modernized .NET 8 application. It is the
successor to the legacy `egrants_new` (.NET Framework) project that also lives in
this repository.

## Features

- Microsoft Entra ID (Azure AD) single sign-on via OpenID Connect
- Grant document upload, indexing, modify, delete, and restore workflows
- "Convert to PDF & Add" document conversion (via IronPdf) with large-file
  (up to 2 GB) upload support
- Institutional file management
- Dashboard, reminders, and reporting (doc transaction, system reports, GPMAT
  work reports)
- Grant access, flag maintenance, category editing, and supplement administration
- Cross-IC (Institute/Center) persona support with role-based access control
- Centralized structured logging and error-notification emails via Serilog

## Technology Stack

- **Framework:** ASP.NET Core MVC on .NET 8
- **Language:** C# (nullable reference types + implicit usings enabled)
- **Data access:** Entity Framework Core 9 (SQL Server)
- **Authentication:** Microsoft Entra ID (`Microsoft.Identity.Web`) with OIDC
  authorization-code flow
- **PDF generation:** IronPdf (embedded Chromium renderer)
- **Email parsing:** MsgReader
- **Logging:** Serilog (Console, rolling File, and Email sinks)
- **Reverse proxy / forwarding:** YARP + `Microsoft.AspNetCore.SystemWebAdapters`
- **GitHub integration:** Octokit (release-version lookup)

## Project Structure

```
eGrants/
??? Common/            Shared helpers (EgrantsCommon, EntraIdUserResolver, etc.)
??? Controllers/       MVC controllers (Egrants, EgrantsDoc, InstitutionalFiles, ...)
??? DAL/               EF Core AppDbContext and data-access layer
??? Database/          Database-related assets
??? DatabaseScripts/   SQL scripts
??? Docs/              Project documentation
??? DTOs/              Data transfer objects
??? Functions/         Supporting function/model classes
??? Middleware/        Custom middleware (exception handling, etc.)
??? Models/            Domain and view models (SessionInfo, etc.)
??? Repositories/      Repository layer + interfaces
??? Services/          Business-logic services + interfaces
??? ViewModels/        View-specific models
??? Views/             Razor views
??? wwwroot/           Static assets (CSS, JS, images)
??? Program.cs         Application startup and middleware pipeline
??? appsettings.json   Configuration
```

### Key Controllers

`EgrantsController`, `EgrantsDocController`, `InstitutionalFilesController`,
`EgrantsFundingController`, `DashboardController`, `ReminderController`,
`ManagementController`, `AdminController`, `EgrantsAccessController`,
`SupplementController`, `ApplDestructedController`, `CategoryEditController`,
`FlagMaintenanceController`, `GPMATWorkReportController`,
`DocTransactionReportController`, `SystemReportController`, `QCController`,
`AccountController`, and `ErrorController`.

## Functional Modules

The application is organized into the following functional areas.

### Grant & Application Search (`EgrantsController`)

The primary entry point and search surface for grants and applications.

- Search by free-text string (`by_str`), by grant (`by_grant`), by application
  (`by_appl`), by QC queue (`by_qc`), and by advanced filters such as fiscal
  year, mechanism, admin code, and serial number (`by_filters`).
- Paginated result navigation (`by_page`) and autocomplete lookups
  (`load_data_autocomplete`).
- Loads supporting data: grant years, applications list, and categories
  (`LoadYears`, `GetAllApplsList`, `LoadCategories`).
- Document grid loading, IMPAC II document data, and document attachments
  (`LoadDocsGrid`, `impac_docs_data`, `doc_attachments_data`).
- Stop-notice and supplement pop-ups (`stop_notice`, `supplement`).

### Document Management (`EgrantsDocController`)

Create, upload, index, modify, delete, and restore grant documents.

- Create documents via file upload or drag-and-drop, including PDF conversion
  of uploaded files ("Convert to PDF & Add").
- Index update/modify workflows, including handling of unidentified documents.
- Delete, restore, and re-index documents, with cross-IC effective-IC handling.
- Application creation helpers (`appl_create_default`, `create_new_appl`).
- If saving a converted PDF to disk fails, the orphaned document record created
  earlier in the workflow is removed so the database is not left in an
  inconsistent state.

### Document Download (`EgrantsController`)

- Single and bulk document download (`Download`, `IsDownloadForm`) with secure,
  GUID-based file retrieval.

### Funding Documents (`EgrantsFundingController`)

Management of funding documents by fiscal year.

- Browse/search funding records by fiscal year (`Index`, `ViewSearch`,
  `ViewAll`, `ViewArra`, `ViewEdit`).
- Create funding documents via file upload or drag-and-drop, with PDF conversion
  (`DocCreateByFile`, `DocCreatePdfByFile`, `DocCreateByDdrop`,
  `DocCreatePdfByDdrop`).
- Edit funding documents and associated applications (`DocEdit`, `ApplEdit`).

### Institutional Files (`InstitutionalFilesController`)

Management of institutional (organization-level) files.

- Browse and search organizations by name index (`Show_Orgs`, `Search_Orgs`).
- Show, create, update, and delete institutional documents for an organization
  (`Show_Docs`, `Create_Doc_by_File`, `Create_Doc_by_DDrop`, `Update_Doc`,
  `Delete_Doc`), with cross-IC effective-IC handling.
- The organization/institution list is ordered by name, and any profiles listed
  in the `HiddenProfiles` configuration are excluded.

### Dashboard & Reminders (`DashboardController`, `ReminderController`)

- Configurable dashboard with widgets (grants new / delayed / expedited /
  to-go, average processing time, audit reports, status) and user widget
  selection (`Save_Selection`).
- Reminder creation and management tied to applications and event types
  (`Reminder_Search`, `Reminder_Select`, `Save_Data`).

### Supplement Administration (`SupplementController`)

- Supplement notification review, save, delete, and resend
  (`Review_Notification`, `Save_Notification`, `Delete_Notification`,
  `Resent_Notification`).
- Email template management (`Create_Email_Template`, `View_Email_Template`) and
  supplement workflow / email-rule configuration (`Load_Workflow`,
  `Access_Email_Rule`).

### Quality Control (`QCController`, `ManagementController`)

- QC entry point (`QCController`).
- QC assignment and routing management: assign/remove QC persons and route work
  by percentage (`To_Assign`, `To_Remove`, `To_Route`).

### Application Destruction / Records Retention (`ApplDestructedController`)

- Manage grant/application destruction records and retention-year workflows.

### Administration

- **Access management** (`EgrantsAccessController`) – manage user access and
  acceptance workflows.
- **Category editing** (`CategoryEditController`) – maintain document categories
  and sub-categories.
- **Flag maintenance** (`FlagMaintenanceController`) – search, set up, and remove
  application/grant flags by flag type, admin code, and serial number.
- **Admin hub** (`AdminController`) – administrative landing and menu.

### Reporting (`DocTransactionReportController`, `SystemReportController`, `GPMATWorkReportController`)

- **Document transaction reports** by transaction type, person, and date range.
- **System reports** by serial number or accession id (paginated).
- **GPMAT work reports** for grants-management workload reporting.

### Integrations

- **IMPAC II / eRA** – retrieval of IMPAC documents and eRA-based data, using the
  configured eRA URL base and client certificate.
- **GitHub (Octokit)** – latest release/version lookup surfaced in the UI.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Access to the eGrants SQL Server database (EIM)
- A registered Microsoft Entra ID application (client ID/secret) with access to
  the NIH tenant
- An eRA client certificate (`.pfx`) for integrations that require it
- Visual Studio 2022 or the .NET CLI

## Configuration

Configuration is read from `appsettings.json` and environment-specific overrides.
Secrets are **not** stored in configuration files; instead, placeholders in
`appsettings.json` are replaced at startup from environment variables:

| Placeholder                     | Environment variable               | Purpose                              |
| ------------------------------- | ---------------------------------- | ------------------------------------ |
| `{DB_USER}`                     | `DB_USER`                          | SQL Server username                  |
| `{DB_PASSWORD}`                 | `DB_PASSWORD`                      | SQL Server password                  |
| `{eGrants_AzureAd_ClientSecret}`| `eGrants_AzureAd_ClientSecret`     | Entra ID client secret               |
| `{CERT_PASSWORD}`               | `CERT_PASSWORD`                    | eRA client certificate password      |
| `IronPdf:LicenseKey` (empty)    | `IRONPDF_LICENSE_KEY`              | IronPdf license key (DOCX?PDF)       |

The IronPdf license key is read from the `IRONPDF_LICENSE_KEY` environment
variable first, falling back to the `IronPdf:LicenseKey` configuration value. If
neither is present a warning is logged at startup and DOCX?PDF conversion will
fail with a licensing exception. The key must never be committed to source
control.

Other notable configuration sections:

- `ConnectionStrings:DefaultConnection` – SQL Server connection string
- `AzureAd` – Entra ID instance, tenant, client ID, and callback path
- `AccessControl:OverrideIc` – optional IC override for testing/pilot scenarios
- `HiddenProfiles` – array of institution/profile names to hide from the Admin
  access-control institution dropdowns (for example `[ "NCMHD" ]`)
- `Serilog` – Console, rolling File, and Email logging sinks

> When running in `Local`/`Development`, Serilog error-notification emails are
> redirected to the current developer (`<username>@mail.nih.gov`) instead of the
> shared team distribution list.

## Getting Started

1. Clone the repository and open `egrants_new.sln` (located in the repository
   root) in Visual Studio 2022, or use the .NET CLI.

2. Set the required environment variables, for example (PowerShell):

   ```powershell
   $env:DB_USER = "<db-user>"
   $env:DB_PASSWORD = "<db-password>"
   $env:eGrants_AzureAd_ClientSecret = "<entra-client-secret>"
   $env:CERT_PASSWORD = "<cert-password>"
   $env:IRONPDF_LICENSE_KEY = "<ironpdf-license-key>"
   ```

3. Restore and build:

   ```powershell
   dotnet restore
   dotnet build
   ```

4. Run the application:

   ```powershell
   dotnet run --project eGrants/eGrants.csproj
   ```

   The default route resolves to the `Egrants` controller's `Index` action.

## Running Tests

Unit and integration tests live in the `eGrants.Tests` project.

```powershell
dotnet test eGrants.Tests/eGrants.Tests.csproj
```

> Some integration tests require connectivity to a development database and are
> gated accordingly.

## Authentication & Session Flow

1. Unauthenticated requests are challenged via Entra ID (OIDC authorization-code
   flow).
2. After sign-in, middleware resolves the user identity from the
   `preferred_username` claim (`EntraIdUserResolver`) and resolves the user's IC.
3. User type and access are validated against the database; failed validation
   redirects to `egrants_default.htm`.
4. On success, user and application settings are loaded into the session for the
   remainder of the request lifecycle.

### Cross-IC Personas

eGrants supports cross-IC (Institute/Center) personas, allowing authorized users
to act on behalf of an IC other than their home IC. For these roles the
*effective IC* is the user's authority IC rather than their home IC, and
read-only versus write access is enforced based on the user's role and position.
The optional `AccessControl:OverrideIc` setting can be used to force a specific IC
for testing and pilot scenarios.

## Logging

Serilog writes to:

- **Console**
- **Rolling file** at `C:\Logs\log-<date>.txt`
- **Email** – error-level notifications sent to the eGrants dev distribution list

Serilog self-diagnostics are written to `serilog-selflog.txt` in the application
base directory to troubleshoot sink/configuration issues.

## Notes

- Large file uploads (up to 2 GB) are supported via tuned Kestrel, IIS, and form
  limits in `Program.cs`.
- IronPdf is initialized and warmed up at startup so the first document
  conversion does not pay the one-time Chromium cold-start cost. A valid license
  key (see `IRONPDF_LICENSE_KEY` above) is required for DOCX?PDF conversion.
- The application coexists with the legacy `egrants_new` project during the
  modernization effort.
- In `DEBUG` builds, uploaded and converted documents are stored locally under
  `C:\PdfFileOutput` instead of the production UNC share, and `Program.cs` maps
  the corresponding `/data/...` URL paths to that folder so files can be viewed
  locally.

## License

This software is subject to the NIH Software License. See the
[NIH Guidelines for Releasing Research Software](https://ncihub.org/resources/899/download/Guidelines_for_Releasing_Research_Software_04062015.pdf).

© National Institutes of Health / National Cancer Institute
