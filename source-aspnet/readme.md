# eGrants

The eGrants solution is an ASP.NET Core (.NET 8) application used by NCI to manage grant application documents, institutional files, funding files, dashboards, and administrative workflows. This document describes the projects that make up the solution.

> Note: The `egrants_new` project is intentionally not documented here.

## Table of Contents

- [Solution Structure](#solution-structure)
- [eGrants](#egrants)
- [pdf_file_conversion](#pdf_file_conversion)
- [eGrants.Tests](#egrantstests)
- [Prerequisites & Setup](#prerequisites--setup)
- [Configuration](#configuration)
- [Authentication & Authorization](#authentication--authorization)
- [Database](#database)
- [Running & Debugging](#running--debugging)
- [Testing](#testing)
- [Deployment](#deployment)
- [Logging & Diagnostics](#logging--diagnostics)
- [Project Conventions](#project-conventions)
- [Contributing](#contributing)
- [License](#license)
- [Support & Contact](#support--contact)
- [Changelog](#changelog)

## Solution Structure

| Project | Type | Target Framework | Description |
| --- | --- | --- | --- |
| [`eGrants`](#egrants) | ASP.NET Core Web (Razor Pages / MVC) | .NET 8 | Main web application. |
| [`pdf_file_conversion`](#pdf_file_conversion) | Class Library | .NET Standard 2.0 | Document/PDF conversion library. |
| [`eGrants.Tests`](#egrantstests) | Test Project | .NET 8 | Unit and integration tests. |

---

## eGrants

The primary web application and the heart of the solution. It is the electronic grants records-management system that lets NCI staff search, view, upload, organize, track, and report on grant-related documents. It is built with ASP.NET Core (.NET 8), hosts controllers and Razor views, authenticates users through Microsoft Entra ID (OIDC), and persists data to SQL Server via Entity Framework Core.

### What it does

The application drives its behavior from an authenticated user session. On each request, middleware in `Program.cs` reads the Entra ID claims (via `EntraIdUserResolver`), resolves the user's ID and Institute/Center code, determines their user type and permissions through `EgrantsCommon`, and populates the session. From there, the various feature areas provide the functionality below:

- **Grant & application search / viewing (`EgrantsController`, `eGrantsService`)** – Search for grant applications, list results with pagination, and display grant folders, application layers, and their associated documents and categories.
- **Document management (`EgrantsDocController`, `DocumentService`)** – Create, upload, update, rename, move, and download grant documents. Supports converting incoming files (Word, RTF, HTML, images/TIFF, email, etc.) to PDF using the `pdf_file_conversion` library, and supports large (up to 2 GB) file uploads configured in `Program.cs`.
- **Institutional files (`InstitutionalFilesController`, `InstitutionalFilesService`)** – Browse and manage institution/organization-level files: list organizations, show/create/update the documents attached to an organization.
- **Funding files (`EgrantsFundingController`, `EgrantsFundingService`)** – Manage funding-related master records and funding documents at the application and grant level.
- **Dashboards & reminders (`DashboardController`, `ReminderController`)** – Present summary widgets (new grants, delayed grants, grants "to go," expedited, status counts, average processing time, audit reports, link lists) and reminder notifications.
- **Administration (`Admin` controllers)** – Maintenance screens for flags (`FlagMaintenanceController`), categories/subcategories (`CategoryEditController`), user access (`EgrantsAccessController`), supplement workflow and email templates (`SupplementController`), destroyed/destructed applications (`ApplDestructedController`), GPMAT work reports (`GPMATWorkReportController`), and the admin menu (`AdminController`).
- **Management reporting (`ManagementController`, `DocTransactionReportController`, `SystemReportController`)** – System and document-transaction reports for oversight.
- **Quality Control (`QCController`)** – QC review workflows, including QC reasons, persons, and reports.
- **Error handling (`ErrorController`, `ExceptionHandling` middleware)** – Centralized exception handling and friendly error/status-code pages.

### Architecture

The project follows a layered architecture with dependency injection wired up in `Program.cs`:

- **Controllers** – MVC/Razor controllers grouped by area (`Admin`, `Dashboard`, `Egrants`, `Management`, `QC`).
- **Services** (`Services/` + `Services/Interfaces/`) – Business-logic layer; each service is registered as a scoped dependency behind an interface (e.g., `IeGrantsService`, `IDocumentService`, `IInstitutionalFilesService`).
- **Repositories** (`Repositories/` + `Repositories/Interfaces/`) – Data-access layer that encapsulates EF Core queries.
- **DAL** – `AppDbContext` (Entity Framework Core) mapping the many `Models` entities (grants, applications, documents, persons, categories, flags, etc.) to SQL Server.
- **Models / DTOs / ViewModels** – Domain entities, data-transfer objects, and strongly-typed view models used by the views.
- **Views** – Razor views (`.cshtml`) rendering the pages, modals, and dashboard widgets.
- **Middleware** – Custom middleware such as `ExceptionHandling` plus inline session-initialization middleware.
- **Common / Functions** – Shared helpers (`EgrantsCommon`, `EntraIdUserResolver`), enums, and extension methods (e.g., `StringTrimExtension`).

### Notable Dependencies
- `Microsoft.EntityFrameworkCore` / `Microsoft.EntityFrameworkCore.SqlServer` – Data access to SQL Server.
- `Microsoft.Identity.Web` / `Microsoft.Identity.Web.UI` – Microsoft Entra ID (OIDC) authentication and sign-in UI.
- `IronPdf`, `MsgReader` – Document and PDF handling.
- `Serilog.AspNetCore` / `Serilog.Sinks.Email` – Structured logging with email alerting.
- `Microsoft.AspNetCore.SystemWebAdapters.CoreServices`, `Yarp.ReverseProxy` – System.Web adapters and reverse-proxy/forwarding support (aids migration from legacy .NET Framework).
- `Octokit` – GitHub API integration.
- References the `pdf_file_conversion` project for file-to-PDF conversion.

---

## pdf_file_conversion

A reusable .NET Standard 2.0 class library responsible for converting a variety of source document formats into PDF so that heterogeneous grant files can be stored and displayed in a single, consistent format. The `eGrants` web application references this library and calls into it whenever a user uploads or converts a document.

### What it does

Given an input file (or its bytes) and its type, the library selects the appropriate converter and produces a normalized PDF. The `PdfConverter` entry point and `ContentForPdf` model orchestrate the pipeline, while each format has a dedicated converter (and matching interface for dependency injection via `Ninject`):

- **HTML ? PDF** (`HtmlConverter` / `IHtmlConverter`) – Renders HTML markup to PDF.
- **Word ? PDF** (`WordConverter`, `WordDocConverter` / `IWordConverter`, `IWordDocConverter`) – Converts `.doc`/`.docx` documents.
- **RTF ? PDF** (`RTFConverter` / `IRTFConverter`) – Converts rich-text documents.
- **Images/TIFF ? PDF** (`TIFFConverter`, `GeneralImageConverter` / `ITIFFConverter`, `IGeneralImageConverter`) – Converts single- and multi-page images.
- **Email ? PDF** (`EmailTextConverter` / `IEmailTextConverter`) – Parses `.msg` email files and renders their content.
- **Formatted / plain text ? PDF** (`FormattedTextConverter` / `IFormattedTextConverter`) – Converts formatted or plain text (including Markdown).
- **PDF passthrough / assembly** (`PDFConverter` / `IPDFConverter`) – Handles existing PDFs and combines content.

The `IConvertToPdf` interface and `Constants` provide the common contract and configuration shared across converters.

### Notable Dependencies
- `IronPdf` – PDF generation.
- `NPOI`, `Spire.Doc` – Office document processing.
- `MsgReader` – Email (`.msg`) parsing.
- `Markdig` – Markdown processing.
- `SkiaSharp` – Image processing.
- `Ninject` – Dependency injection.
- `Serilog` – Logging.

---

## eGrants.Tests

A test project (.NET 8) providing automated unit and integration test coverage for the `eGrants` application. It uses **xUnit** as the test framework, **Moq** for mocking dependencies, `Microsoft.EntityFrameworkCore.InMemory` for database-backed tests, and `Microsoft.AspNetCore.Mvc.Testing` for spinning up the app in-process.

### What it does

The project validates the correctness of the web application's services, controllers, and end-to-end request handling:

- **Unit tests** (`Unit/`) – Exercise individual services and controllers in isolation using mocks:
  - `Service/eGrantsServiceTests`, `DocumentServiceTests`, `Service/InstitutionalFilesServiceTests` – business-logic verification.
  - `Controllers/EgrantsDocControllerTests` – controller behavior.
  - `Authentication/EntraIdUserResolverTests` – verifies Entra ID claim-to-user resolution logic.
- **Integration tests** (`Integration/`) – Boot the application via a test host and issue real HTTP requests: `EgrantsControllerTests`, `EgrantsDocsControllerTests`, `InstitutionalFilesControllerTests`.
- **Utilities** (`Utilities/`) – Shared test helpers such as `TestSession` (an in-memory `ISession` implementation for simulating user sessions).

### Notable Dependencies
- `xunit` / `xunit.runner.visualstudio` – Test framework and runner.
- `Moq` – Mocking.
- `Microsoft.AspNetCore.Mvc.Testing` – In-process integration testing.
- `Microsoft.EntityFrameworkCore.InMemory` – In-memory database for tests.
- `coverlet.collector` – Code coverage.
- References the `eGrants` project.

---

## Prerequisites & Setup

### Required tools
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (the application connects to an `EIM` database)
- [LibreOffice](https://www.libreoffice.org/) – used for some document conversions (path is configured under the `LibreOffice` setting)
- An IDE: **Visual Studio 2022** (recommended) or **Visual Studio Code** with the C# Dev Kit

### Recommended global tools
- `dotnet-ef` for Entity Framework Core migrations:
  ```powershell
  dotnet tool install --global dotnet-ef
  ```

### Clone
```powershell
git clone https://github.com/CBIIT/nciitrc_eGrants.git
cd nciitrc_eGrants
```

---

## Configuration

Configuration is layered across environment-specific `appsettings` files in the `eGrants` project:

- `appsettings.json` – base settings
- `appsettings.Development.json`
- `appsettings.Staging.json`
- `appsettings.Production.json`
- `appsettings.Test.json`

The active file is selected by the `ASPNETCORE_ENVIRONMENT` environment variable (e.g., `Development`).

### Secrets & environment variables

Sensitive values are injected at runtime rather than stored in the committed files. `Program.cs` replaces `{DB_USER}` and `{DB_PASSWORD}` placeholders in the connection string with environment variables:

| Variable | Purpose |
| --- | --- |
| `DB_USER` | SQL Server user id for the `DefaultConnection` string |
| `DB_PASSWORD` | SQL Server password for the `DefaultConnection` string |
| `EGRANTS_CERT_PASSWORD` | Password for the client certificate (`AppSettings:certPass`) |
| `GITHUB_TOKEN` | Token used for GitHub (Octokit) integration (`AppSettings:GitHubToken`) |
| `ASPNETCORE_ENVIRONMENT` | Selects the active environment/config (e.g., `Development`) |
| `ProxyTo` | Optional upstream URL for the reverse proxy/forwarder |

> ?? **Do not commit real secrets.** For local development prefer [.NET User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) or environment variables. Any secrets currently present in the config files (client secret, tokens, certificate passwords) should be rotated and moved to a secure store (e.g., environment variables or a key vault).

### Key configuration sections
- **`ConnectionStrings:DefaultConnection`** – SQL Server connection (uses `{DB_USER}`/`{DB_PASSWORD}` placeholders).
- **`AzureAd`** – Microsoft Entra ID (OIDC) settings: `Instance`, `Domain`, `TenantId`, `ClientId`, `ClientSecret`, `CallbackPath`, `SignedOutCallbackPath`.
- **`Serilog`** – Logging sinks (Console, rolling File at `C:\Logs\log-.txt`, and an Email sink for errors).
- **`AppSettings`** – Application-specific paths and URLs (image/file server URLs, document relative paths, eRA endpoints, certificate path, GitHub token).
- **`LibreOffice:Path`** – Path to `soffice.exe` for document conversion.

---

## Authentication & Authorization

The application authenticates users with **Microsoft Entra ID** using OpenID Connect via `Microsoft.Identity.Web`:

- Configuration lives in the `AzureAd` section of `appsettings`.
- A global **fallback authorization policy** requires every request to be from an authenticated user.
- After sign-in, middleware in `Program.cs` uses `EntraIdUserResolver` to read the `preferred_username` claim, derive the user id and Institute/Center (IC) code, and initialize the user session (user type, permissions, person id) via `EgrantsCommon`.
- Sign-in/sign-out callbacks use `/signin-oidc` and `/signout-callback-oidc`.

To run against your own tenant, register an app registration in Entra ID and update `TenantId`, `ClientId`, `ClientSecret`, and the redirect URIs to match the `CallbackPath`/`SignedOutCallbackPath`.

---

## Database

The application uses Entity Framework Core against SQL Server through `AppDbContext` (in `eGrants/DAL`).

- Set `DB_USER` and `DB_PASSWORD` environment variables and confirm the `DefaultConnection` server/catalog values.
- SQL scripts (if any) are kept under `eGrants/Database/Scripts/`.
- If migrations are used, run them with:
  ```powershell
  dotnet ef database update --project eGrants/eGrants.csproj
  ```

---

## Running & Debugging

### Build
```powershell
dotnet build
```

### Run the application
```powershell
dotnet run --project eGrants/eGrants.csproj
```

By default (see `Properties/launchSettings.json`) the app listens on:
- Kestrel: `https://localhost:7275` and `http://localhost:5275`
- IIS Express: `http://localhost:65167` (SSL `44575`)

Notes:
- Trust the local HTTPS development certificate: `dotnet dev-certs https --trust`.
- The app enforces HTTPS redirection and (outside Development) HSTS.
- **Large uploads:** Kestrel/IIS and form limits are configured for up to **2 GB** uploads to support the "Convert to PDF & Add" feature. If uploads fail near completion with `ERR_HTTP2_PROTOCOL_ERROR`, check upstream proxy/timeout settings (see the notes in `Program.cs`).

---

## Testing

Run the full test suite:
```powershell
dotnet test eGrants.Tests/eGrants.Tests.csproj
```

Collect code coverage (via `coverlet.collector`):
```powershell
dotnet test eGrants.Tests/eGrants.Tests.csproj --collect:"XPlat Code Coverage"
```

The `eGrants.Tests` project contains both unit tests (services/controllers with Moq) and in-process integration tests (`Microsoft.AspNetCore.Mvc.Testing`).

---

## Deployment

- **Hosting:** The app is configured to run under both **IIS** (`IISServerOptions`, Windows Authentication enabled in `iisSettings`) and **Kestrel** (`KestrelServerOptions`). Choose the model that matches your target environment.
- **Environment:** Set `ASPNETCORE_ENVIRONMENT` to `Staging` or `Production` to load the corresponding `appsettings` file.
- **Secrets:** Provide `DB_USER`, `DB_PASSWORD`, `EGRANTS_CERT_PASSWORD`, and `GITHUB_TOKEN` through the hosting platform's secure configuration (environment variables / key vault), not in source control.
- **Reverse proxy:** `Yarp.ReverseProxy` and the HTTP forwarder are registered; set `ProxyTo` to forward requests to an upstream service when needed.
- **Production hardening:** Outside Development, HSTS is enabled and custom exception/status-code pages are served (`/Error`), while several server-identifying response headers are stripped.
- **Publish:**
  ```powershell
  dotnet publish eGrants/eGrants.csproj -c Release -o ./publish
  ```

---

## Logging & Diagnostics

Logging is handled by **Serilog**, configured in the `Serilog` section of `appsettings`:

- **Console** sink for local output.
- **File** sink writing daily rolling logs to `C:\Logs\log-.txt`.
- **Email** sink that sends error-level events (e.g., to `eGrantsDevs@mail.nih.gov`) with the environment in the subject.

Serilog self-diagnostics (configuration/sink failures) are written to `serilog-selflog.txt` in the app base directory — useful when logs aren't appearing as expected.

---

## Project Conventions

- **Architecture:** Layered design — Controllers ? Services (behind interfaces) ? Repositories (behind interfaces) ? `AppDbContext`. Register new services/repositories for DI in `Program.cs`.
- **Interfaces:** Each service and repository has a matching interface under `Services/Interfaces` or `Repositories/Interfaces`.
- **Organization:** Controllers are grouped by area (`Admin`, `Dashboard`, `Egrants`, `Management`, `QC`); models, DTOs, and view models live in their respective folders.
- **Branching:** Work is done on feature branches (e.g., `dd/eGrants-1259`) and merged via pull request into the mainline branch.

---

## Contributing

1. Create a feature branch from the mainline branch.
2. Make your changes, following the existing code style and layered architecture.
3. Add or update unit/integration tests in `eGrants.Tests`.
4. Ensure `dotnet build` and `dotnet test` pass.
5. Open a pull request describing your changes for review.

---

## License

Internal NCI/NIH application. Add the applicable license or usage terms here.

---

## Support & Contact

- **Development team:** eGrantsDevs@mail.nih.gov
- **Repository:** https://github.com/CBIIT/nciitrc_eGrants
- For issues, open a ticket in the project's issue tracker.

---

## Changelog

Notable changes are tracked per release. Add release notes here or link to a `CHANGELOG.md`.
