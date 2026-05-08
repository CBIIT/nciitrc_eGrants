using System;
using System.Data.SqlClient;
using CommonUtilties;

namespace ExchangeLatest
{
    /// <summary>
    /// Exchange Latest Email Router - Migrated from exchange_latest.vbs
    /// Processes emails from efile public folder.
 /// </summary>
    internal class Program
    {
  static void Main(string[] args)
  {
    try
       {
   var startTimeStamp = DateTime.Now;
  Console.WriteLine("ExchangeLatest - eFile Email Router");

       var verbose = CommonUtilities.GetConfigVal("Verbose");
   var logDir = CommonUtilities.GetConfigVal("logDir");
       CommonUtilities.LogDir = logDir;
 var conStr = CommonUtilities.GetConfigVal("conStr");
     var dirPath = CommonUtilities.GetConfigVal("dirpathLatest");
  var outDir = CommonUtilities.GetConfigVal("OutDir");

  CommonUtilities.WriteLog(8, "...........Task Started!...........", null, startTimeStamp);

        using (var con = new SqlConnection(conStr))
         {
   var processor = new Processor();
var itemsProcessed = processor.Process(dirPath, con, verbose, outDir);
   CommonUtilities.WriteLog(8, $"******* Task Completed! ******* {itemsProcessed} Items Processed", null, DateTime.Now);
    }
    }
  catch (Exception ex)
     {
       Console.WriteLine($"Error: {ex.Message}");
    }
 }
    }
}
