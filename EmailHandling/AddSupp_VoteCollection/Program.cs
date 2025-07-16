using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonUtilties;

namespace AddSupp_VoteCollection
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
            var _logDir = CommonUtilities.GetConfigVal("logDirDEV");
            CommonUtilities.LogDir = _logDir;
            CommonUtilities.ShowDiagnosticIfVerbose($"_logDir: '{_logDir}'", _verbose);
            var _conStr = CommonUtilities.GetConfigVal("conStr");
            CommonUtilities.ShowDiagnosticIfVerbose($"_conStr: '{_conStr}'", _verbose);
            var _dirPath = CommonUtilities.GetConfigVal("dirpathRouterDEV"); //"NCIOGAeGrantsProd@mail.nih.gov\Inbox\"	
            CommonUtilities.ShowDiagnosticIfVerbose($"_dirPath: '{_dirPath}'", _verbose);

            CommonUtilities.ShowDiagnosticIfVerbose("Running the Add Supp Vote Collection Program", _verbose);

            int _forAppending = 8;
            var _taskStartMssg = "...........Add Supp Vote Task Started!...........";
            //CommonUtilities.WriteLog(_forAppending, _taskStartMssg, null, _startTimeStamp);

            var processor = new Processor();
            var _itemsProcessed = processor.Process(_dirPath, _verbose, _debug);

            var _taskEndMssg = $"******* Add Supp Vote Task Completed! *******";
            var _endTimeStamp = DateTime.Now;
            //CommonUtilities.WriteLog(_forAppending, _taskEndMssg, null, _endTimeStamp);
        }
    }
}
