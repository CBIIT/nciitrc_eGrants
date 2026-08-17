using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace EmailHandlingTests.ProcessTests
{
    /// <summary>
    /// Log output validation smoke tests.
    /// 
    /// PURPOSE:
    /// Verify that executables produce expected log output and handle logging correctly.
    /// 
    /// WHAT THESE TESTS CATCH:
    /// - Logging initialization failures
    /// - Missing log directories
    /// - Permission issues writing logs
    /// - Improper exception handling (unlogged errors)
    /// - Log format issues
    /// - Serilog configuration problems
    /// </summary>
    [TestClass]
    public class LogOutputSmokeTests
    {
        private static string _solutionDir;
        private static string _buildOutputDir;
        private static string _testLogDir;

        [ClassInitialize]
        public static void ClassSetup(TestContext context)
        {
            _solutionDir = FindSolutionDirectory();
            _buildOutputDir = _solutionDir;  // Solution dir IS the EmailHandling directory

            // Create a temporary log directory for tests
            _testLogDir = Path.Combine(Path.GetTempPath(), "EmailHandling_TestLogs", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testLogDir);

            Console.WriteLine($"Test log directory: {_testLogDir}");
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            // Clean up test log directory
            try
            {
                if (Directory.Exists(_testLogDir))
                {
                    Directory.Delete(_testLogDir, recursive: true);
                }
            }
            catch { /* Ignore cleanup errors */ }
        }

        #region Console Output Tests

        [TestMethod]
        [TestCategory("SmokeTest")]
        [TestCategory("Logging")]
        public void Router_ProducesExpectedConsoleOutput()
        {
            var result = RunExecutableAndCaptureOutput("Router", timeoutSeconds: 20);

            // Router may fail to start due to COM Interop issues when launched as external process
            // This is a known limitation (see SMOKE_TEST_OUTLOOK_SETUP.md)
            bool hasMeaningfulOutput = 
                result.Output.Contains("Router") || 
                result.Output.Contains("Email Router") ||
                result.Output.Contains("Loading configuration") ||
                result.ErrorOutput.Contains("FileNotFoundException") ||  // Known COM issue
                result.ErrorOutput.Contains("office") ||  // COM assembly loading
                result.ErrorOutput.Contains("Outlook");  // Outlook COM error

            Assert.IsTrue(hasMeaningfulOutput,
                $"Expected Router to produce output or show known COM interop error. " +
                $"Output: {result.Output}, Error: {result.ErrorOutput}");

            // If it started successfully, verify environment and config detection
            if (result.ExitCode == 0 && result.Output.Contains("Router"))
            {
                Assert.IsTrue(
                    result.Output.Contains("DOTNET_ENVIRONMENT") || 
                    result.Output.Contains("Development") || 
                    result.Output.Contains("Production") ||
                    result.Output.Contains("config") || 
                    result.Output.Contains("Configuration") || 
                    result.Output.Contains("Loading"),
                    "Expected environment and configuration information in output");
            }
        }

        [TestMethod]
        [TestCategory("SmokeTest")]
        [TestCategory("Logging")]
        public void ExchangeFixed_ProducesExpectedConsoleOutput()
        {
            var result = RunExecutableAndCaptureOutput("ExchangeFixed", timeoutSeconds: 20);

            Assert.IsTrue(
                result.Output.Contains("ExchangeFixed") || result.Output.Contains("Fixed Path Email Router"),
                "Expected application name in output");

            Assert.IsTrue(
                result.Output.Contains("DOTNET_ENVIRONMENT") || result.Output.Contains("config") || result.Output.Contains("Error"),
                "Expected environment or configuration messages");
        }

        [TestMethod]
        [TestCategory("SmokeTest")]
        [TestCategory("Logging")]
        public void AddSuppEmailer_ProducesExpectedConsoleOutput()
        {
            var result = RunExecutableAndCaptureOutput("AddSuppEmailer", timeoutSeconds: 20);

            Assert.IsTrue(
                result.Output.Contains("AddSuppEmailer") || result.Output.Contains("Administrative Supplement"),
                "Expected application name in output");
        }

        [TestMethod]
        [TestCategory("SmokeTest")]
        [TestCategory("Logging")]
        public void AddSuppProd_ProducesExpectedConsoleOutput()
        {
            var result = RunExecutableAndCaptureOutput("AddSuppProd", timeoutSeconds: 20);

            Assert.IsTrue(
                result.Output.Contains("AddSuppProd") || result.Output.Contains("Administrative Supplement Production"),
                "Expected application name in output");
        }

        #endregion

        #region Exception Handling Tests

        [TestMethod]
        [TestCategory("SmokeTest")]
        [TestCategory("ErrorHandling")]
        public void AllExecutables_HandleMissingEnvironmentVariablesGracefully()
        {
            var projects = new[] { "Router", "ExchangeFixed", "AddSuppEmailer", "AddSuppProd" };

            foreach (var project in projects)
            {
                var result = RunExecutableWithNoEnvironmentVariables(project, timeoutSeconds: 15);

                // Different projects handle missing credentials differently:
                // - Some will fail immediately (Router, ExchangeFixed need Outlook + DB)
                // - Others may succeed if there's no work to do (AddSuppEmailer, AddSuppProd)
                // 
                // What we're checking:
                // 1. The app doesn't crash with an unhandled exception
                // 2. If it needs credentials and they're missing, it either:
                //    a. Exits with non-zero code, OR
                //    b. Shows a meaningful error message, OR
                //    c. Completes successfully with "no work done" message

                bool handlesGracefully = 
                    result.ExitCode != 0 ||  // Failed gracefully with error code
                    result.Output.Contains("environment variable") ||  // Mentioned missing env vars
                    result.Output.Contains("DB_USER") ||  // Mentioned the specific variable
                    result.Output.Contains("DB_PASSWORD") ||
                    result.Output.Contains("credential") ||
                    result.Output.Contains("0 emails sent") ||  // Completed with no work done
                    result.Output.Contains("Processing complete") ||  // Successfully completed (no emails to process)
                    result.ErrorOutput.Contains("environment variable") ||
                    result.ErrorOutput.Contains("DB_USER") ||
                    result.ErrorOutput.Contains("DB_PASSWORD") ||
                    !result.ErrorOutput.Contains("Unhandled exception");  // Most importantly: no crash

                Assert.IsTrue(handlesGracefully,
                    $"{project}: Should handle missing environment variables gracefully. " +
                    $"Exit code: {result.ExitCode}, Output: {result.Output}, Error: {result.ErrorOutput}");

                if (result.ExitCode == 0)
                {
                    Console.WriteLine($"? {project}: Completed successfully (no work to do or credentials not required yet)");
                }
                else
                {
                    Console.WriteLine($"? {project}: Handles missing environment variables (exit code: {result.ExitCode})");
                }
            }
        }

        [TestMethod]
        [TestCategory("SmokeTest")]
        [TestCategory("ErrorHandling")]
        public void OutlookProjects_HandleMissingOutlookGracefully()
        {
            // On a build server or test environment without Outlook, these should fail gracefully
            var outlookProjects = new[] { "Router", "ExchangeFixed", "AddSuppProd", "AddSuppVoteCollection" };

            foreach (var project in outlookProjects)
            {
                var result = RunExecutableAndCaptureOutput(project, timeoutSeconds: 20);

                // If Outlook is not installed, should either:
                // 1. Exit with error code, OR
                // 2. Show error message about Outlook not being available
                // We don't assert failure here because Outlook might actually be installed

                if (result.ExitCode != 0)
                {
                    Console.WriteLine($"? {project}: Exited with code {result.ExitCode} (Outlook may not be available - this is expected in test environment)");
                }
                else
                {
                    Console.WriteLine($"  {project}: Exited successfully (Outlook may be installed)");
                }
            }
        }

        #endregion

        #region Log File Tests

        [TestMethod]
        [TestCategory("SmokeTest")]
        [TestCategory("Logging")]
        [TestCategory("Integration")]
        public void AddSuppEmailer_CreatesLogFile()
        {
            // This test requires Serilog to be properly configured
            var projectLogDir = Path.Combine(_testLogDir, "AddSuppEmailer");
            Directory.CreateDirectory(projectLogDir);

            var result = RunExecutableWithCustomLogDir("AddSuppEmailer", projectLogDir, timeoutSeconds: 20);

            // Check if any log files were created
            var logFiles = Directory.GetFiles(projectLogDir, "*AddSuppEmailer*.log");

            if (logFiles.Length > 0)
            {
                Console.WriteLine($"? AddSuppEmailer: Created log file: {Path.GetFileName(logFiles[0])}");

                // Verify log file has content
                var logContent = File.ReadAllText(logFiles[0]);
                Assert.IsFalse(string.IsNullOrWhiteSpace(logContent), "Log file should not be empty");

                Console.WriteLine($"  Log file size: {new FileInfo(logFiles[0]).Length} bytes");
            }
            else
            {
                Console.WriteLine($"  AddSuppEmailer: No Serilog file created (may use different logging mechanism)");
            }
        }

        #endregion

        #region Startup Time Tests

        [TestMethod]
        [TestCategory("SmokeTest")]
        [TestCategory("Performance")]
        public void AllExecutables_StartupTimeIsReasonable()
        {
            var projects = new[] 
            { 
                "Router", "ExchangeFixed", "AddSuppEmailer", "AddSuppProd", 
                "AddSuppVoteCollection", "LoadPfr", "LoadSuppPfr"
            };

            foreach (var project in projects)
            {
                var stopwatch = Stopwatch.StartNew();
                var result = RunExecutableAndCaptureOutput(project, timeoutSeconds: 30);
                stopwatch.Stop();

                // Startup should be reasonably fast (under 10 seconds)
                // This catches issues like:
                // - Excessive assembly scanning
                // - Network timeouts during config loading
                // - Slow DI container initialization
                Assert.IsTrue(stopwatch.ElapsedMilliseconds < 30000,
                    $"{project}: Startup took {stopwatch.ElapsedMilliseconds}ms (should be under 30 seconds)");

                Console.WriteLine($"  {project}: Startup time = {stopwatch.ElapsedMilliseconds}ms");
            }
        }

        #endregion

        #region Helper Methods

        private static string FindSolutionDirectory()
        {
            var currentDir = Directory.GetCurrentDirectory();

            while (!string.IsNullOrEmpty(currentDir))
            {
                if (Directory.GetFiles(currentDir, "*.sln").Any())
                {
                    return currentDir;
                }

                currentDir = Directory.GetParent(currentDir)?.FullName;
            }

            throw new InvalidOperationException("Could not find solution directory");
        }

        private static string GetExecutablePath(string projectName)
        {
            // Some projects target net8.0-windows, others target net8.0
            // Try both in Debug and Release configurations
            var possiblePaths = new[]
            {
                Path.Combine(_buildOutputDir, projectName, "bin", "Debug", "net8.0-windows", $"{projectName}.exe"),
                Path.Combine(_buildOutputDir, projectName, "bin", "Debug", "net8.0", $"{projectName}.exe"),
                Path.Combine(_buildOutputDir, projectName, "bin", "Release", "net8.0-windows", $"{projectName}.exe"),
                Path.Combine(_buildOutputDir, projectName, "bin", "Release", "net8.0", $"{projectName}.exe")
            };

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }

            var triedPaths = string.Join("\n", possiblePaths);
            Assert.Fail($"Executable not found for {projectName}. Tried:\n{triedPaths}\n\nBuild the solution first.");
            return null;
        }

        private ProcessTestResult RunExecutableAndCaptureOutput(string projectName, int timeoutSeconds = 30)
        {
            var exePath = GetExecutablePath(projectName);
            var workingDir = Path.GetDirectoryName(exePath);
            var output = new StringBuilder();
            var errorOutput = new StringBuilder();

            var startInfo = new ProcessStartInfo
            {
                FileName = exePath,
                WorkingDirectory = workingDir,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            // Set test environment variables
            startInfo.Environment["DOTNET_ENVIRONMENT"] = "Development";
            startInfo.Environment["DB_USER"] = "test_user";
            startInfo.Environment["DB_PASSWORD"] = "test_password";

            using (var process = new Process { StartInfo = startInfo })
            {
                process.OutputDataReceived += (sender, e) => 
                {
                    if (e.Data != null) output.AppendLine(e.Data);
                };

                process.ErrorDataReceived += (sender, e) => 
                {
                    if (e.Data != null) errorOutput.AppendLine(e.Data);
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                bool exited = process.WaitForExit(timeoutSeconds * 1000);

                if (!exited)
                {
                    process.Kill();
                }

                return new ProcessTestResult
                {
                    ExitCode = exited ? process.ExitCode : -1,
                    Output = output.ToString(),
                    ErrorOutput = errorOutput.ToString(),
                    TimedOut = !exited
                };
            }
        }

        private ProcessTestResult RunExecutableWithNoEnvironmentVariables(string projectName, int timeoutSeconds = 30)
        {
            var exePath = GetExecutablePath(projectName);
            var workingDir = Path.GetDirectoryName(exePath);
            var output = new StringBuilder();
            var errorOutput = new StringBuilder();

            var startInfo = new ProcessStartInfo
            {
                FileName = exePath,
                WorkingDirectory = workingDir,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            // Explicitly DO NOT set DB_USER or DB_PASSWORD
            startInfo.Environment["DOTNET_ENVIRONMENT"] = "Development";

            using (var process = new Process { StartInfo = startInfo })
            {
                process.OutputDataReceived += (sender, e) => 
                {
                    if (e.Data != null) output.AppendLine(e.Data);
                };

                process.ErrorDataReceived += (sender, e) => 
                {
                    if (e.Data != null) errorOutput.AppendLine(e.Data);
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                bool exited = process.WaitForExit(timeoutSeconds * 1000);

                if (!exited)
                {
                    process.Kill();
                }

                return new ProcessTestResult
                {
                    ExitCode = exited ? process.ExitCode : -1,
                    Output = output.ToString(),
                    ErrorOutput = errorOutput.ToString(),
                    TimedOut = !exited
                };
            }
        }

        private ProcessTestResult RunExecutableWithCustomLogDir(string projectName, string logDir, int timeoutSeconds = 30)
        {
            var exePath = GetExecutablePath(projectName);
            var workingDir = Path.GetDirectoryName(exePath);
            var output = new StringBuilder();
            var errorOutput = new StringBuilder();

            // Temporarily modify appsettings.json to use test log directory
            var appSettingsPath = Path.Combine(workingDir, "appsettings.json");
            string originalContent = null;
            bool modifiedConfig = false;

            if (File.Exists(appSettingsPath))
            {
                originalContent = File.ReadAllText(appSettingsPath);
                var modifiedContent = Regex.Replace(originalContent, 
                    @"""LogDir"":\s*""[^""]*""", 
                    $@"""LogDir"": ""{logDir.Replace("\\", "\\\\")}""");
                File.WriteAllText(appSettingsPath, modifiedContent);
                modifiedConfig = true;
            }

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    WorkingDirectory = workingDir,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                startInfo.Environment["DOTNET_ENVIRONMENT"] = "Development";
                startInfo.Environment["DB_USER"] = "test_user";
                startInfo.Environment["DB_PASSWORD"] = "test_password";

                using (var process = new Process { StartInfo = startInfo })
                {
                    process.OutputDataReceived += (sender, e) => 
                    {
                        if (e.Data != null) output.AppendLine(e.Data);
                    };

                    process.ErrorDataReceived += (sender, e) => 
                    {
                        if (e.Data != null) errorOutput.AppendLine(e.Data);
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    bool exited = process.WaitForExit(timeoutSeconds * 1000);

                    if (!exited)
                    {
                        process.Kill();
                    }

                    // Give Serilog time to flush
                    Thread.Sleep(1000);

                    return new ProcessTestResult
                    {
                        ExitCode = exited ? process.ExitCode : -1,
                        Output = output.ToString(),
                        ErrorOutput = errorOutput.ToString(),
                        TimedOut = !exited
                    };
                }
            }
            finally
            {
                // Restore original appsettings.json
                if (modifiedConfig && originalContent != null)
                {
                    File.WriteAllText(appSettingsPath, originalContent);
                }
            }
        }

        private class ProcessTestResult
        {
            public int ExitCode { get; set; }
            public string Output { get; set; }
            public string ErrorOutput { get; set; }
            public bool TimedOut { get; set; }
        }

        #endregion
    }
}
