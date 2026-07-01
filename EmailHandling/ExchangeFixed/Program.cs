using System;
using System.Data.SqlClient;
using System.IO;
using CommonUtilties;
using Microsoft.Extensions.Configuration;

namespace ExchangeFixed
{
    /// <summary>
    /// ExchangeFixed - Fixed Path Email Router
    /// 
    /// PURPOSE:
    /// This application processes emails from a fixed Outlook public folder path,
    /// extracting document content and/or attachments and filing them into the eGrants
    /// document management system. It parses structured subject lines to determine
    /// how each email should be categorized and stored.
    /// 
    /// ORIGINAL SOURCE: Migrated from exchange_Fixed.vbs (also known as exchange_latest.vbs)
    /// 
    /// WORKFLOW:
    /// 1. Reads configuration from appsettings.json (folder path, connection string, output dir)
    /// 2. Connects to the configured Outlook public folder via COM automation
    /// 3. For each email in the folder, parses the subject line for metadata:
    ///    - grantnumber: Grant number used to look up the application ID
    ///    - applid: Direct application ID (bypasses grant number lookup)
    ///    - category: Document category (defaults to "Correspondence")
    ///    - sub: Sub-category for further classification
    ///    - extract: Controls what to save (1=body text, 2=attachment, 3=both)
    ///    - documentdate: Document date (defaults to email received time)
    ///    - documentid: Existing document ID for updates
    /// 4. Calls SP_CREATE_EGRANTS_DOCUMENT_NEW to register the document and get a file number
    /// 5. Saves content based on category and extract mode:
    ///    - Standard categories: saves as .txt or attachment files
    ///    - PublicAccess/JIT Info/CT.gov/Closeout: generates PDF via Word or Acrobat
    /// 6. Moves processed emails to the "old" archive subfolder
    /// 7. Sends error notifications to admin team if processing fails
    /// 8. Limits processing to 30 items per run to prevent duplicate processing
    /// 
    /// DEPENDENCIES:
    /// - CommonUtilties project (for logging)
    /// - Microsoft Outlook (COM Interop) - must be installed and configured
    /// - Microsoft Word (COM Interop) - for PDF generation with embedded images
    /// - Adobe Acrobat SDK (COM Interop) - for PDF merging (PublicAccess category)
    /// - SQL Server EIM database with:
    ///   - SP_CREATE_EGRANTS_DOCUMENT_NEW stored procedure
    ///   - SP_CLEAR_OLD_JIT_SUBMISSIONS stored procedure
    ///   - dbo.Imm_fn_applid_match() scalar function
    /// - File system write access to the output directory and PDF working directory
    /// 
    /// SCHEDULED TASK:
    /// Typically run as a Windows Scheduled Task to process incoming eFile emails.
    /// </summary>
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
#if DEBUG
                Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
#endif

                var startTimeStamp = DateTime.Now;
                Console.WriteLine("ExchangeFixed - Fixed Path Email Router");

                // Diagnostic: Check what environment variable is set
                var dotnetEnv = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
                Console.WriteLine($"DOTNET_ENVIRONMENT: {dotnetEnv ?? "(not set)"}");

                // Load configuration from shared appsettings.json (via CommonUtilties.AppConfig)
                var config = AppConfig.Load();

                var verbose = config["AppSettings:Verbose"] ?? "n";
                var logDir = config["AppSettings:LogDir"] ?? @"C:\egrants\apps\log\";
                var conStr = AppConfig.GetConnectionString(config, "EIM");
                var dirPath = config["FolderPaths:dirpathFixed"];
                var outDir = config["AppSettings:OutDir"] ?? @"C:\eGrants\watch\out\";
                var publicAccessBackup = config["AppSettings:PublicAccessBackup"] ?? @"C:\eGrants\publicaccess\";
                var adminRecipients = config["AppSettings:AdminRecipients"];

                // Initialize Serilog logging (creates log directory and configures file + console sinks)
                CommonUtilities.InitializeLogging("ExchangeFixed", logDir);

                CommonUtilities.WriteLog(8, "...........Task Started!...........", null, startTimeStamp);
                CommonUtilities.ShowDiagnosticIfVerbose("Exchange_latest script is going to run!!", verbose);

                using (var con = new SqlConnection(conStr))
                {
                    var processor = new Processor();
                    var itemsProcessed = processor.Process(dirPath, con, verbose, outDir, publicAccessBackup, adminRecipients);
                    CommonUtilities.WriteLog(8, $"******* Task Completed! ******* {itemsProcessed} Mail Items Have Been Processed", null, DateTime.Now);
                }

                CommonUtilities.ShowDiagnosticIfVerbose("Done", verbose);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                CommonUtilities.WriteLog(8, "Fatal Error in ExchangeFixed", 
                    $"Message: {ex.Message}\nStackTrace: {ex.StackTrace}", 
                    DateTime.Now);
            }
            finally
            {
                CommonUtilities.CloseLogging();
            }
        }
    }
}
