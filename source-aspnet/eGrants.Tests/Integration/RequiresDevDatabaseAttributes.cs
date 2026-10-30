using System;
using System.IO;
using System.Net.Sockets;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

using Xunit;

namespace eGrants.Tests.Integration
{
    /// <summary>
    /// Single source of truth for the SQL Server that the live-database integration tests
    /// target. The connection string is loaded from the eGrants web project's
    /// appsettings files for the current environment, exactly like Program.cs does, so the
    /// tests always hit the same server the app itself would use for that environment
    /// (dev, test, stage, or production).
    /// </summary>
    public static class TestDatabase
    {
        /// <summary>
        /// Connection string used by every live-database integration test, resolved from
        /// appsettings.json + appsettings.{Environment}.json in the eGrants web project.
        ///
        /// The environment is taken from ASPNETCORE_ENVIRONMENT / DOTNET_ENVIRONMENT and
        /// defaults to "Development". The {DB_USER}/{DB_PASSWORD} placeholders are filled
        /// from configuration (typically environment variables), mirroring Program.cs.
        /// </summary>
        public static string ConnectionString { get; } = LoadConnectionString();

        private static string LoadConnectionString()
        {
            var environment =
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                ?? "Development";

            var projectDir = FindWebProjectDirectory()
                ?? throw new InvalidOperationException(
                    "Could not locate the eGrants web project directory to load appsettings.");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(projectDir)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var raw = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "ConnectionStrings:DefaultConnection was not found in the eGrants appsettings.");

            // Mirror Program.cs: substitute the credential placeholders from configuration.
            var user = configuration["DB_USER"] ?? string.Empty;
            var password = configuration["DB_PASSWORD"] ?? string.Empty;

            return raw
                .Replace("{DB_USER}", user)
                .Replace("{DB_PASSWORD}", password);
        }

        /// <summary>
        /// Walks up from the test assembly location to locate the eGrants web project
        /// directory (the folder containing eGrants.csproj and appsettings.json).
        /// </summary>
        private static string? FindWebProjectDirectory()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);

            while (dir is not null)
            {
                var candidate = Path.Combine(dir.FullName, "eGrants");
                if (File.Exists(Path.Combine(candidate, "eGrants.csproj")) &&
                    File.Exists(Path.Combine(candidate, "appsettings.json")))
                {
                    return candidate;
                }

                if (File.Exists(Path.Combine(dir.FullName, "eGrants.csproj")) &&
                    File.Exists(Path.Combine(dir.FullName, "appsettings.json")))
                {
                    return dir.FullName;
                }

                dir = dir.Parent;
            }

            return null;
        }
    }

    /// <summary>
    /// Reachability probe for the SQL Server used by the live-database integration tests.
    /// The host/port are parsed from <see cref="TestDatabase.ConnectionString"/>, so the
    /// probe checks whichever environment the tests are actually pointed at (dev, test,
    /// stage, or production).
    ///
    /// The probe runs at most once per test process and the result is cached, so
    /// discovering many DB tests does not repeatedly pay the connect cost. When the server
    /// cannot be reached (e.g. off the NCI network / VPN, or on a build agent) the
    /// associated tests are reported as SKIPPED instead of blocking the entire run for
    /// ~45 seconds each.
    /// </summary>
    internal static class DevDatabaseAvailability
    {
        // Keep the probe short so an unreachable server fails fast rather than hanging.
        private static readonly TimeSpan ProbeTimeout = TimeSpan.FromSeconds(3);

        private static readonly Lazy<string?> LazySkipReason = new(Probe);

        /// <summary>
        /// Null when the target database is reachable; otherwise a human-readable skip reason.
        /// </summary>
        public static string? SkipReason => LazySkipReason.Value;

        private static string? Probe()
        {
            string host;
            int port;

            try
            {
                (host, port) = ResolveHostAndPort(TestDatabase.ConnectionString);
            }
            catch (Exception ex)
            {
                return $"Skipped: could not parse the test database connection string ({ex.GetType().Name}).";
            }

            try
            {
                using var client = new TcpClient();
                var connectTask = client.ConnectAsync(host, port);

                if (!connectTask.Wait(ProbeTimeout) || !client.Connected)
                {
                    return $"Skipped: test database {host}:{port} is not reachable.";
                }

                return null;
            }
            catch (Exception ex)
            {
                return $"Skipped: test database {host}:{port} is not reachable ({ex.GetType().Name}).";
            }
        }

        /// <summary>
        /// Extracts the TCP host and port from a SQL Server connection string's Data Source,
        /// which may be of the form "host\instance,port", "host,port", "host\instance", or
        /// "host". When no explicit port is present, the default SQL Server port (1433) is
        /// assumed for the reachability check.
        /// </summary>
        private static (string Host, int Port) ResolveHostAndPort(string connectionString)
        {
            const int DefaultSqlPort = 1433;

            var dataSource = new SqlConnectionStringBuilder(connectionString).DataSource ?? string.Empty;

            // Strip an optional "tcp:" prefix.
            if (dataSource.StartsWith("tcp:", StringComparison.OrdinalIgnoreCase))
            {
                dataSource = dataSource.Substring(4);
            }

            var port = DefaultSqlPort;

            // "host\instance,port" or "host,port" -> split off the port.
            var commaIndex = dataSource.LastIndexOf(',');
            if (commaIndex >= 0)
            {
                var portText = dataSource.Substring(commaIndex + 1).Trim();
                if (int.TryParse(portText, out var parsedPort))
                {
                    port = parsedPort;
                }

                dataSource = dataSource.Substring(0, commaIndex);
            }

            // Drop any "\instance" suffix; only the host is needed for a TCP probe.
            var backslashIndex = dataSource.IndexOf('\\');
            if (backslashIndex >= 0)
            {
                dataSource = dataSource.Substring(0, backslashIndex);
            }

            var host = dataSource.Trim();
            if (host.Length == 0 || host == ".")
            {
                host = "localhost";
            }

            return (host, port);
        }
    }

    /// <summary>
    /// A <see cref="FactAttribute"/> that is automatically skipped when the target test
    /// database is not reachable. Use in place of <see cref="FactAttribute"/> for tests
    /// that hit the live database.
    /// </summary>
    public sealed class DbFactAttribute : FactAttribute
    {
        public DbFactAttribute()
        {
            Skip = DevDatabaseAvailability.SkipReason;
        }
    }

    /// <summary>
    /// A <see cref="TheoryAttribute"/> that is automatically skipped when the target test
    /// database is not reachable. Use in place of <see cref="TheoryAttribute"/> for tests
    /// that hit the live database.
    /// </summary>
    public sealed class DbTheoryAttribute : TheoryAttribute
    {
        public DbTheoryAttribute()
        {
            Skip = DevDatabaseAvailability.SkipReason;
        }
    }
}
