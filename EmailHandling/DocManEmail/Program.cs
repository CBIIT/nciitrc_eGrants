using System;
using System.Data.SqlClient;
using System.IO;
using CommonUtilties;

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
                var startTimeStamp = DateTime.Now;
                Console.WriteLine("DocManEmail - Document Management Email Processor");

                var verbose = CommonUtilities.GetConfigVal("Verbose");
                var logDir = CommonUtilities.GetConfigVal("logDir");
                CommonUtilities.LogDir = logDir;
                var conStr = CommonUtilities.GetConfigVal("conStrDocMan");
                var dirPath = CommonUtilities.GetConfigVal("dirpathDocMan");
                var outDir = CommonUtilities.GetConfigVal("OutDirDocMan");

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
                ? $"{timeStamp}  -\t{message}"
                : $"{timeStamp}  -\t{message}\r\n\t\t-> {errorInfo}";
            File.AppendAllText(Path.Combine(logDir, fileName), content + Environment.NewLine);
        }
    }
}
