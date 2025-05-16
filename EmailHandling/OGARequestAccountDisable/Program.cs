using CommonUtilties;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OGARequestAccountDisable
{
    internal class Program
    {
        static void Main(string[] args)
        {

            var _startTimeStamp = DateTime.Now;

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

            CommonUtilities.ShowDiagnosticIfVerbose("Running the OGA Request Account Disable Program", _verbose);

            int _forAppending = 8;
            var _taskStartMssg = "...........Disable Task Started!...........";
            CommonUtilities.WriteLog(_forAppending, _taskStartMssg, null, _startTimeStamp);

            SqlConnection _con = new SqlConnection(_conStr);

            var processor = new Processor();
            var _emailsCountRequestedToBeDisabled = processor.Process(_dirPath, _con, _verbose, _debug);

            var _taskEndMssg = $"******* Disable Task Completed! ******* {_emailsCountRequestedToBeDisabled} many email accounts have been requested to OGA for disabling";
            var _endTimeStamp = DateTime.Now;
            CommonUtilities.WriteLog(_forAppending, _taskEndMssg, null, _endTimeStamp);

            CommonUtilities.ShowDiagnosticIfVerbose("OGARequestAccountDisable.cs completed successfully.", _verbose);

            int _forAppending2 = 8;
            var _taskStartMssg2 = "...........Warning Task Started!...........";
            var _startTimeStamp2 = DateTime.Now;
            CommonUtilities.WriteLog(_forAppending2, _taskStartMssg2, null, _startTimeStamp2);

            var warningProcessor = new ProcessorWarning();
            var _emailsCountRequestedToSendWarning = warningProcessor.ProcessWarning(_dirPath, _con, _verbose, _debug);

            var _taskEndMssg2 = $"******* Warning Task Completed! ******* {_emailsCountRequestedToSendWarning} many email accounts have been requested to OGA for disabling";
            var _endTimeStamp2 = DateTime.Now;
            CommonUtilities.WriteLog(_forAppending2, _taskEndMssg2, null, _endTimeStamp2);

            CommonUtilities.ShowDiagnosticIfVerbose("OGARequestAccountDisable.cs completed successfully.", _verbose);
        }
    }
}
