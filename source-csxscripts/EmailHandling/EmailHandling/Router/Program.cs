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

            Utilities.ShowDiagnosticIfVerbose("Running the router", _verbose);

            int _forAppending = 8;
            var _taskStartMssg = "...........Task Started!...........";
            Utilities.WriteLog(_forAppending, _taskStartMssg, null, _startTimeStamp);

            // moved to Process
            //var _dBugEmail = "leul.ayana@nih.gov";
            //var _eGrantsDevEmail = "eGrantsDev@mail.nih.gov";
            //var _eGrantsTestEmail = "eGrantsTest1@mail.nih.gov";
            //var _eGrantsStageEmail = "eGrantsStage@mail.nih.gov";
            //var _eFileEmail = "efile@mail.nih.gov";
            //var _nciGrantsPostAwardEmail = "NCIGrantsPostAward@nih.gov";

            // System.ArgumentException: 'Keyword not supported: 'provider'.'
            SqlConnection _con = new SqlConnection(_conStr);

            // TODO :
            // Test getNthWord
            // Add error layer

            var processor = new Processor();
            var _itemsProcessed = processor.Process(_dirPath, _con, _verbose, _debug);

            var _taskEndMssg = $"******* Task Completed! ******* {_itemsProcessed} Mail Items Have Been Processed";
            var _endTimeStamp = DateTime.Now;
            Utilities.WriteLog(_forAppending, _taskEndMssg, null, _endTimeStamp);

            Utilities.ShowDiagnosticIfVerbose("Router.cs completed successfully.", _verbose);


        }
    }
}
