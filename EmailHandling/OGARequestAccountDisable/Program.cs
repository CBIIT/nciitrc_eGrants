using System;
using System.Data.SqlClient;
using System.IO;
using CommonUtilties;
using Microsoft.Extensions.Configuration;

namespace OGARequestAccountDisable
{
    /// <summary>
    /// OGA Request Account Disable Application
    /// 
    /// PURPOSE:
    /// Automatically identifies inactive eGrants user accounts and manages the deprovisioning
    /// process with OGA (Office of Grants Administration). This application performs two main tasks:
    /// 1. Sends deprovisioning requests to OGA for accounts that have been inactive for 60 days
    /// 2. Sends warning emails to users whose accounts are approaching the 60-day deactivation threshold
    /// 
    /// WORKFLOW - Disable Task:
    /// 1. Queries database for accounts in people_for_oga_to_disable table (not yet sent to OGA)
    /// 2. Filters out accounts with missing name information
    /// 3. Creates an HTML email with a table of users to be deprovisioned
    /// 4. Sends email to OGA team (or dev team if in debug mode)
    /// 5. Updates database to mark accounts as sent to OGA
    /// 
    /// WORKFLOW - Warning Task:
    /// 1. Queries database for accounts approaching deactivation (46 days of inactivity)
    /// 2. Sends individual warning emails to each user
    /// 3. Tracks warning emails sent in people_sent_warning table
    /// 4. Resends warnings if user logs in and then becomes inactive again
    /// 
    /// DATABASE TABLES:
    /// - people_for_oga_to_disable: Tracks accounts pending OGA deprovisioning
    /// - people_sent_warning: Tracks which users have been sent warning emails
    /// - people: Main user account table
    /// 
    /// EMAIL RECIPIENTS:
    /// - Debug Mode: Emails go to eGrantsDev@mail.nih.gov
    /// - Production Mode: Deprovisioning emails go to NCIOGABOBTeam2@mail.nih.gov
    /// - Warning emails always go to individual users
    /// 
    /// CONFIGURATION:
    /// Uses appsettings.json with environment-specific overrides for configuration management.
    /// 
    /// DEPENDENCIES:
    /// - SQL Server database with EIM schema
    /// - Microsoft Outlook (COM Interop) - must be installed and configured
    /// - CommonUtilities project for logging and diagnostics
    /// 
    /// SCHEDULED TASK:
    /// Typically run as a Windows Scheduled Task on a daily basis to identify
    /// and process inactive accounts.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Main entry point for the OGA Request Account Disable application.
        /// Initializes configuration, runs disable task, then runs warning task.
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
                Console.WriteLine("OGARequestAccountDisable - Account Deprovisioning Manager");

                // Load configuration from shared appsettings.json (via CommonUtilties.AppConfig)
                var config = AppConfig.Load();

                var verbose = config["AppSettings:Verbose"] ?? "n";
                CommonUtilities.ShowDiagnosticIfVerbose($"Verbose mode: '{verbose}'", verbose);

                var logDir = config["AppSettings:LogDir"] ?? @"C:\egrants\apps\log\";
                CommonUtilities.LogDir = logDir;
                CommonUtilities.ShowDiagnosticIfVerbose($"Log directory: '{logDir}'", verbose);

                var conStr = AppConfig.GetConnectionString(config, "EIM");
                CommonUtilities.ShowDiagnosticIfVerbose($"Connection string loaded", verbose);

                // Load email settings that will be passed to processors
                var emailSettings = new EmailSettings
                {
                    EGrantsDevEmail = config["AppSettings:DebugEmail"] ?? "daryl.dehuff@nih.gov",
                    OgaProdEmail = config["EmailSettings:OgaProdEmail"] ?? "NCIOGABOBTeam2@mail.nih.gov",
                    OgaSubject = config["EmailSettings:OgaSubject"] ?? "eGrants: Deprovisioning Request Due to Inactivity ",
                    UserWarningSubject = config["EmailSettings:UserWarningSubject"] ?? "Action Required: eGrants Account Deactivation"
                };

                CommonUtilities.ShowDiagnosticIfVerbose("Running the OGA Request Account Disable Program", verbose);

                // ===== TASK 1: DISABLE ACCOUNTS (Send deprovisioning request to OGA) =====
                int forAppending = 8;
                var taskStartMsg = "...........Disable Task Started!...........";
                CommonUtilities.WriteLog(forAppending, taskStartMsg, null, startTimeStamp);

                SqlConnection con = new SqlConnection(conStr);

                var processor = new Processor(emailSettings);
                var emailsCountRequestedToBeDisabled = processor.Process("", con, verbose);

                var taskEndMsg = $"******* Disable Task Completed! ******* {emailsCountRequestedToBeDisabled} email account(s) have been requested to OGA for disabling";
                var endTimeStamp = DateTime.Now;
                CommonUtilities.WriteLog(forAppending, taskEndMsg, null, endTimeStamp);

                CommonUtilities.ShowDiagnosticIfVerbose("Disable task completed successfully.", verbose);

                // ===== TASK 2: WARNING EMAILS (Send warning to users approaching deactivation) =====
                int forAppending2 = 8;
                var taskStartMsg2 = "...........Warning Task Started!...........";
                var startTimeStamp2 = DateTime.Now;
                CommonUtilities.WriteLog(forAppending2, taskStartMsg2, null, startTimeStamp2);

                var warningProcessor = new ProcessorWarning(emailSettings);
                var emailsCountRequestedToSendWarning = warningProcessor.ProcessWarning("", con, verbose);

                var taskEndMsg2 = $"******* Warning Task Completed! ******* {emailsCountRequestedToSendWarning} warning email(s) have been sent to users";
                var endTimeStamp2 = DateTime.Now;
                CommonUtilities.WriteLog(forAppending2, taskEndMsg2, null, endTimeStamp2);

                CommonUtilities.ShowDiagnosticIfVerbose("Warning task completed successfully.", verbose);
                CommonUtilities.ShowDiagnosticIfVerbose("OGARequestAccountDisable completed successfully.", verbose);
            }
            catch (Exception ex)
            {
                // Log any unhandled exceptions to console and log file
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                try
                {
                    // Attempt to write the error to the log file
                    var logDir = Environment.ExpandEnvironmentVariables(@"C:\egrants\apps\log\");
                    CommonUtilities.WriteLog(8, "Fatal Error in OGARequestAccountDisable", 
                        $"Message: {ex.Message}\nStackTrace: {ex.StackTrace}", DateTime.Now);
                }
                catch { }
            }
        }
    }

    /// <summary>
    /// Email configuration settings for OGA deprovisioning and user warnings.
    /// </summary>
    public class EmailSettings
    {
        public string EGrantsDevEmail { get; set; }
        public string OgaProdEmail { get; set; }
        public string OgaSubject { get; set; }
        public string UserWarningSubject { get; set; }
    }
}
