using System;

namespace eGrants.Tests.Integration
{
    /// <summary>
    /// Provides a single, process-wide <see cref="SmokeTestWebApplicationFactory"/> instance.
    ///
    /// WHY THIS EXISTS:
    /// The application calls <c>AddSystemWebAdapters()</c> in Program.cs, which registers
    /// <c>System.Web.Hosting.HostingEnvironmentAccessor</c>. That accessor sets a
    /// PROCESS-GLOBAL "Current" hosting environment and throws
    /// "Hosting environment is already set" if a second host is started in the same process
    /// (the global is not reset on host dispose).
    ///
    /// Because of that, only ONE in-process host may exist for the lifetime of the test
    /// process. Individually a smoke test class works, but when the whole suite runs the
    /// separate per-class fixtures (and route-discovery factories) each try to boot their own
    /// host, and every host after the first fails.
    ///
    /// Sharing this single lazily-created factory across every smoke test class and every
    /// [MemberData] discovery method guarantees exactly one host per process. The instance is
    /// intentionally never disposed; it lives until the test process exits.
    /// </summary>
    public static class SmokeTestHost
    {
        private static readonly Lazy<SmokeTestWebApplicationFactory> LazyFactory =
            new(() => new SmokeTestWebApplicationFactory());

        public static SmokeTestWebApplicationFactory Factory => LazyFactory.Value;
    }
}
