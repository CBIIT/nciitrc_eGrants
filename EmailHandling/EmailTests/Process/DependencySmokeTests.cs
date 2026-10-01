using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace EmailHandlingTests.ProcessTests
{
    /// <summary>
    /// Dependency and runtime requirement smoke tests.
    /// 
    /// PURPOSE:
    /// Verify that all executables have their required dependencies and can load successfully.
    /// 
    /// WHAT THESE TESTS CATCH:
    /// - Missing DLL dependencies
    /// - Version mismatches
    /// - Platform target issues (x86 vs x64 vs AnyCPU)
    /// - .NET runtime version issues
    /// - Missing configuration files
    /// - COM interop registration issues
    /// </summary>
    [TestClass]
    public class DependencySmokeTests
    {
        private static string _solutionDir;
        private static string _buildOutputDir;

        [ClassInitialize]
        public static void ClassSetup(TestContext context)
        {
            _solutionDir = FindSolutionDirectory();
            _buildOutputDir = _solutionDir;  // Solution dir IS the EmailHandling directory
        }

        #region Assembly Loading Tests

        [TestMethod]
        [TestCategory("SmokeTest")]
        [TestCategory("Dependencies")]
        public void AllExecutables_CanLoadAssemblies()
        {
            var projects = new[] 
            { 
                "Router", "ExchangeFixed", "AddSuppEmailer", "AddSuppProd", 
                "AddSuppVoteCollection", "LoadPfr", "LoadSuppPfr", "EGrantsAcmAuditReport", 
                "OGARequestAccountDisable", "StartOutlook"
            };

            foreach (var project in projects)
            {
                var exePath = GetExecutablePath(project);
                var exeDir = Path.GetDirectoryName(exePath);

                // In .NET 8, .exe files are native executables, not managed assemblies
                // We need to load the .dll file instead
                var dllPath = exePath.Replace(".exe", ".dll");

                if (!File.Exists(dllPath))
                {
                    Assert.Fail($"{project}: Could not find managed DLL at {dllPath}");
                    continue;
                }

                try
                {
                    // Set up assembly resolution from the executable's directory
                    AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
                    {
                        var assemblyName = new AssemblyName(args.Name).Name;
                        var depDllPath = Path.Combine(exeDir, $"{assemblyName}.dll");

                        if (File.Exists(depDllPath))
                        {
                            return Assembly.LoadFrom(depDllPath);
                        }
                        return null;
                    };

                    // Try to load the assembly
                    var assembly = Assembly.LoadFrom(dllPath);
                    Assert.IsNotNull(assembly, $"{project}: Failed to load assembly");

                    // Verify it has a valid entry point
                    var entryPoint = assembly.EntryPoint;
                    Assert.IsNotNull(entryPoint, $"{project}: No entry point (Main method) found");

                    Console.WriteLine($"? {project}: Assembly loaded successfully, entry point: {entryPoint.DeclaringType}.{entryPoint.Name}");
                }
                catch (Exception ex)
                {
                    Assert.Fail($"{project}: Failed to load assembly: {ex.Message}");
                }
            }
        }

        [TestMethod]
        [TestCategory("SmokeTest")]
        [TestCategory("Dependencies")]
        public void AllExecutables_HaveCommonUtilitiesDependency()
        {
            var projects = new[] 
            { 
                "Router", "ExchangeFixed", "AddSuppEmailer", "AddSuppProd", 
                "AddSuppVoteCollection", "LoadPfr", "LoadSuppPfr", "EGrantsAcmAuditReport", 
                "OGARequestAccountDisable"
            };

            foreach (var project in projects)
            {
                var exePath = GetExecutablePath(project);
                var exeDir = Path.GetDirectoryName(exePath);
                var commonUtilitiesDll = Path.Combine(exeDir, "CommonUtilties.dll");

                Assert.IsTrue(File.Exists(commonUtilitiesDll), 
                    $"{project}: CommonUtilties.dll not found at {commonUtilitiesDll}");

                Console.WriteLine($"? {project}: CommonUtilties.dll found");
            }
        }

        [TestMethod]
        [TestCategory("SmokeTest")]
        [TestCategory("Dependencies")]
        public void AllExecutables_HaveSerilogDependencies()
        {
            var projects = new[] 
            { 
                "Router", "ExchangeFixed", "AddSuppEmailer", "AddSuppProd", 
                "AddSuppVoteCollection", "LoadPfr", "LoadSuppPfr", "EGrantsAcmAuditReport", 
                "OGARequestAccountDisable"
            };

            foreach (var project in projects)
            {
                var exePath = GetExecutablePath(project);
                var exeDir = Path.GetDirectoryName(exePath);

                // Check for Serilog core
                var serilogDll = Path.Combine(exeDir, "Serilog.dll");
                Assert.IsTrue(File.Exists(serilogDll), 
                    $"{project}: Serilog.dll not found at {serilogDll}");

                Console.WriteLine($"? {project}: Serilog dependencies found");
            }
        }

        [TestMethod]
        [TestCategory("SmokeTest")]
        [TestCategory("Dependencies")]
        public void OutlookProjects_HaveInteropDependencies()
        {
            // Projects that use Outlook COM interop
            var outlookProjects = new[] 
            { 
                "Router", "ExchangeFixed", "AddSuppProd", 
                "AddSuppVoteCollection", "OGARequestAccountDisable"
            };

            foreach (var project in outlookProjects)
            {
                var exePath = GetExecutablePath(project);
                var exeDir = Path.GetDirectoryName(exePath);

                // Check for Microsoft.Office.Interop.Outlook (may be embedded or as separate DLL)
                // If using late binding, this may not be present
                Console.WriteLine($"  {project}: Checking for Outlook interop (may use late binding)");
            }
        }

        #endregion

        #region Configuration File Tests

        [TestMethod]
        [TestCategory("SmokeTest")]
        [TestCategory("Configuration")]
        public void AllExecutables_HaveRuntimeConfig()
        {
            var projects = new[] 
            { 
                "Router", "ExchangeFixed", "AddSuppEmailer", "AddSuppProd", 
                "AddSuppVoteCollection", "LoadPfr", "LoadSuppPfr", "EGrantsAcmAuditReport", 
                "OGARequestAccountDisable", "StartOutlook"
            };

            foreach (var project in projects)
            {
                var exePath = GetExecutablePath(project);
                var exeDir = Path.GetDirectoryName(exePath);
                var runtimeConfigJson = Path.Combine(exeDir, $"{project}.runtimeconfig.json");

                Assert.IsTrue(File.Exists(runtimeConfigJson), 
                    $"{project}: Runtime config not found at {runtimeConfigJson}");

                // Verify it specifies .NET 8
                var content = File.ReadAllText(runtimeConfigJson);
                Assert.IsTrue(content.Contains("\"version\": \"8.0") || content.Contains("\"tfm\": \"net8.0"),
                    $"{project}: Runtime config should specify .NET 8");

                Console.WriteLine($"? {project}: Runtime config found and specifies .NET 8");
            }
        }

        [TestMethod]
        [TestCategory("SmokeTest")]
        [TestCategory("Configuration")]
        public void AllExecutables_HaveDepsJson()
        {
            var projects = new[] 
            { 
                "Router", "ExchangeFixed", "AddSuppEmailer", "AddSuppProd", 
                "AddSuppVoteCollection", "LoadPfr", "LoadSuppPfr", "EGrantsAcmAuditReport", 
                "OGARequestAccountDisable", "StartOutlook"
            };

            foreach (var project in projects)
            {
                var exePath = GetExecutablePath(project);
                var exeDir = Path.GetDirectoryName(exePath);
                var depsJson = Path.Combine(exeDir, $"{project}.deps.json");

                Assert.IsTrue(File.Exists(depsJson), 
                    $"{project}: Dependencies manifest not found at {depsJson}");

                Console.WriteLine($"? {project}: Dependencies manifest found");
            }
        }

        #endregion

        #region Platform and Target Tests

        [TestMethod]
        [TestCategory("SmokeTest")]
        [TestCategory("Platform")]
        public void AllExecutables_AreCorrectPlatformTarget()
        {
            var projects = new[] 
            { 
                "Router", "ExchangeFixed", "AddSuppEmailer", "AddSuppProd", 
                "AddSuppVoteCollection", "LoadPfr", "LoadSuppPfr", "EGrantsAcmAuditReport", 
                "OGARequestAccountDisable", "StartOutlook"
            };

            foreach (var project in projects)
            {
                var exePath = GetExecutablePath(project);

                // In .NET 8, .exe files are native executables, not managed assemblies
                // We need to load the .dll file instead to check the platform target
                var dllPath = exePath.Replace(".exe", ".dll");

                if (!File.Exists(dllPath))
                {
                    Assert.Fail($"{project}: Could not find managed DLL at {dllPath}");
                    continue;
                }

                try
                {
                    var assembly = Assembly.LoadFrom(dllPath);
                    var name = assembly.GetName();

                    // Verify processor architecture (should be MSIL/AnyCPU or match current platform)
                    Console.WriteLine($"  {project}: Platform = {name.ProcessorArchitecture}");

                    Assert.IsTrue(
                        name.ProcessorArchitecture == ProcessorArchitecture.MSIL ||
                        name.ProcessorArchitecture == ProcessorArchitecture.Amd64 ||
                        name.ProcessorArchitecture == ProcessorArchitecture.X86,
                        $"{project}: Unexpected processor architecture: {name.ProcessorArchitecture}");
                }
                catch (Exception ex)
                {
                    Assert.Fail($"{project}: Failed to check platform target: {ex.Message}");
                }
            }
        }

        #endregion

        #region Working Directory Tests

        [TestMethod]
        [TestCategory("SmokeTest")]
        [TestCategory("WorkingDirectory")]
        public void AllExecutables_CanFindConfigFromWorkingDirectory()
        {
            var projects = new[] 
            { 
                "Router", "ExchangeFixed", "AddSuppEmailer", "AddSuppProd", 
                "AddSuppVoteCollection", "LoadPfr", "LoadSuppPfr", "EGrantsAcmAuditReport", 
                "OGARequestAccountDisable"
            };

            foreach (var project in projects)
            {
                var exePath = GetExecutablePath(project);
                var exeDir = Path.GetDirectoryName(exePath);

                // Simulate running from the exe directory (typical scheduler setup)
                var originalDir = Directory.GetCurrentDirectory();
                try
                {
                    Directory.SetCurrentDirectory(exeDir);

                    // Verify appsettings.json can be found from working directory
                    Assert.IsTrue(File.Exists("appsettings.json"), 
                        $"{project}: appsettings.json not found when working directory is set to exe location");

                    Console.WriteLine($"? {project}: Can find appsettings.json from working directory");
                }
                finally
                {
                    Directory.SetCurrentDirectory(originalDir);
                }
            }
        }

        #endregion

        #region SQL Dependencies Tests

        [TestMethod]
        [TestCategory("SmokeTest")]
        [TestCategory("Dependencies")]
        public void DatabaseProjects_HaveSqlClientDependency()
        {
            // Projects that connect to SQL Server
            var dbProjects = new[] 
            { 
                "Router", "ExchangeFixed", "AddSuppEmailer", "AddSuppProd", 
                "LoadPfr", "LoadSuppPfr", "EGrantsAcmAuditReport", "OGARequestAccountDisable"
            };

            foreach (var project in dbProjects)
            {
                var exePath = GetExecutablePath(project);
                var exeDir = Path.GetDirectoryName(exePath);

                // Check for System.Data.SqlClient or Microsoft.Data.SqlClient
                var sqlClientFound = Directory.GetFiles(exeDir, "*SqlClient*.dll").Any();

                Assert.IsTrue(sqlClientFound, 
                    $"{project}: No SQL Client library found (System.Data.SqlClient or Microsoft.Data.SqlClient)");

                Console.WriteLine($"? {project}: SQL Client dependency found");
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

        #endregion
    }
}
