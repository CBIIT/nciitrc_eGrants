using System;
using System.Data.SqlClient;
using System.IO;
using CommonUtilties;

namespace ExchangeFixed
{
  /// <summary>
 /// Exchange Fixed Email Router - Migrated from exchange_Fixed.vbs
    /// Processes emails from a configurable fixed public folder path.
    /// </summary>
    internal class Program
    {
        static void Main(string[] args)
   {
     try
   {
     var startTimeStamp = DateTime.Now;
                Console.WriteLine("ExchangeFixed - Fixed Path Email Router");

           var verbose = CommonUtilities.GetConfigVal("Verbose");
          var logDir = CommonUtilities.GetConfigVal("logDir");
      CommonUtilities.LogDir = logDir;
     var conStr = CommonUtilities.GetConfigVal("conStr");
        var dirPath = CommonUtilities.GetConfigVal("dirpathFixed");
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
