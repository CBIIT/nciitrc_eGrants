using System;
using System.Data.SqlClient;
using System.IO;
using CommonUtilties;
using Outlook = Microsoft.Office.Interop.Outlook;
using Microsoft.Office.Interop.Outlook;

namespace Router
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
#if DEBUG
                Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
#endif

                var _startTimeStamp = DateTime.Now;
                Console.WriteLine("Router - Email Router");

                // Load configuration from shared appsettings.json (via CommonUtilties.AppConfig)
                var config = AppConfig.Load();

                var _verbose = config["AppSettings:Verbose"] ?? "n";
                CommonUtilities.ShowDiagnosticIfVerbose($"_verbose: '{_verbose}'", _verbose);
                var _debug = config["AppSettings:dBug"] ?? "n";
                CommonUtilities.ShowDiagnosticIfVerbose($"_debug: '{_debug}'", _verbose);
                var _logDir = config["AppSettings:LogDir"] ?? @"C:\egrants\apps\log\";
                CommonUtilities.LogDir = _logDir;
                CommonUtilities.ShowDiagnosticIfVerbose($"_logDir: '{_logDir}'", _verbose);
                var _conStr = AppConfig.GetConnectionString(config, "EIM");
                CommonUtilities.ShowDiagnosticIfVerbose($"_conStr loaded", _verbose);
                var _dirPath = config["FolderPaths:dirpathRouter"];
                CommonUtilities.ShowDiagnosticIfVerbose($"_dirPath: '{_dirPath}'", _verbose);
                var _routingBreakDurationToken = config["AppSettings:RoutingBreakDuration"];
                var _routingBreakDuration = 1000;
                if (!string.IsNullOrWhiteSpace(_routingBreakDurationToken) && !_routingBreakDurationToken.ToLower().Contains("fail"))
                {
                    bool success = int.TryParse(_routingBreakDurationToken, out _routingBreakDuration);
                    if (!success)
                    {
                        _routingBreakDuration = 1000;
                        CommonUtilities.ShowDiagnosticIfVerbose($"Unable to load routingBreakDuration from config : ({_routingBreakDurationToken}), so setting to 1000 milliseconds", _verbose);
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
            catch (System.Exception ex)
            {
                string message = $"An unanticipated failure was caught at the global level at {DateTime.UtcNow} UTC. You might need to restart Outlook. Here is some info : {ex.Message} \r\n {ex.ToString()}";
                CommonUtilities.ShowDiagnosticIfVerbose(message, "y");
                Outlook.Application oApp = new Outlook.Application();
                CommonUtilities.ShowDiagnosticIfVerbose("Created the outlook object.", "y");
                Outlook.NameSpace oNS = oApp.GetNamespace("MAPI");

                Outlook.MailItem mailItem =
                    (Outlook.MailItem)oApp.CreateItem(Outlook.OlItemType.olMailItem);

                mailItem.Subject = "Global level email failure.";
                mailItem.To = "egrantsdevs@mail.nih.gov;leul.ayana@nih.gov";
                mailItem.HTMLBody = message;
                mailItem.BodyFormat = OlBodyFormat.olFormatHTML;
                mailItem.Send();
            }

        }
    }
}
