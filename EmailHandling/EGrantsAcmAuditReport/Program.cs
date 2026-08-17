using System;
using System.Data.SqlClient;
using System.IO;
using CommonUtilties;
using Microsoft.Extensions.Configuration;

namespace EGrantsAcmAuditReport
{
    /// <summary>
    /// eGrants ACM Audit Report - Migrated from eGrants_ACM_Audit_report.vbs
    /// Processes and uploads ACM monthly audit reports.
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
                Console.WriteLine("EGrantsAcmAuditReport - ACM Audit Report Processor");

                // Load configuration from shared appsettings.json (via CommonUtilties.AppConfig)
                var config = AppConfig.Load();

                var verbose = config["AppSettings:Verbose"] ?? "n";
                var logDir = config["AppSettings:LogDir"] ?? @"C:\eGrants\apps\log\";
                var conStr = AppConfig.GetConnectionString(config, "EIM");
                var srcDir = config["AcmAuditReport:SrcDir"];
                var bckDir = config["AcmAuditReport:BckDir"];
                var imgSvrPath = config["AcmAuditReport:ImgSvrPath"];
                var imgSvrPath2 = config["AcmAuditReport:ImgSvrPath2"];

                CommonUtilities.LogDir = logDir;

                WriteLog("...........Task Started!...........", null, startTimeStamp, logDir);

                using (var con = new SqlConnection(conStr))
                {
                    var processor = new Processor();
                    var filesProcessed = processor.Process(con, srcDir, bckDir, imgSvrPath, imgSvrPath2, verbose, logDir);
                    WriteLog($"Task Completed! {filesProcessed} files processed.", null, DateTime.Now, logDir);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public static void WriteLog(string message, string errorInfo, DateTime timeStamp, string logDir)
        {
            var fileName = $"ACM-Audit-Log-{timeStamp:yyyy-M-d}.txt";
            var content = string.IsNullOrEmpty(errorInfo)
      ? $"{timeStamp}  -\t{message}"
           : $"{timeStamp}  -\t{message}\r\n\t\t-> {errorInfo}";
            File.AppendAllText(Path.Combine(logDir, fileName), content + Environment.NewLine);
        }
    }
}
