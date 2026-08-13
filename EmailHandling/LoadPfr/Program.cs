using System;
using System.Data.SqlClient;
using System.IO;
using CommonUtilties;
using Microsoft.Extensions.Configuration;

namespace LoadPfr
{
    /// <summary>
    /// Load PFR (Progress/Final Report) Application
    /// 
    /// PURPOSE:
    /// Processes XML metadata files containing Progress or Final Report information and loads
    /// the associated PDF documents into the eGrants document management system.
    /// 
    /// ORIGINAL SOURCE: Migrated from Load_PFR.vbs
    /// 
    /// WORKFLOW:
    /// 1. Reads configuration from appsettings.json (paths, connection string, logging)
    /// 2. Scans the source directory for XML metadata files
    /// 3. For each XML file:
    ///    - Parses metadata (applid, filename, date, file type, creator)
    ///    - Calls Create_PFR stored procedure to register the document
    ///    - Copies the PDF to the final destination with the assigned file number
    ///    - Moves both XML and PDF files to the backup directory
    /// 4. Logs all processing activities and errors
    /// 
    /// XML FILE FORMAT:
    /// The XML metadata files contain one or more document entries with these fields:
    /// - APPLID: Application ID in the eGrants system
    /// - FILENAME: Name of the PDF file to be loaded
    /// - DATE: Document date (when the report was created)
    /// - FILE_TYPE: File extension (typically "pdf")
    /// - UID: User ID of the person who created/uploaded the report
    /// 
    /// CONFIGURATION:
    /// Uses appsettings.json with environment-specific overrides:
    /// - appsettings.json: Base configuration
    /// - appsettings.Development.json: Development environment settings
    /// - appsettings.Production.json: Production environment settings
    /// 
    /// DEPENDENCIES:
    /// - SQL Server database with Create_PFR stored procedure
    /// - File system access to source, backup, and final destination directories
    /// - CommonUtilities project for logging and diagnostics
    /// 
    /// SCHEDULED TASK:
    /// Typically run as a Windows Scheduled Task on a regular interval to process
    /// incoming PFR documents that have been placed in the source directory.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Main entry point for the LoadPfr application.
        /// Initializes configuration, processes PFR files, and handles top-level errors.
        /// </summary>
        /// <param name="args">Command line arguments (currently not used)</param>
        static void Main(string[] args)
        {
            try
            {
#if DEBUG
                // In debug builds, default to Development environment for local testing
                Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
#endif

                var startTimeStamp = DateTime.Now;
                Console.WriteLine("LoadPfr - Progress/Final Report Loader");

                // Load configuration from shared appsettings.json (via CommonUtilties.AppConfig)
                var config = AppConfig.Load();

                var verbose = config["AppSettings:Verbose"] ?? "n";
                var logDir = config["AppSettings:LogDir"] ?? @"C:\eGrants\apps\log\";
                var conStr = AppConfig.GetConnectionString(config, "EIM");
                var networkSharePath = config["PfrPaths:NetworkSharePath"];
                var docSrcPath = config["PfrPaths:DocSrcPath"];
                var bakDstPath = config["PfrPaths:BakDstPath"];
                var outDir = config["PfrPaths:OutDir"];
                var serverDstPath = config["PfrPaths:ServerDstPath"];

                // Set the global log directory for CommonUtilities logging
                CommonUtilities.LogDir = logDir;
                CommonUtilities.InitializeLogging("LoadPfr", logDir);

                // Log the start of processing
                CommonUtilities.Logger?.Information(".........Task Started!........");
                CommonUtilities.ShowDiagnosticIfVerbose("LoadPfr task is starting", verbose);

                // Move files from network share to local source directory before processing
                int filesMoved = MoveFilesFromNetworkShare(networkSharePath, docSrcPath, verbose, logDir);
                if (filesMoved > 0)
                {
                    CommonUtilities.Logger?.Information("{FilesMoved} file(s) moved from network share to {DocSrcPath}", filesMoved, docSrcPath);
                    CommonUtilities.ShowDiagnosticIfVerbose($"{filesMoved} file(s) moved from network share.", verbose);
                }
                else
                {
                    CommonUtilities.Logger?.Information("No files found on network share to move.");
                    CommonUtilities.ShowDiagnosticIfVerbose("No files found on network share to move.", verbose);
                }

                // Check DocSrcPath for any files to process
                int xmlFileCount = Directory.Exists(docSrcPath) ? Directory.GetFiles(docSrcPath, "*.xml").Length : 0;
                if (xmlFileCount == 0)
                {
                    CommonUtilities.Logger?.Information("No files to process in DocSrcPath. Exiting.");
                    CommonUtilities.ShowDiagnosticIfVerbose("No files to process in DocSrcPath. Exiting.", verbose);
                    return;
                }

                CommonUtilities.Logger?.Information("Found {XmlFileCount} XML file(s) in DocSrcPath to process.", xmlFileCount);
                CommonUtilities.ShowDiagnosticIfVerbose($"Found {xmlFileCount} XML file(s) in DocSrcPath to process.", verbose);

                // Process all PFR files in the source directory
                using (var con = new SqlConnection(conStr))
                {
                    var processor = new Processor();
                    var filesProcessed = processor.Process(con, docSrcPath, bakDstPath, outDir, serverDstPath, verbose, logDir, config);
                    CommonUtilities.Logger?.Information("******* Task Completed! ******* {FilesProcessed} files processed.", filesProcessed);
                }

                CommonUtilities.ShowDiagnosticIfVerbose("Done", verbose);
            }
            catch (Exception ex)
            {
                // Log any unhandled exceptions to console and log file
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                CommonUtilities.Logger?.Fatal(ex, "Fatal error in LoadPfr: {ErrorMessage}", ex.Message);
            }
            finally
            {
                CommonUtilities.CloseLogging();
            }
        }

        /// <summary>
        /// Moves all files from the network share to the local source directory.
        /// This ensures files are available locally before processing begins.
        /// Returns the number of files moved (0 if none found or share not configured).
        /// </summary>
        private static int MoveFilesFromNetworkShare(string networkSharePath, string docSrcPath, string verbose, string logDir)
        {
            if (string.IsNullOrWhiteSpace(networkSharePath))
            {
                CommonUtilities.ShowDiagnosticIfVerbose("NetworkSharePath not configured, skipping network transfer", verbose);
                return 0;
            }

            CommonUtilities.ShowDiagnosticIfVerbose($"Moving files from network share: {networkSharePath} to {docSrcPath}", verbose);
            CommonUtilities.Logger?.Information("Moving from network share. Source='{NetworkSharePath}' Destination='{DocSrcPath}'", networkSharePath, docSrcPath);

            if (!Directory.Exists(networkSharePath))
            {
                var errorMsg = $"Network share source folder not found: {networkSharePath}";
                CommonUtilities.Logger?.Error("Network share source folder not found: {NetworkSharePath}", networkSharePath);
                CommonUtilities.ShowDiagnosticIfVerbose(errorMsg, verbose);
                throw new DirectoryNotFoundException(errorMsg);
            }

            if (!Directory.Exists(docSrcPath))
            {
                var errorMsg = $"Local destination folder not found: {docSrcPath}";
                CommonUtilities.Logger?.Error("Local destination folder not found: {DocSrcPath}", docSrcPath);
                CommonUtilities.ShowDiagnosticIfVerbose(errorMsg, verbose);
                throw new DirectoryNotFoundException(errorMsg);
            }

            var files = Directory.GetFiles(networkSharePath);

            if (files.Length == 0)
            {
                CommonUtilities.Logger?.Information("No files found on network share. Nothing to move.");
                CommonUtilities.ShowDiagnosticIfVerbose("No files found on network share. Nothing to move.", verbose);
                return 0;
            }

            int filesMoved = 0;
            foreach (var filePath in files)
            {
                var fileName = Path.GetFileName(filePath);
                var destPath = Path.Combine(docSrcPath, fileName);
                try
                {
                    File.Move(filePath, destPath, true);
                }
                catch (IOException)
                {
                    // Move fails across volumes/network shares; fall back to copy+delete
                    File.Copy(filePath, destPath, true);
                    File.Delete(filePath);
                }
                CommonUtilities.Logger?.Information("Moved: '{FilePath}' -> '{DestPath}'", filePath, destPath);
                CommonUtilities.ShowDiagnosticIfVerbose($"Moved: '{filePath}' -> '{destPath}'", verbose);
                filesMoved++;
            }

            CommonUtilities.Logger?.Information("Network share transfer completed. Files moved: {FilesMoved}", filesMoved);
            CommonUtilities.ShowDiagnosticIfVerbose($"Network share transfer completed. Files moved: {filesMoved}", verbose);
            return filesMoved;
        }


    }
}
