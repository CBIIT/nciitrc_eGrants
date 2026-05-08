using System;
using System.Data.SqlClient;
using System.IO;
using CommonUtilties;

namespace AddSuppProd
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var startTimeStamp = DateTime.Now;
                Console.WriteLine("AddSuppProd - Administrative Supplement Production Processor");
                var verbose = CommonUtilities.GetConfigVal("Verbose");
                var logDir = CommonUtilities.GetConfigVal("logDir");
                CommonUtilities.LogDir = logDir;
                var conStr = CommonUtilities.GetConfigVal("conStr");
                var dirPath = CommonUtilities.GetConfigVal("dirpathSupplement");
                var outDir = CommonUtilities.GetConfigVal("OutDir");
                WriteLog("Task Started", null, startTimeStamp, logDir);
                using (var con = new SqlConnection(conStr))
                {
                    var processor = new Processor();
                    var itemsProcessed = processor.Process(con, dirPath, outDir, verbose, logDir);
                    WriteLog("Task Completed", null, DateTime.Now, logDir);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        public static void WriteLog(string message, string errorInfo, DateTime timeStamp, string logDir)
        {
            var fileName = "AddSupp-Prod-Log-" + timeStamp.ToString("yyyy-M-d") + ".txt";
            var content = string.IsNullOrEmpty(errorInfo) ? timeStamp + "  -\t" + message : timeStamp + "  -\t" + message + "\r\n\t\t-> " + errorInfo;
            File.AppendAllText(Path.Combine(logDir, fileName), content + Environment.NewLine);
        }
    }
}
