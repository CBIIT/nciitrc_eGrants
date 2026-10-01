using System.Collections.Generic;
using System.Linq;

using eGrants.DAL;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace eGrants.Tests.Integration
{
    /// <summary>
    /// Boots the eGrants application in-memory for integration smoke tests.
    ///
    /// Two things make the smoke tests self-contained:
    ///   1. TestAuth:Enabled is set, which activates the test-auth seam in Program.cs and
    ///      seeds a fully validated session (the "fake auth handler" for this session-based
    ///      app), skipping SiteMinder / database / GitHub driven session initialization.
    ///   2. The SQL Server AppDbContext registration is replaced with the EF Core in-memory
    ///      provider so the tests do not require a live database.
    /// </summary>
    public class SmokeTestWebApplicationFactory : WebApplicationFactory<Program>
    {
        // Set to true ONLY when you intentionally want tests to send real exception emails
        // via the Serilog Email sink configured in appsettings.json. Leave false normally.
        private const bool SendEmailsFromTests = false;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");

            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["TestAuth:Enabled"] = "true",
                    ["TestAuth:UserId"] = "testuser",
                    ["TestAuth:Ic"] = "NCI",
                    // Provide placeholders so the connection-string assembly in Program.cs
                    // does not fail; the context is replaced with the in-memory provider below.
                    ["ConnectionStrings:DefaultConnection"] = "Server=(localdb);Database=egrants_test;Trusted_Connection=True;",
                    ["DB_USER"] = "test",
                    ["DB_PASSWORD"] = "test",
                });
            });

            builder.ConfigureServices(services =>
            {
                // Remove the app's SQL Server AppDbContext registration.
                var descriptors = services
                    .Where(d =>
                        d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                        d.ServiceType == typeof(AppDbContext))
                    .ToList();

                foreach (var descriptor in descriptors)
                {
                    services.Remove(descriptor);
                }

                // Register an in-memory database so tests run without SQL Server.
                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase("egrants_smoke_tests"));
            });
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            var host = base.CreateHost(builder);

            // Program.cs wires a process-wide Serilog Email sink that emails on every Error log.
            // Tests exercise failure paths that call Log.Error(...), so unless explicitly enabled
            // we replace the static logger with console-only to guarantee tests never send email.
            if (!SendEmailsFromTests)
            {
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .WriteTo.Console()
                    .CreateLogger();
            }

            return host;
        }
    }
}
