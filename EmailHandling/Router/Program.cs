using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using Router;
using Outlook = Microsoft.Office.Interop.Outlook;
using CommonUtilties;
using System.IO;


namespace Router
{
    internal class Program
    {
        static void Main(string[] args)
        {

            var _startTimeStamp = DateTime.Now;

            Console.WriteLine("The current directory is {0}", Directory.GetCurrentDirectory());

            var _verbose = CommonUtilities.GetConfigVal("Verbose");
            CommonUtilities.ShowDiagnosticIfVerbose($"_verbose: '{_verbose}'", _verbose);
            var _debug = CommonUtilities.GetConfigVal("dBug");
            CommonUtilities.ShowDiagnosticIfVerbose($"_debug: '{_debug}'", _verbose);
            var _logDir = CommonUtilities.GetConfigVal("logDir");
            CommonUtilities.LogDir = _logDir;
            CommonUtilities.ShowDiagnosticIfVerbose($"_logDir: '{_logDir}'", _verbose);
            var _conStr = CommonUtilities.GetConfigVal("conStr");
            CommonUtilities.ShowDiagnosticIfVerbose($"_conStr: '{_conStr}'", _verbose);
            var _dirPath = CommonUtilities.GetConfigVal("dirpathRouter");
            CommonUtilities.ShowDiagnosticIfVerbose($"_dirPath: '{_dirPath}'", _verbose);
            var _routingBreakDurationToken = CommonUtilities.GetConfigVal("routingBreakDuration");
            var _routingBreakDuration = 1000;
            if (!string.IsNullOrWhiteSpace(_routingBreakDurationToken) && !_routingBreakDurationToken.ToLower().Contains("fail"))
            {
                bool success = int.TryParse(_routingBreakDurationToken, out _routingBreakDuration);
                if (!success)
                {
                    _routingBreakDuration = 1000;
                    CommonUtilities.ShowDiagnosticIfVerbose($"Unable to load routingBreakDuration from config : ({_routingBreakDurationToken}), so settin to 1000 milliseconds", _verbose);
                }
            }
            CommonUtilities.ShowDiagnosticIfVerbose($"_routingBreakDuration: '{_routingBreakDuration}'", _verbose);

            CommonUtilities.ShowDiagnosticIfVerbose("Running the router", _verbose);

            int _forAppending = 8;
            var _taskStartMssg = "...........Task Started!...........";
            CommonUtilities.WriteLog(_forAppending, _taskStartMssg, null, _startTimeStamp);

            SqlConnection _con = new SqlConnection(_conStr);

            var processor = new Processor();
            var _itemsProcessed = processor.Process(_dirPath, _con, _verbose, _debug, _routingBreakDuration);

            var _taskEndMssg = $"******* Task Completed! ******* {_itemsProcessed} Mail Items Have Been Processed";
            var _endTimeStamp = DateTime.Now;
            CommonUtilities.WriteLog(_forAppending, _taskEndMssg, null, _endTimeStamp);

            CommonUtilities.ShowDiagnosticIfVerbose("Router.cs completed successfully.", _verbose);


        }
    }
}
