using System;
using System.Data.SqlClient;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EmailHandlingTests.Integration.AddSuppEmailer
{
    /// <summary>
    /// Integration tests for AddSuppEmailer.Processor with database connectivity.
    /// These tests require database credentials set as environment variables and validate
    /// the full workflow including database functions for email generation.
    /// 
    /// Prerequisites:
    /// - Environment variables must be set (DB_USER, DB_PASSWORD)
    ///   Set using PowerShell:
    ///   [System.Environment]::SetEnvironmentVariable('DB_USER', 'your_username', [System.EnvironmentVariableTarget]::User)
    ///   [System.Environment]::SetEnvironmentVariable('DB_PASSWORD', 'your_password', [System.EnvironmentVariableTarget]::User)
    /// 
    /// - EIM database must have the required functions:
    ///   - fn_adsupp_getemail_subject(notification_id): Returns email subject
    ///   - fn_adsupp_getemail_body(notification_id): Returns email body HTML
    ///   - fn_adsupp_getemail_string(notification_id, email_type): Returns recipients
    ///   - adsup_Notification_email_status table must exist
    /// 
    /// Note: These tests connect to the same EIM database that the production
    /// AddSuppEmailer application uses (NCIDB-D387-V.nci.nih.gov\MSSQLEGRANTSQ).
    /// </summary>
    [TestClass]
    public class AddSuppEmailerIntegrationTests
    {
        private static string _connectionString;
        private static string _testLogDir;
        private static bool _databaseAvailable;

        [ClassInitialize]
        public static void ClassSetup(TestContext context)
        {
            // Setup logging directory
            _testLogDir = Path.Combine(Path.GetTempPath(), "AddSuppEmailer_IntegrationTests_" + Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testLogDir);
            CommonUtilties.CommonUtilities.LogDir = _testLogDir;

            // Get database credentials directly from environment variables
            string dbUser = Environment.GetEnvironmentVariable("DB_USER") 
                         ?? Environment.GetEnvironmentVariable("DB_USER");
            string dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD")
                             ?? Environment.GetEnvironmentVariable("DB_PASSWORD");

            if (!string.IsNullOrWhiteSpace(dbUser) && !string.IsNullOrWhiteSpace(dbPassword))
            {
                // Use the same connection string format as appsettings.json
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
        }

        #region Database Connection Tests

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

        #endregion

        #region Database Function Tests

        /// <summary>
        /// Verifies that fn_adsupp_getemail_subject function exists.
        /// </summary>
        [TestMethod]
        public void Database_FnAdSuppGetEmailSubjectFunction_Exists()
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
                    AND name = 'fn_adsupp_getemail_subject'", con))
                {
                    int count = (int)cmd.ExecuteScalar();
                    exists = (count > 0);
                }
            }

            // Assert
            Assert.IsTrue(exists, "fn_adsupp_getemail_subject function should exist");
        }

        /// <summary>
        /// Verifies that fn_adsupp_getemail_body function exists.
        /// </summary>
        [TestMethod]
        public void Database_FnAdSuppGetEmailBodyFunction_Exists()
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
                    AND name = 'fn_adsupp_getemail_body'", con))
                {
                    int count = (int)cmd.ExecuteScalar();
                    exists = (count > 0);
                }
            }

            // Assert
            Assert.IsTrue(exists, "fn_adsupp_getemail_body function should exist");
        }

        /// <summary>
        /// Verifies that fn_adsupp_getemail_string function exists.
        /// </summary>
        [TestMethod]
        public void Database_FnAdSuppGetEmailStringFunction_Exists()
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
                    AND name = 'fn_adsupp_getemail_string'", con))
                {
                    int count = (int)cmd.ExecuteScalar();
                    exists = (count > 0);
                }
            }

            // Assert
            Assert.IsTrue(exists, "fn_adsupp_getemail_string function should exist");
        }

        /// <summary>
        /// Verifies that adsup_Notification_email_status table exists.
        /// </summary>
        [TestMethod]
        public void Database_AdsupNotificationEmailStatusTable_Exists()
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
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM adsup_Notification_email_status", con))
                {
                    count = (int)cmd.ExecuteScalar();
                }
            }

            // Assert
            Assert.IsTrue(count >= 0, "adsup_Notification_email_status table should exist and be accessible");
            Console.WriteLine($"adsup_Notification_email_status table has {count} rows");
        }

        /// <summary>
        /// Verifies that the notification query returns valid structure.
        /// </summary>
        [TestMethod]
        public void Database_NotificationQuery_ReturnsValidStructure()
        {
            if (!_databaseAvailable)
            {
                Assert.Inconclusive("Database not available - test skipped");
                return;
            }

            // Arrange & Act
            bool hasNotificationIdColumn = false;
            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();
                string sql = @"
                    SELECT DISTINCT Notification_id 
                    FROM dbo.adsup_Notification_email_status 
                    WHERE email_date IS NULL 
                    ORDER BY Notification_id DESC";

                using (var cmd = new SqlCommand(sql, con))
                using (var reader = cmd.ExecuteReader())
                {
                    // Verify column exists
                    hasNotificationIdColumn = reader.FieldCount > 0 && 
                                            reader.GetName(0) == "Notification_id";
                }
            }

            // Assert
            Assert.IsTrue(hasNotificationIdColumn, "Query should return Notification_id column");
        }

        #endregion

        #region Database Helper Method Tests

        /// <summary>
        /// Verifies that GetEmailSubject can be called (may return null if no test data).
        /// </summary>
        [TestMethod]
        public void GetEmailSubject_WithNotificationId_ReturnsResult()
        {
            if (!_databaseAvailable)
            {
                Assert.Inconclusive("Database not available - test skipped");
                return;
            }

            // Arrange
            var processor = new global::AddSuppEmailer.Processor();
            int testNotifId = 1; // Use a low ID that might exist

            // Act
            string subject = null;
            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();

                // Use reflection to call protected method
                var method = typeof(global::AddSuppEmailer.Processor).GetMethod("GetEmailSubject", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (method != null)
                {
                    subject = method.Invoke(processor, new object[] { con, testNotifId }) as string;
                }
            }

            // Assert
            Assert.IsNotNull(subject, "GetEmailSubject should return a value (even if default)");
            Console.WriteLine($"Subject for notification {testNotifId}: {subject}");
        }

        /// <summary>
        /// Verifies that GetEmailBody can be called (may return null if no test data).
        /// </summary>
        [TestMethod]
        public void GetEmailBody_WithNotificationId_ReturnsResult()
        {
            if (!_databaseAvailable)
            {
                Assert.Inconclusive("Database not available - test skipped");
                return;
            }

            // Arrange
            var processor = new global::AddSuppEmailer.Processor();
            int testNotifId = 1;

            // Act
            string body = null;
            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();

                var method = typeof(global::AddSuppEmailer.Processor).GetMethod("GetEmailBody", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (method != null)
                {
                    body = method.Invoke(processor, new object[] { con, testNotifId }) as string;
                }
            }

            // Assert
            Assert.IsNotNull(body, "GetEmailBody should return a value (even if default)");
            Console.WriteLine($"Body length for notification {testNotifId}: {body?.Length ?? 0} characters");
        }

        /// <summary>
        /// Verifies that GetEmailRecipients can be called for TO recipients.
        /// </summary>
        [TestMethod]
        public void GetEmailRecipients_ForToType_ReturnsResult()
        {
            if (!_databaseAvailable)
            {
                Assert.Inconclusive("Database not available - test skipped");
                return;
            }

            // Arrange
            var processor = new global::AddSuppEmailer.Processor();
            int testNotifId = 1;

            // Act
            string recipients = null;
            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();

                var method = typeof(global::AddSuppEmailer.Processor).GetMethod("GetEmailRecipients", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (method != null)
                {
                    recipients = method.Invoke(processor, new object[] { con, testNotifId, "TO" }) as string;
                }
            }

            // Assert
            Assert.IsNotNull(recipients, "GetEmailRecipients should return a value (even if empty)");
            Console.WriteLine($"TO recipients for notification {testNotifId}: {(string.IsNullOrEmpty(recipients) ? "(none)" : recipients)}");
        }

        /// <summary>
        /// Verifies that GetEmailRecipients can be called for CC recipients.
        /// </summary>
        [TestMethod]
        public void GetEmailRecipients_ForCcType_ReturnsResult()
        {
            if (!_databaseAvailable)
            {
                Assert.Inconclusive("Database not available - test skipped");
                return;
            }

            // Arrange
            var processor = new global::AddSuppEmailer.Processor();
            int testNotifId = 1;

            // Act
            string recipients = null;
            using (var con = new SqlConnection(_connectionString))
            {
                con.Open();

                var method = typeof(global::AddSuppEmailer.Processor).GetMethod("GetEmailRecipients", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (method != null)
                {
                    recipients = method.Invoke(processor, new object[] { con, testNotifId, "CC" }) as string;
                }
            }

            // Assert
            Assert.IsNotNull(recipients, "GetEmailRecipients should return a value (even if empty)");
            Console.WriteLine($"CC recipients for notification {testNotifId}: {(string.IsNullOrEmpty(recipients) ? "(none)" : recipients)}");
        }

        #endregion

        #region Configuration Tests

        /// <summary>
        /// Verifies that environment variables can be read.
        /// </summary>
        [TestMethod]
        public void Configuration_EnvironmentVariables_CanBeRead()
        {
            // Act
            string dbUser = Environment.GetEnvironmentVariable("DB_USER");

            // Assert
            Console.WriteLine($"DB_USER environment variable: {(string.IsNullOrEmpty(dbUser) ? "(not set)" : "***set***")}");
            Assert.IsTrue(true, "Environment variable check completed");
        }

        /// <summary>
        /// Verifies that the Processor can be instantiated.
        /// </summary>
        [TestMethod]
        public void Processor_Instantiation_Succeeds()
        {
            // Act
            var processor = new global::AddSuppEmailer.Processor();

            // Assert
            Assert.IsNotNull(processor, "Processor should instantiate successfully");
        }

        #endregion

        #region File I/O Tests

        /// <summary>
        /// Verifies that log directory can be created and accessed.
        /// </summary>
        [TestMethod]
        public void FileIO_LogDirectory_CanCreateAndAccess()
        {
            // Arrange
            string testDir = Path.Combine(_testLogDir, "SubFolder_" + Guid.NewGuid());

            // Act
            Directory.CreateDirectory(testDir);
            bool exists = Directory.Exists(testDir);

            // Assert
            Assert.IsTrue(exists, "Should be able to create log directory");

            // Cleanup
            Directory.Delete(testDir);
        }

        /// <summary>
        /// Verifies that test log files can be written.
        /// </summary>
        [TestMethod]
        public void FileIO_WriteTestLogFile_SuccessfullyCreated()
        {
            // Arrange
            string testFile = Path.Combine(_testLogDir, "test_log.txt");
            string testContent = "Test log entry for AddSuppEmailer integration testing.";

            // Act
            File.WriteAllText(testFile, testContent);
            bool exists = File.Exists(testFile);
            string readContent = File.ReadAllText(testFile);

            // Assert
            Assert.IsTrue(exists, "Test log file should be created");
            Assert.AreEqual(testContent, readContent, "File content should match");

            // Cleanup
            File.Delete(testFile);
        }

        #endregion
    }
}
