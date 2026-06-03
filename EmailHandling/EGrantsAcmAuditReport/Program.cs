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

                // Load credentials from shared secrets file in the solution root (if present)
                var secretsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "secrets.local.csv");
                CommonUtilities.LoadLocalSecrets(secretsPath);

                var startTimeStamp = DateTime.Now;
                Console.WriteLine("EGrantsAcmAuditReport - ACM Audit Report Processor");

                // Build configuration from appsettings.json files
                var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: false)
                    .AddEnvironmentVariables()
                    .Build();

                // Load configuration values from appsettings
                var verbose = configuration["AppSettings:Verbose"] ?? "n";
                var logDir = Environment.ExpandEnvironmentVariables(configuration["AppSettings:LogDir"] ?? @"C:\eGrants\apps\log\");
                var conStr = Environment.ExpandEnvironmentVariables(configuration["ConnectionStrings:EIM"]);
                var srcDir = configuration["AcmAuditReport:SrcDir"];
                var bckDir = configuration["AcmAuditReport:BckDir"];
                var imgSvrPath = configuration["AcmAuditReport:ImgSvrPath"];
                var imgSvrPath2 = configuration["AcmAuditReport:ImgSvrPath2"];

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
