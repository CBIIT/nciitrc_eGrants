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
        /// Sets default test credentials if environment variables are not already set.
        /// 
        /// NOTE: Some tests require actual database access and will fail with test credentials.
        /// These are integration tests that should be run with valid credentials:
        /// - Set DB_USER and DB_PASSWORD environment variables for integration tests
        /// - Or run with filter: --filter "TestCategory!=Integration"
        /// </summary>
        [AssemblyInitialize]
        public static void Initialize(TestContext context)
        {
            // Check if environment variables are already set
            var dbUser = Environment.GetEnvironmentVariable("DB_USER");
            var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");

            if (string.IsNullOrEmpty(dbUser))
            {
                // Set default test credentials for unit tests
                // Integration tests that need database access will fail with these credentials
                Console.WriteLine("WARNING: DB_USER not set - using default test credentials");
                Console.WriteLine("Integration tests requiring database access will fail.");
                Console.WriteLine("To run integration tests, set environment variables:");
                Console.WriteLine("  [System.Environment]::SetEnvironmentVariable('DB_USER', 'your_user', 'User')");
                Console.WriteLine("  [System.Environment]::SetEnvironmentVariable('DB_PASSWORD', 'your_pass', 'User')");

                Environment.SetEnvironmentVariable("DB_USER", "test_user");
                Environment.SetEnvironmentVariable("DB_PASSWORD", "test_password");
            }
            else
            {
                Console.WriteLine($"Using configured credentials: DB_USER={dbUser}");
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
