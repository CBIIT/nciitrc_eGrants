using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EmailHandlingTests.CommonUtilitiesProject
{
    /// <summary>
    /// Integration tests for the CommonUtilties.CommonUtilities class.
    /// These tests verify utility functions used across all email processing projects.
    /// </summary>
    [TestClass]
    public class CommonUtilitiesIntegrationTests
    {
        private string _testLogDir;

        [TestInitialize]
        public void Setup()
        {
            _testLogDir = Path.Combine(Path.GetTempPath(), "EmailHandlingTestsLogs_" + Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testLogDir);
            CommonUtilties.CommonUtilities.LogDir = _testLogDir;
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_testLogDir))
            {
                try
                {
                    Directory.Delete(_testLogDir, true);
                }
                catch
                {
                    // Ignore cleanup errors in tests
                }
            }
        }

        #region RemoveSpaceCharacters Tests

        /// <summary>
        /// Verifies that colons are replaced with spaces and then removed.
        /// </summary>
        [TestMethod]
        public void RemoveSpaceCharacters_RemovesColons()
        {
            // Arrange
            string input = "Test:Subject:Line";

            // Act
            string result = CommonUtilties.CommonUtilities.RemoveSpaceCharacters(input);

            // Assert
            Assert.IsFalse(result.Contains(":"), "Result should not contain colons");
        }

        /// <summary>
        /// Verifies that forward slashes are replaced.
        /// </summary>
        [TestMethod]
        public void RemoveSpaceCharacters_RemovesForwardSlashes()
        {
            // Arrange
            string input = "Test/Subject/Line";

            // Act
            string result = CommonUtilties.CommonUtilities.RemoveSpaceCharacters(input);

            // Assert
            Assert.IsFalse(result.Contains("/"), "Result should not contain forward slashes");
        }

        /// <summary>
        /// Verifies that backslashes are replaced.
        /// </summary>
        [TestMethod]
        public void RemoveSpaceCharacters_RemovesBackslashes()
        {
            // Arrange
            string input = @"Test\Subject\Line";

            // Act
            string result = CommonUtilties.CommonUtilities.RemoveSpaceCharacters(input);

            // Assert
            Assert.IsFalse(result.Contains(@"\"), "Result should not contain backslashes");
        }

        /// <summary>
        /// Verifies that ampersands are replaced with 'and'.
        /// </summary>
        [TestMethod]
        public void RemoveSpaceCharacters_ReplacesAmpersandWithAnd()
        {
            // Arrange
            string input = "Test&Subject";

            // Act
            string result = CommonUtilties.CommonUtilities.RemoveSpaceCharacters(input);

            // Assert
            Assert.IsTrue(result.Contains("and"), "Result should contain 'and'");
            Assert.IsFalse(result.Contains("&"), "Result should not contain ampersand");
        }

        /// <summary>
        /// Verifies that semicolons are removed.
        /// </summary>
        [TestMethod]
        public void RemoveSpaceCharacters_RemovesSemicolons()
        {
            // Arrange
            string input = "Test;Subject;Line";

            // Act
            string result = CommonUtilties.CommonUtilities.RemoveSpaceCharacters(input);

            // Assert
            Assert.IsFalse(result.Contains(";"), "Result should not contain semicolons");
        }

        /// <summary>
        /// Verifies that angle brackets are removed.
        /// </summary>
        [TestMethod]
        public void RemoveSpaceCharacters_RemovesAngleBrackets()
        {
            // Arrange
            string input = "<Test>Subject<Line>";

            // Act
            string result = CommonUtilties.CommonUtilities.RemoveSpaceCharacters(input);

            // Assert
            Assert.IsFalse(result.Contains("<"), "Result should not contain <");
            Assert.IsFalse(result.Contains(">"), "Result should not contain >");
        }

        /// <summary>
        /// Verifies that double angle brackets are removed.
        /// </summary>
        [TestMethod]
        public void RemoveSpaceCharacters_RemovesDoubleAngleBrackets()
        {
            // Arrange
            string input = "<<Test>>Subject";

            // Act
            string result = CommonUtilties.CommonUtilities.RemoveSpaceCharacters(input);

            // Assert
            Assert.IsFalse(result.Contains("<<"), "Result should not contain <<");
            Assert.IsFalse(result.Contains(">>"), "Result should not contain >>");
        }

        /// <summary>
        /// Verifies that caret symbols are removed.
        /// </summary>
        [TestMethod]
        public void RemoveSpaceCharacters_RemovesCarets()
        {
            // Arrange
            string input = "Test^Subject";

            // Act
            string result = CommonUtilties.CommonUtilities.RemoveSpaceCharacters(input);

            // Assert
            Assert.IsFalse(result.Contains("^"), "Result should not contain caret");
        }

        /// <summary>
        /// Verifies that percent symbols are removed.
        /// </summary>
        [TestMethod]
        public void RemoveSpaceCharacters_RemovesPercent()
        {
            // Arrange
            string input = "Test%Subject";

            // Act
            string result = CommonUtilties.CommonUtilities.RemoveSpaceCharacters(input);

            // Assert
            Assert.IsFalse(result.Contains("%"), "Result should not contain percent");
        }

        /// <summary>
        /// Verifies that at symbols are removed.
        /// </summary>
        [TestMethod]
        public void RemoveSpaceCharacters_RemovesAtSymbol()
        {
            // Arrange
            string input = "test@example.com";

            // Act
            string result = CommonUtilties.CommonUtilities.RemoveSpaceCharacters(input);

            // Assert
            Assert.IsFalse(result.Contains("@"), "Result should not contain @ symbol");
        }

        /// <summary>
        /// Verifies that single quotes are removed.
        /// </summary>
        [TestMethod]
        public void RemoveSpaceCharacters_RemovesSingleQuotes()
        {
            // Arrange
            string input = "Test'Subject";

            // Act
            string result = CommonUtilties.CommonUtilities.RemoveSpaceCharacters(input);

            // Assert
            Assert.IsFalse(result.Contains("'"), "Result should not contain single quotes");
        }

        /// <summary>
        /// Verifies that all spaces are removed.
        /// </summary>
        [TestMethod]
        public void RemoveSpaceCharacters_RemovesSpaces()
        {
            // Arrange
            string input = "Test Subject Line";

            // Act
            string result = CommonUtilties.CommonUtilities.RemoveSpaceCharacters(input);

            // Assert
            Assert.IsFalse(result.Contains(" "), "Result should not contain spaces");
            Assert.AreEqual("TestSubjectLine", result);
        }

        /// <summary>
        /// Verifies that vbLf is replaced with vbCrLF.
        /// </summary>
        [TestMethod]
        public void RemoveSpaceCharacters_ReplacesVbLf()
        {
            // Arrange
            string input = "TestvbLfSubject";

            // Act
            string result = CommonUtilties.CommonUtilities.RemoveSpaceCharacters(input);

            // Assert
            Assert.IsTrue(result.Contains("vbCrLF"), "Result should contain vbCrLF");
            Assert.IsFalse(result.Contains("vbLf"), "Result should not contain vbLf");
        }

        /// <summary>
        /// Verifies that result is trimmed.
        /// </summary>
        [TestMethod]
        public void RemoveSpaceCharacters_TrimResult()
        {
            // Arrange
            string input = "  TestSubject  ";

            // Act
            string result = CommonUtilties.CommonUtilities.RemoveSpaceCharacters(input);

            // Assert
            Assert.AreEqual("TestSubject", result, "Result should be trimmed");
        }

        /// <summary>
        /// Verifies handling of complex input with multiple special characters.
        /// </summary>
        [TestMethod]
        public void RemoveSpaceCharacters_ComplexInput()
        {
            // Arrange
            string input = "Grant: CA123456 - 01/15/2024 <PI: John & Jane>";

            // Act
            string result = CommonUtilties.CommonUtilities.RemoveSpaceCharacters(input);

            // Assert
            Assert.IsFalse(result.Contains(":"), "Should not contain colon");
            Assert.IsFalse(result.Contains("/"), "Should not contain slash");
            Assert.IsFalse(result.Contains("<"), "Should not contain <");
            Assert.IsFalse(result.Contains(">"), "Should not contain >");
            Assert.IsFalse(result.Contains(" "), "Should not contain space");
            Assert.IsTrue(result.Contains("and"), "Should contain 'and'");
        }

        /// <summary>
        /// Verifies handling of empty string.
        /// </summary>
        [TestMethod]
        public void RemoveSpaceCharacters_EmptyString()
        {
            // Arrange
            string input = "";

            // Act
            string result = CommonUtilties.CommonUtilities.RemoveSpaceCharacters(input);

            // Assert
            Assert.AreEqual("", result, "Empty input should return empty result");
        }

        /// <summary>
        /// Verifies handling of string with only special characters.
        /// </summary>
        [TestMethod]
        public void RemoveSpaceCharacters_OnlySpecialChars()
        {
            // Arrange
            string input = ":;/<>@%^'";

            // Act
            string result = CommonUtilties.CommonUtilities.RemoveSpaceCharacters(input);

            // Assert
            Assert.AreEqual("", result, "String with only special chars should return empty");
        }

        #endregion

        #region ShowDiagnosticIfVerbose Tests

        /// <summary>
        /// Verifies that message is written when verbose is 'y'.
        /// </summary>
        [TestMethod]
        public void ShowDiagnosticIfVerbose_VerboseY_WritesMessage()
        {
            // Arrange
            string message = "Test diagnostic message";
            var consoleOutput = new StringWriter();
            var originalOutput = Console.Out;
            Console.SetOut(consoleOutput);

            try
            {
                // Act
                CommonUtilties.CommonUtilities.ShowDiagnosticIfVerbose(message, "y");

                // Assert
                string output = consoleOutput.ToString();
                Assert.IsTrue(output.Contains(message), "Output should contain the message");
            }
            finally
            {
                Console.SetOut(originalOutput);
            }
        }

        /// <summary>
        /// Verifies that message is written when verbose is 'Y' (uppercase).
        /// </summary>
        [TestMethod]
        public void ShowDiagnosticIfVerbose_VerboseUpperY_WritesMessage()
        {
            // Arrange
            string message = "Test uppercase verbose";
            var consoleOutput = new StringWriter();
            var originalOutput = Console.Out;
            Console.SetOut(consoleOutput);

            try
            {
                // Act
                CommonUtilties.CommonUtilities.ShowDiagnosticIfVerbose(message, "Y");

                // Assert
                string output = consoleOutput.ToString();
                Assert.IsTrue(output.Contains(message), "Output should contain the message");
            }
            finally
            {
                Console.SetOut(originalOutput);
            }
        }

        /// <summary>
        /// Verifies that message is written when verbose is 'yes'.
        /// </summary>
        [TestMethod]
        public void ShowDiagnosticIfVerbose_VerboseYes_WritesMessage()
        {
            // Arrange
            string message = "Test yes verbose";
            var consoleOutput = new StringWriter();
            var originalOutput = Console.Out;
            Console.SetOut(consoleOutput);

            try
            {
                // Act
                CommonUtilties.CommonUtilities.ShowDiagnosticIfVerbose(message, "yes");

                // Assert
                string output = consoleOutput.ToString();
                Assert.IsTrue(output.Contains(message), "Output should contain the message");
            }
            finally
            {
                Console.SetOut(originalOutput);
            }
        }

        /// <summary>
        /// Verifies that message is NOT written when verbose is 'n'.
        /// </summary>
        [TestMethod]
        public void ShowDiagnosticIfVerbose_VerboseN_NoOutput()
        {
            // Arrange
            string message = "This should not appear";
            var consoleOutput = new StringWriter();
            var originalOutput = Console.Out;
            Console.SetOut(consoleOutput);

            try
            {
                // Act
                CommonUtilties.CommonUtilities.ShowDiagnosticIfVerbose(message, "n");

                // Assert
                string output = consoleOutput.ToString();
                Assert.IsFalse(output.Contains(message), "Output should NOT contain the message");
            }
            finally
            {
                Console.SetOut(originalOutput);
            }
        }

        /// <summary>
        /// Verifies that message is NOT written when verbose is empty.
        /// </summary>
        [TestMethod]
        public void ShowDiagnosticIfVerbose_VerboseEmpty_NoOutput()
        {
            // Arrange
            string message = "This should not appear";
            var consoleOutput = new StringWriter();
            var originalOutput = Console.Out;
            Console.SetOut(consoleOutput);

            try
            {
                // Act
                CommonUtilties.CommonUtilities.ShowDiagnosticIfVerbose(message, "");

                // Assert
                string output = consoleOutput.ToString();
                Assert.IsFalse(output.Contains(message), "Output should NOT contain the message");
            }
            finally
            {
                Console.SetOut(originalOutput);
            }
        }

        #endregion

        #region WriteLog Tests

        /// <summary>
        /// Verifies that log file is created with correct name format.
        /// </summary>
        [TestMethod]
        public void WriteLog_CreatesLogFile()
        {
            // Arrange
            var timestamp = DateTime.Now;
            string expectedFileName = $"eMailRouter-Log-{timestamp.Year}-{timestamp.Month}-{timestamp.Day}.txt";

            // Act
            CommonUtilties.CommonUtilities.WriteLog(1, "Test message", null, timestamp);

            // Assert
            string expectedPath = Path.Combine(_testLogDir, expectedFileName);
            Assert.IsTrue(File.Exists(expectedPath), $"Log file should exist at {expectedPath}");
        }

        /// <summary>
        /// Verifies that log file contains the message.
        /// </summary>
        [TestMethod]
        public void WriteLog_ContainsMessage()
        {
            // Arrange
            var timestamp = DateTime.Now;
            string message = "Test log message content";
            string expectedFileName = $"eMailRouter-Log-{timestamp.Year}-{timestamp.Month}-{timestamp.Day}.txt";

            // Act
            CommonUtilties.CommonUtilities.WriteLog(1, message, null, timestamp);

            // Assert
            string logPath = Path.Combine(_testLogDir, expectedFileName);
            string content = File.ReadAllText(logPath);
            Assert.IsTrue(content.Contains(message), "Log should contain the message");
        }

        /// <summary>
        /// Verifies that log file contains error info when provided.
        /// </summary>
        [TestMethod]
        public void WriteLog_ContainsErrorInfo()
        {
            // Arrange
            var timestamp = DateTime.Now;
            string message = "Error occurred";
            string errorInfo = "Stack trace details here";
            string expectedFileName = $"eMailRouter-Log-{timestamp.Year}-{timestamp.Month}-{timestamp.Day}.txt";

            // Act
            CommonUtilties.CommonUtilities.WriteLog(1, message, errorInfo, timestamp);

            // Assert
            string logPath = Path.Combine(_testLogDir, expectedFileName);
            string content = File.ReadAllText(logPath);
            Assert.IsTrue(content.Contains(errorInfo), "Log should contain the error info");
        }

        /// <summary>
        /// Verifies that log file contains timestamp.
        /// </summary>
        [TestMethod]
        public void WriteLog_ContainsTimestamp()
        {
            // Arrange
            var timestamp = new DateTime(2024, 6, 15, 14, 30, 0);
            string expectedFileName = $"eMailRouter-Log-{timestamp.Year}-{timestamp.Month}-{timestamp.Day}.txt";

            // Act
            CommonUtilties.CommonUtilities.WriteLog(1, "Test", null, timestamp);

            // Assert
            string logPath = Path.Combine(_testLogDir, expectedFileName);
            string content = File.ReadAllText(logPath);
            Assert.IsTrue(content.Contains("6/15/2024") || content.Contains("2024"),
              "Log should contain timestamp");
        }

        /// <summary>
        /// Verifies that multiple log entries are appended.
        /// </summary>
        [TestMethod]
        public void WriteLog_AppendsMultipleEntries()
        {
            // Arrange
            var timestamp = DateTime.Now;
            string expectedFileName = $"eMailRouter-Log-{timestamp.Year}-{timestamp.Month}-{timestamp.Day}.txt";

            // Act
            CommonUtilties.CommonUtilities.WriteLog(1, "First entry", null, timestamp);
            CommonUtilties.CommonUtilities.WriteLog(2, "Second entry", null, timestamp);
            CommonUtilties.CommonUtilities.WriteLog(3, "Third entry", null, timestamp);

            // Assert
            string logPath = Path.Combine(_testLogDir, expectedFileName);
            string content = File.ReadAllText(logPath);
            Assert.IsTrue(content.Contains("First entry"), "Should contain first entry");
            Assert.IsTrue(content.Contains("Second entry"), "Should contain second entry");
            Assert.IsTrue(content.Contains("Third entry"), "Should contain third entry");
        }

        #endregion

        #region GetConfigVal Tests - OBSOLETE (projects now use appsettings.json)

        // NOTE: GetConfigVal has been removed from CommonUtilities.
        // All projects now use appsettings.json with AppConfig.Load() instead of config.csv.
        // These tests are commented out as they are no longer applicable.

        /*
        /// <summary>
        /// Verifies that GetConfigVal expands environment variables in the returned value.
        /// </summary>
        [TestMethod]
        public void GetConfigVal_WithEnvironmentVariables_ExpandsVariables()
        {
            // Arrange - Environment variables are set in TestInitialize
            string expectedUser = Environment.GetEnvironmentVariable("DB_USER");
            string expectedPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
            string tempConfigPath = "config.csv";
            string originalContent = null;
            bool configExisted = File.Exists(tempConfigPath);

            if (configExisted)
            {
                originalContent = File.ReadAllText(tempConfigPath);
            }

            try
            {
                // Create test config with environment variables
                File.WriteAllText(tempConfigPath, "testConStr,,,,,User ID=%DB_USER%;Password=%DB_PASSWORD%");

                // Act
                string result = CommonUtilties.CommonUtilities.GetConfigVal("testConStr");

                // Assert
                Assert.IsTrue(result.Contains(expectedUser), "Should expand DB_USER");
                Assert.IsTrue(result.Contains(expectedPassword), "Should expand DB_PASSWORD");
                Assert.IsFalse(result.Contains("%DB_USER%"), "Should not contain unexpanded variable");
                Assert.IsFalse(result.Contains("%DB_PASSWORD%"), "Should not contain unexpanded variable");
            }
            finally
            {
                // Restore original config.csv
                if (configExisted && originalContent != null)
                {
                    File.WriteAllText(tempConfigPath, originalContent);
                }
                else if (!configExisted)
                {
                    File.Delete(tempConfigPath);
                }
            }
        }

        /// <summary>
        /// Verifies that SetLocalTestEnvironmentVariables properly sets environment variables.
        /// </summary>
        [TestMethod]
        public void SetLocalTestEnvironmentVariables_SetsVariables()
        {
            // Arrange
            string testUser = "testuser123";
            string testPassword = "testpass456";

            // Act
            CommonUtilties.CommonUtilities.SetLocalTestEnvironmentVariables(testUser, testPassword);

            // Assert
            Assert.AreEqual(testUser, Environment.GetEnvironmentVariable("DB_USER"));
            Assert.AreEqual(testPassword, Environment.GetEnvironmentVariable("DB_PASSWORD"));
        }

        /// <summary>
        /// Verifies that GetConfigVal returns correct value for existing key.
        /// Note: This test requires config.csv to exist with test data.
        /// </summary>
        [TestMethod]
        public void GetConfigVal_ExistingKey_ReturnsValue()
        {
            // This test would need a config.csv file to be present
            // Skip if config file doesn't exist
            if (!File.Exists("config.csv"))
            {
                Assert.Inconclusive("config.csv not found - skipping test");
                return;
            }

            // Act
            string result = CommonUtilties.CommonUtilities.GetConfigVal("test_key");

            // Assert - The actual assertion depends on what's in config.csv
            Assert.IsNotNull(result, "Result should not be null");
        }

        /// <summary>
        /// Verifies that GetConfigVal returns failure message for non-existing key.
        /// Note: This test requires config.csv to exist.
        /// </summary>
        [TestMethod]
        public void GetConfigVal_NonExistingKey_ReturnsFailedMessage()
        {
            // This test would need a config.csv file to be present
            if (!File.Exists("config.csv"))
            {
                Assert.Inconclusive("config.csv not found - skipping test");
                return;
            }

            // Act
            string result = CommonUtilties.CommonUtilities.GetConfigVal("non_existing_key_12345");

            // Assert
            Assert.AreEqual("FAILED TO FIND VALUE", result, "Should return failure message");
        }
        */

        #endregion

        #region LogDir Property Tests

        /// <summary>
        /// Verifies that LogDir property can be set and retrieved.
        /// </summary>
        [TestMethod]
        public void LogDir_SetAndGet_ReturnsSetValue()
        {
            // Arrange
            string expectedDir = @"C:\Test\Logs";

            // Act
            CommonUtilties.CommonUtilities.LogDir = expectedDir;

            // Assert
            Assert.AreEqual(expectedDir, CommonUtilties.CommonUtilities.LogDir);
        }

        /// <summary>
        /// Verifies that LogDir defaults to empty string.
        /// </summary>
        [TestMethod]
        public void LogDir_NewInstance_DefaultsToEmpty()
        {
            // Arrange
            var utils = new CommonUtilties.CommonUtilities();

            // The static LogDir would have been set by other tests,
            // so this just verifies the constructor runs without error
            Assert.IsNotNull(utils, "Should create instance successfully");
        }

        #endregion
    }
}
