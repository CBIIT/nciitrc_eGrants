# EmailHandling Solution (.NET8)

## Overview
This solution is a modernized migration of legacy VBS scripts to .NET8 C# projects for robust, maintainable, and secure email and document processing. Each VBS script is now a dedicated C# console application, with shared utilities and best practices for configuration, logging, and secrets management.

## Projects
- AddSuppEmailer
- AddSuppProd
- AddSuppVoteCollection
- ~~DocManEmail~~ (deprecated - no longer in production)
- ExchangeFixed
- LoadPfr
- LoadSuppPfr
- OGARequestAccountDisable
- Router
- EGrantsAcmAuditReport
- CommonUtilties (shared utilities)
- EmailTests (integration/unit tests)

## Configuration
Each project uses a `config.csv` file for environment-specific settings (connection strings, folder paths, etc.).

- **Secrets (usernames, passwords, etc.) are never stored directly in config files.**
- Instead, use environment variable placeholders (e.g., `%EGRANTS_DB_USER%`, `%EGRANTS_DB_PASSWORD%`).

Example:
```
conStr,,,,,User ID=%EGRANTS_DB_USER%;Password=%EGRANTS_DB_PASSWORD%;...
```

## Secrets Management
- **Local Development:**
 - Store secrets in `secrets.local.csv` (gitignored).
 - At startup (in DEBUG mode), the app loads this file and sets environment variables.
 - Never commit real secrets—only commit `secrets.local.csv.template` with placeholders.
- **Production/Server:**
 - Set environment variables (`EGRANTS_DB_USER`, `EGRANTS_DB_PASSWORD`, etc.) at the OS or job scheduler level.
 - Do not use a secrets file in production.

## Logging
- Uses Serilog for structured logging (file and console, daily rolling logs, retention, etc.).
- Logging configuration is handled in `CommonUtilities`.

## Testing
- Integration tests load secrets from `secrets.local.csv`.
- Tests assert on values from environment variables, not hardcoded credentials.

## Best Practices
- Never hardcode or commit secrets.
- Use environment variables for all secrets.
- Use a gitignored secrets file for local dev, and load it at startup.
- Reference secrets in config using environment variable syntax.
- Provide a template secrets file for onboarding.
- Use secret scanning tools in your CI pipeline.

## Onboarding Steps
1. Copy `secrets.local.csv.template` to `secrets.local.csv` and fill in your credentials.
2. Build and run the solution in DEBUG mode for local development.
3. For production/server, set environment variables and do not deploy `secrets.local.csv`.

## Migration Notes
- All legacy VBS scripts have been refactored to C# with improved error handling, logging, and maintainability.
- Shared logic is centralized in the `CommonUtilities` project.
- All projects target .NET8 and use modern C# features.

---

For more details, see inline documentation in each project and the `CommonUtilities` class.
