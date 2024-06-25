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


namespace Router
{
    internal class Program
    {
        static void Main(string[] args)
        {

            var _startTimeStamp = DateTime.Now;

            var _verbose = Utilities.GetConfigVal("Verbose");
            Utilities.ShowDiagnosticIfVerbose($"_verbose: '{_verbose}'", _verbose);
            var _debug = Utilities.GetConfigVal("dBug");
            Utilities.ShowDiagnosticIfVerbose($"_debug: '{_debug}'", _verbose);
            var _logDir = Utilities.GetConfigVal("logDir");
            Utilities.LogDir = _logDir;
            Utilities.ShowDiagnosticIfVerbose($"_logDir: '{_logDir}'", _verbose);
            var _conStr = Utilities.GetConfigVal("conStr");
            Utilities.ShowDiagnosticIfVerbose($"_conStr: '{_conStr}'", _verbose);
            var _dirPath = Utilities.GetConfigVal("dirpathRouter");
            Utilities.ShowDiagnosticIfVerbose($"_dirPath: '{_dirPath}'", _verbose);
            var _routingBreakDurationToken = Utilities.GetConfigVal("routingBreakDuration");
            var _routingBreakDuration = 1000;
            if (!string.IsNullOrWhiteSpace(_routingBreakDurationToken) && !_routingBreakDurationToken.ToLower().Contains("fail"))
            {
                bool success = int.TryParse(_routingBreakDurationToken, out _routingBreakDuration);
                if (!success)
                {
                    _routingBreakDuration = 1000;
                    Utilities.ShowDiagnosticIfVerbose($"Unable to load routingBreakDuration from config : ({_routingBreakDurationToken}), so settin to 1000 milliseconds", _verbose);
                }
            }
            Utilities.ShowDiagnosticIfVerbose($"_routingBreakDuration: '{_routingBreakDuration}'", _verbose);

            Utilities.ShowDiagnosticIfVerbose("Running the router", _verbose);

            int _forAppending = 8;
            var _taskStartMssg = "...........Task Started!...........";
            Utilities.WriteLog(_forAppending, _taskStartMssg, null, _startTimeStamp);

            SqlConnection _con = new SqlConnection(_conStr);

            var processor = new Processor();
            var _itemsProcessed = processor.Process(_dirPath, _con, _verbose, _debug, _routingBreakDuration);

            var _taskEndMssg = $"******* Task Completed! ******* {_itemsProcessed} Mail Items Have Been Processed";
            var _endTimeStamp = DateTime.Now;
            Utilities.WriteLog(_forAppending, _taskEndMssg, null, _endTimeStamp);

            Utilities.ShowDiagnosticIfVerbose("Router.cs completed successfully.", _verbose);


        }
    }
}
