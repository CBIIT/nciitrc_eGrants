using System;
using System.Data.SqlClient;
using System.IO;
using CommonUtilties;
using Microsoft.Extensions.Configuration;

namespace DocManEmail
{
    /// <summary>
    /// Document Management Email Processor - Migrated from DocMan_email_2008_Prod.vbs
    /// Processes emails from eContracts public folder for document management.
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
                Console.WriteLine("DocManEmail - Document Management Email Processor");

                // Load configuration from shared appsettings.json (via CommonUtilties.AppConfig)
                var config = AppConfig.Load();

                var verbose = config["AppSettings:Verbose"] ?? "n";
                var logDir = config["AppSettings:LogDir"] ?? @"C:\eGrants\apps\log\";
                var conStr = AppConfig.GetConnectionString(config, "DocMan");
                var dirPath = config["FolderPaths:dirpathDocMan"];
                var outDir = config["DocMan:OutDir"] ?? @"C:\egrants\watch\out\docman\";

                CommonUtilities.LogDir = logDir;

                WriteLog("...........Task Started!...........", null, startTimeStamp, logDir);

                using (var con = new SqlConnection(conStr))
                {
                    var processor = new Processor();
                    var itemsProcessed = processor.Process(con, dirPath, outDir, verbose, logDir);
                    WriteLog($"******* Task Completed! ******* {itemsProcessed} Mail Items Processed", null, DateTime.Now, logDir);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public static void WriteLog(string message, string errorInfo, DateTime timeStamp, string logDir)
        {
            var fileName = $"DocMan-Email-Log-{timeStamp:yyyy-M-d}.txt";
            var content = string.IsNullOrEmpty(errorInfo)
                ? $"{timeStamp}: {message}"
                : $"{timeStamp}: {message}\n{errorInfo}";
            File.AppendAllText(Path.Combine(logDir, fileName), content + Environment.NewLine);
        }
    }
}
