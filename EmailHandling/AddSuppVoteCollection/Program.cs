using System;
using System.IO;
using CommonUtilties;

namespace AddSuppVoteCollection
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var startTimeStamp = DateTime.Now;
                Console.WriteLine("AddSuppVoteCollection - Vote Collection Processor");
                var verbose = CommonUtilities.GetConfigVal("Verbose");
                var logDir = CommonUtilities.GetConfigVal("logDir");
                CommonUtilities.LogDir = logDir;
                var dirPath = CommonUtilities.GetConfigVal("dirpathVoteCollection");
                WriteLog("Task Started", null, startTimeStamp, logDir);
                var processor = new Processor();
                var itemsProcessed = processor.Process(dirPath, verbose, logDir);
                WriteLog("Task Completed", null, DateTime.Now, logDir);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        public static void WriteLog(string message, string errorInfo, DateTime timeStamp, string logDir)
        {
            var fileName = "Supp-VoteColl-Log-" + timeStamp.ToString("yyyy-M-d") + ".txt";
            var content = string.IsNullOrEmpty(errorInfo) ? timeStamp + "  -\t" + message : timeStamp + "  -\t" + message + "\r\n\t\t-> " + errorInfo;
            File.AppendAllText(Path.Combine(logDir, fileName), content + Environment.NewLine);
        }
    }
}
