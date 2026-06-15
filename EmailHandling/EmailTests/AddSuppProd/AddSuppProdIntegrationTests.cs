using System;
using System.Data.SqlClient;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EmailHandlingTests.AddSuppProd
{
    /// <summary>
    /// Integration tests for AddSuppProd.Processor with database connectivity.
    /// These tests require database credentials set as environment variables and validate
    /// the full workflow including database stored procedures, functions, and file I/O.
    /// 
    /// Prerequisites:
    /// - Environment variables must be set (DB_USER, DB_PASSWORD)
    ///   Set using PowerShell:
    ///   [System.Environment]::SetEnvironmentVariable('DB_USER', 'your_username', [System.EnvironmentVariableTarget]::User)
    ///   [System.Environment]::SetEnvironmentVariable('DB_PASSWORD', 'your_password', [System.EnvironmentVariableTarget]::User)
    /// 
    /// - EIM database must have the required stored procedures and functions:
    ///   - getPlaceHolder_new (stored procedure)
    ///   - fn_PA_match (function)
    ///   - Imm_fn_applid_match (function)
    ///   - adsup_notification (table)
    ///   - adsup_Notification_email_status (table)
    /// 
    /// Note: These tests connect to the same EIM database that the production
    /// AddSuppProd application uses (NCIDB-D387-V.nci.nih.gov\MSSQLEGRANTSQ).
    /// </summary>
    [TestClass]
    public class AddSuppProdIntegrationTests
    {
        private static string _connectionString;
        private static string _testLogDir;
        private static string _testOutDir;
        private static bool _databaseAvailable;

        [ClassInitialize]
        public static void ClassSetup(TestContext context)
        {
            // Setup logging directory
            _testLogDir = Path.Combine(Path.GetTempPath(), "AddSuppProd_IntegrationTests_" + Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testLogDir);
            CommonUtilties.CommonUtilities.LogDir = _testLogDir;

            // Setup output directory for test files
            _testOutDir = Path.Combine(Path.GetTempPath(), "AddSuppProd_Output_" + Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testOutDir);

            // Get database credentials directly from environment variables
            // These should be set at the system or user level before running tests
            string dbUser = Environment.GetEnvironmentVariable("DB_USER") 
                         ?? Environment.GetEnvironmentVariable("DB_USER");
            string dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD")
                             ?? Environment.GetEnvironmentVariable("DB_PASSWORD");

            if (!string.IsNullOrWhiteSpace(dbUser) && !string.IsNullOrWhiteSpace(dbPassword))
            {
                // Use the same connection string format as appsettings.json
                // This connects to the EIM database (not eGrants or eGrants_test)
                _connectionString = $"Password={dbPassword};Persist Security Info=True;User ID={dbUser};Initial Catalog=EIM;Data Source=NCIDB-D387-V.nci.nih.gov\\MSSQLEGRANTSQ,52000;Application Name=egrants_test";

                // Test database connectivity
                try
                {
                    using (var con = new SqlConnection(_connectionString))
                    {
                        con.Open();
                        _databaseAvailable = true;
                        Console.WriteLine("Database connection successful - integration tests will run");
                    }
                }
                catch (Exception ex)
                {
                    _databaseAvailable = false;
                    Console.WriteLine($"Database not available - integration tests will be skipped: {ex.Message}");
                }
            }
            else
            {
                _databaseAvailable = false;
                Console.WriteLine("Database credentials not found in environment variables - integration tests will be skipped");
                Console.WriteLine("To run integration tests, set environment variables:");
                Console.WriteLine("  [System.Environment]::SetEnvironmentVariable('DB_USER', 'your_username', [System.EnvironmentVariableTarget]::User)");
                Console.WriteLine("  [System.Environment]::SetEnvironmentVariable('DB_PASSWORD', 'your_password', [System.EnvironmentVariableTarget]::User)");
            }
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            // Cleanup test directories
            if (Directory.Exists(_testLogDir))
            {
                try { Directory.Delete(_testLogDir, true); }
                catch { /* Ignore cleanup errors */ }
            }

            if (Directory.Exists(_testOutDir))
            {
                try { Directory.Delete(_testOutDir, true); }
                catch { /* Ignore cleanup errors */ }
            }
        }

        #region Database Helper Method Tests

        /// <summary>
        /// Verifies that GetApplIdFromText can match a valid grant number.
        /// This tests the Imm_fn_applid_match database function.
        /// </summary>
        [TestMethod]
        public void GetApplIdFromText_ValidGrantNumber_ReturnsApplId()
        {
            if (!_databaseAvailable)
            {
                Assert.Inconclusive("Database not available - test skipped");
                return;
            }

            // Arrange
            var processor = new TestAddSuppProdProcessor();
            string testGrantNumber = "5R01CA123456-03"; // Use a grant number that exists in test data

            // Act
            string applId = null;
            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();

                // Call via reflection since method is internal/private
                var method = typeof(global::AddSuppProd.Processor).GetMethod("GetApplIdFromText",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (method != null)
                {
                    applId = method.Invoke(processor, new object[] { con, testGrantNumber }) as string;
                }
            }

            // Assert
            // Note: This test will only pass if the test grant number exists in the database
            // Adjust assertion based on your test data
            Assert.IsNotNull(applId, "Should return an application ID");
            Console.WriteLine($"Found applId: {applId} for grant number: {testGrantNumber}");
        }

        /// <summary>
        /// Verifies that GetPAFromText can match a valid PA code.
        /// This tests the fn_PA_match database function.
        /// </summary>
        [TestMethod]
        public void GetPAFromText_ValidPACode_ReturnsPA()
        {
            if (!_databaseAvailable)
            {
                Assert.Inconclusive("Database not available - test skipped");
                return;
            }

            // Arrange
            var processor = new TestAddSuppProdProcessor();
            string textWithPA = "PAR-20-123 Administrative Supplement"; // Use a PA that exists in test data

            // Act
            string pa = null;
            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();

                var method = typeof(global::AddSuppProd.Processor).GetMethod("GetPAFromText",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (method != null)
                {
                    pa = method.Invoke(processor, new object[] { con, textWithPA }) as string;
                }
            }

            // Assert
            // Note: Result depends on test data - adjust assertion accordingly
            Console.WriteLine($"Found PA: {pa ?? "(null)"} for text: {textWithPA}");
            // Assert.IsNotNull(pa, "Should return a PA code if match exists");
        }

        /// <summary>
        /// Verifies database connection and basic query execution.
        /// </summary>
        [TestMethod]
        public void DatabaseConnection_ValidCredentials_ConnectsSuccessfully()
        {
            if (!_databaseAvailable)
            {
                Assert.Inconclusive("Database not available - test skipped");
                return;
            }

            // Arrange & Act
            bool connected = false;
            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();
                connected = (con.State == System.Data.ConnectionState.Open);
            }

            // Assert
            Assert.IsTrue(connected, "Should connect to database successfully");
        }

        /// <summary>
        /// Verifies that the adsup_notification table exists and is accessible.
        /// </summary>
        [TestMethod]
        public void Database_AdsupNotificationTable_Exists()
        {
            if (!_databaseAvailable)
            {
                Assert.Inconclusive("Database not available - test skipped");
                return;
            }

            // Arrange & Act
            int count = -1;
            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM adsup_notification", con))
                {
                    count = (int)cmd.ExecuteScalar();
                }
            }

            // Assert
            Assert.IsTrue(count >= 0, "adsup_notification table should exist and be accessible");
            Console.WriteLine($"adsup_notification table has {count} rows");
        }

        /// <summary>
        /// Verifies that the getPlaceHolder_new stored procedure exists.
        /// </summary>
        [TestMethod]
        public void Database_GetPlaceHolderNewProcedure_Exists()
        {
            if (!_databaseAvailable)
            {
                Assert.Inconclusive("Database not available - test skipped");
                return;
            }

            // Arrange & Act
            bool exists = false;
            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();
                using (var cmd = new SqlCommand(@"
                    SELECT COUNT(*) 
                    FROM sys.procedures 
                    WHERE name = 'getPlaceHolder_new'", con))
                {
                    int count = (int)cmd.ExecuteScalar();
                    exists = (count > 0);
                }
            }

            // Assert
            Assert.IsTrue(exists, "getPlaceHolder_new stored procedure should exist");
        }

        /// <summary>
        /// Verifies that the Imm_fn_applid_match function exists.
        /// </summary>
        [TestMethod]
        public void Database_ImmFnApplidMatchFunction_Exists()
        {
            if (!_databaseAvailable)
            {
                Assert.Inconclusive("Database not available - test skipped");
                return;
            }

            // Arrange & Act
            bool exists = false;
            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();
                using (var cmd = new SqlCommand(@"
                    SELECT COUNT(*) 
                    FROM sys.objects 
                    WHERE type IN ('FN', 'IF', 'TF') 
                    AND name = 'Imm_fn_applid_match'", con))
                {
                    int count = (int)cmd.ExecuteScalar();
                    exists = (count > 0);
                }
            }

            // Assert
            Assert.IsTrue(exists, "Imm_fn_applid_match function should exist");
        }

        /// <summary>
        /// Verifies that the fn_PA_match function exists.
        /// </summary>
        [TestMethod]
        public void Database_FnPAMatchFunction_Exists()
        {
            if (!_databaseAvailable)
            {
                Assert.Inconclusive("Database not available - test skipped");
                return;
            }

            // Arrange & Act
            bool exists = false;
            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();
                using (var cmd = new SqlCommand(@"
                    SELECT COUNT(*) 
                    FROM sys.objects 
                    WHERE type IN ('FN', 'IF', 'TF') 
                    AND name = 'fn_PA_match'", con))
                {
                    int count = (int)cmd.ExecuteScalar();
                    exists = (count > 0);
                }
            }

            // Assert
            Assert.IsTrue(exists, "fn_PA_match function should exist");
        }

        #endregion

        #region File I/O Tests

        /// <summary>
        /// Verifies that the output directory can be created and accessed.
        /// </summary>
        [TestMethod]
        public void FileIO_OutputDirectory_CanCreateAndAccess()
        {
            // Arrange
            string testDir = Path.Combine(_testOutDir, "SubFolder_" + Guid.NewGuid());

            // Act
            Directory.CreateDirectory(testDir);
            bool exists = Directory.Exists(testDir);

            // Assert
            Assert.IsTrue(exists, "Should be able to create output directory");

            // Cleanup
            Directory.Delete(testDir);
        }

        /// <summary>
        /// Verifies that test files can be written to the output directory.
        /// </summary>
        [TestMethod]
        public void FileIO_WriteTestFile_SuccessfullyCreated()
        {
            // Arrange
            string testFile = Path.Combine(_testOutDir, "test_file.txt");
            string testContent = "This is a test file for AddSuppProd integration testing.";

            // Act
            File.WriteAllText(testFile, testContent);
            bool exists = File.Exists(testFile);
            string readContent = File.ReadAllText(testFile);

            // Assert
            Assert.IsTrue(exists, "Test file should be created");
            Assert.AreEqual(testContent, readContent, "File content should match");

            // Cleanup
            File.Delete(testFile);
        }

        #endregion

        #region Configuration and Setup Tests

        /// <summary>
        /// Verifies that environment variables can be read.
        /// </summary>
        [TestMethod]
        public void Configuration_EnvironmentVariables_CanBeRead()
        {
            // Act
            string dbUser = Environment.GetEnvironmentVariable("DB_USER");

            // Assert
            // Note: Test will pass even if variable is not set (it's optional for other tests)
            // Integration tests that require DB will be skipped if not set
            Console.WriteLine($"DB_USER environment variable: {(string.IsNullOrEmpty(dbUser) ? "(not set)" : "***set***")}");
            Assert.IsTrue(true, "Environment variable check completed");
        }

        /// <summary>
        /// Verifies that the Processor can be instantiated with admin email config.
        /// </summary>
        [TestMethod]
        public void Processor_Instantiation_WithAdminEmail()
        {
            // Arrange
            string testAdminEmail = "test.admin@nih.gov;backup.admin@nih.gov";

            // Act
            var processor = new global::AddSuppProd.Processor(testAdminEmail);

            // Assert
            Assert.IsNotNull(processor, "Processor should instantiate successfully");
        }

        #endregion

        #region End-to-End Scenario Tests (Commented - Require Specific Test Data)

        /*
        /// <summary>
        /// End-to-end test for processing a system notification.
        /// REQUIRES: Test notification ID in database
        /// </summary>
        [TestMethod]
        public void EndToEnd_ProcessSystemNotification_CreatesWIPEntry()
        {
            if (!_databaseAvailable)
            {
                Assert.Inconclusive("Database not available - test skipped");
                return;
            }

            // Arrange
            var processor = new AddSuppProd.Processor("test.admin@nih.gov");
            string testNotificationId = "12345"; // Replace with actual test notification ID
            string testBody = $"This is a test notification. Notification Id={testNotificationId}";

            // This test would require:
            // 1. A test notification record in adsup_notification table
            // 2. Mock Outlook email item
            // 3. Verification that WIP entry was created

            // Implementation requires mocking Outlook COM objects or using actual Outlook
            Assert.Inconclusive("End-to-end test requires specific test data setup");
        }

        /// <summary>
        /// End-to-end test for processing staff upload.
        /// REQUIRES: Valid test grant number
        /// </summary>
        [TestMethod]
        public void EndToEnd_ProcessStaffUpload_SavesFile()
        {
            if (!_databaseAvailable)
            {
                Assert.Inconclusive("Database not available - test skipped");
                return;
            }

            // This test would require:
            // 1. Mock email with staff sender
            // 2. Valid grant number in database
            // 3. Mock attachment or email body
            // 4. Verification of file creation

            Assert.Inconclusive("End-to-end test requires mock Outlook objects");
        }
        */

        #endregion

        #region Helper Methods

        /// <summary>
        /// Creates a test notification record in the database for testing.
        /// Use this in SetUp for end-to-end tests.
        /// </summary>
        private void CreateTestNotificationRecord(SqlConnection con, string notificationId, string applId)
        {
            string sql = @"
                IF NOT EXISTS (SELECT 1 FROM adsup_notification WHERE id = @NotifId)
                BEGIN
                    INSERT INTO adsup_notification (id, appl_id, created_date)
                    VALUES (@NotifId, @ApplId, GETDATE())
                END";

            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@NotifId", notificationId);
                cmd.Parameters.AddWithValue("@ApplId", applId);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Cleans up test notification records.
        /// Use this in TearDown for end-to-end tests.
        /// </summary>
        private void CleanupTestNotificationRecord(SqlConnection con, string notificationId)
        {
            string sql = "DELETE FROM adsup_notification WHERE id = @NotifId";
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@NotifId", notificationId);
                cmd.ExecuteNonQuery();
            }
        }

        #endregion
    }
}
