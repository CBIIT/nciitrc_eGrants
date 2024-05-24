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


            var _itemsProcessed = Processor.Process(_dirPath, _con, _verbose, _debug);

            var _taskEndMssg = $"******* Task Completed! ******* {_itemsProcessed} Mail Items Have Been Processed";
            var _endTimeStamp = DateTime.Now;
            Utilities.WriteLog(_forAppending, _taskEndMssg, null, _endTimeStamp);



            // See https://aka.ms/new-console-template for more information
            Console.WriteLine("Hello, World!");

            var sb = new StringBuilder();

            Console.WriteLine("Here we go ...");
            sb.Append("Here we go ...\r\n");
            Outlook.Application oApp = new Outlook.Application();
            Console.WriteLine("Created the object ...");
            sb.Append("Created the object ...\r\n");
            Outlook.NameSpace oNS = oApp.GetNamespace("MAPI");
            oNS.Logon("", "", false, true);
            Outlook.MAPIFolder oInbox = oNS.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderInbox);
            Outlook.Items oItems = oInbox.Items;
            Console.WriteLine(oItems.Count);
            sb.Append($"Found {oItems.Count} many emails\r\n");
            Outlook.MailItem oMsg = (Outlook.MailItem)oItems.GetFirst();
            Console.WriteLine(oMsg.Subject);
            sb.Append($"Here's the first subject : {oMsg.Subject}\r\n");
            oNS.Logoff();
            Console.ReadLine();

        }
    }
}
