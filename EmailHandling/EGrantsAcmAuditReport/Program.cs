using System;
using System.Data.SqlClient;
using System.IO;
using CommonUtilties;

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
                var startTimeStamp = DateTime.Now;
                Console.WriteLine("EGrantsAcmAuditReport - ACM Audit Report Processor");

                var verbose = CommonUtilities.GetConfigVal("Verbose");
                var logDir = CommonUtilities.GetConfigVal("logDir");
                CommonUtilities.LogDir = logDir;
                var conStr = CommonUtilities.GetConfigVal("conStr");
                var srcDir = CommonUtilities.GetConfigVal("srcDirPathAcm");
                var bckDir = CommonUtilities.GetConfigVal("bckDirPathAcm");
                var imgSvrPath = CommonUtilities.GetConfigVal("imgSvrPathAcm");
                var imgSvrPath2 = CommonUtilities.GetConfigVal("imgSvrPath2Acm");

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
