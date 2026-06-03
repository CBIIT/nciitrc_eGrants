using System;
using System.Data.SqlClient;
using System.IO;
using CommonUtilties;

namespace LoadPfr
{
    /// <summary>
    /// Load PFR (Progress/Final Report) - Migrated from Load_PFR.vbs
    /// Loads PFR from XML files with metadata.
    /// </summary>
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
#if DEBUG
                // Load credentials from shared secrets file in the solution root (not committed to source control)
                CommonUtilities.LoadLocalSecrets(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "secrets.local.csv"));
#endif

                var startTimeStamp = DateTime.Now;
                Console.WriteLine("LoadPfr - Progress/Final Report Loader");

                var verbose = CommonUtilities.GetConfigVal("Verbose");
                var logDir = CommonUtilities.GetConfigVal("logDir");
                CommonUtilities.LogDir = logDir;
                var conStr = CommonUtilities.GetConfigVal("conStr");
                var docSrcPath = CommonUtilities.GetConfigVal("docSrcPathPfr");
                var bakDstPath = CommonUtilities.GetConfigVal("bakDstPathPfr");
                var finalDstPath = CommonUtilities.GetConfigVal("finalDstPathPfr");

                WriteLog(".........Task Started!........", null, startTimeStamp, logDir);

                using (var con = new SqlConnection(conStr))
                {
                    var processor = new Processor();
                    var filesProcessed = processor.Process(con, docSrcPath, bakDstPath, finalDstPath, verbose, logDir);
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
            var fileName = $"PFR-Log-{timeStamp:yyyy-M-d}.txt";
            var content = string.IsNullOrEmpty(errorInfo)
                ? $"{timeStamp}  -\t{message}"
                : $"{timeStamp}  -\t{message}\r\n\t\t-> {errorInfo}";
            File.AppendAllText(Path.Combine(logDir, fileName), content + Environment.NewLine);
        }
    }
}
