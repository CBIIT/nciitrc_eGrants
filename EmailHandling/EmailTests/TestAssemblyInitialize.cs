using CommonUtilties;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace EmailHandlingTests
{
    /// <summary>
    /// Assembly-level test initialization.
    /// Sets up environment variables needed for database connection strings.
    /// </summary>
    [TestClass]
    public class TestAssemblyInitialize
    {
        /// <summary>
        /// Runs once before any tests in the assembly.
        /// Loads credentials from secrets.local.csv (not committed to source control).
        /// </summary>
        [AssemblyInitialize]
        public static void Initialize(TestContext context)
        {
            // Try to load secrets from local file (not committed to source control)
            if (!CommonUtilities.LoadLocalSecrets("secrets.local.csv"))
            {
                // Fall back to checking if environment variables are already set
                var dbUser = Environment.GetEnvironmentVariable("EGRANTS_DB_USER");
                if (string.IsNullOrEmpty(dbUser))
                {
                    throw new InvalidOperationException(
                        "Database credentials not configured.\n" +
                        "Please either:\n" +
                        "1. Copy 'secrets.local.csv.template' to 'secrets.local.csv' and fill in credentials, OR\n" +
                        "2. Set EGRANTS_DB_USER and EGRANTS_DB_PASSWORD environment variables.");
                }
            }
        }

        /// <summary>
        /// Runs once after all tests in the assembly have completed.
        /// </summary>
        [AssemblyCleanup]
        public static void Cleanup()
        {
            CommonUtilities.CloseLogging();
        }
    }
}
