using System;
using System.Data.SqlClient;
using System.IO;
using CommonUtilties;

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

                var processor = new Processor(config);
                var _itemsProcessed = processor.Process(_dirPath, _con, _verbose, _debug, _routingBreakDuration);

                var _taskEndMssg = $"******* Task Completed! ******* {_itemsProcessed} Mail Items Have Been Processed";
                var _endTimeStamp = DateTime.Now;
                CommonUtilities.WriteLog(_forAppending, _taskEndMssg, null, _endTimeStamp);

                CommonUtilities.ShowDiagnosticIfVerbose("Router.cs completed successfully.", _verbose);
            }
            catch (System.Exception ex)
            {
                string message = $"An unanticipated failure was caught at the global level at {DateTime.UtcNow} UTC. You might need to restart Outlook. Here is some info : {ex.Message} \r\n {ex.ToString()}";
                Console.WriteLine(message);
                CommonUtilities.ShowDiagnosticIfVerbose(message, "y");

                try
                {
                    SendGlobalErrorEmail(message);
                }
                catch (System.Exception emailEx)
                {
                    Console.WriteLine($"Failed to send error notification email: {emailEx.Message}");
                }
            }

        }

        /// <summary>
        /// Isolated in a separate method so that Outlook COM types are not JIT-compiled
        /// as part of Main(). This prevents a blank-screen crash when Outlook is not available.
        /// </summary>
        private static void SendGlobalErrorEmail(string message)
        {
            Type outlookType = Type.GetTypeFromProgID("Outlook.Application");
            if (outlookType == null)
                throw new InvalidOperationException("Outlook.Application COM class not found. Is Outlook installed?");

            dynamic oApp = GetRunningOutlook() ?? Activator.CreateInstance(outlookType);
            dynamic oNS = oApp.GetNamespace("MAPI");

            // CreateItem(0) = olMailItem
            dynamic mailItem = oApp.CreateItem(0);

            var envPrefix = GetEnvironmentPrefix();
            mailItem.Subject = envPrefix + "Global level email failure.";

            try
            {
                var config = AppConfig.Load();
                var errorRecipients = config["EmailRecipients:ErrorNotificationRecipients"] ?? "egrantsdevs@mail.nih.gov;leul.ayana@nih.gov";
                mailItem.To = errorRecipients;
            }
            catch
            {
                mailItem.To = "egrantsdevs@mail.nih.gov;leul.ayana@nih.gov";
            }

            mailItem.HTMLBody = message;
            mailItem.BodyFormat = 2; // olFormatHTML
            mailItem.Send();
        }

        /// <summary>
        /// Returns the environment name in parentheses (e.g. "(Development) ") if not Production.
        /// Returns empty string for Production or if DOTNET_ENVIRONMENT is not set.
        /// </summary>
        private static string GetEnvironmentPrefix()
        {
            var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
            if (string.IsNullOrWhiteSpace(env) || env.Equals("Production", StringComparison.OrdinalIgnoreCase))
                return string.Empty;
            return $"({env}) ";
        }

        [System.Runtime.InteropServices.DllImport("oleaut32.dll", PreserveSig = false)]
        private static extern void GetActiveObject(
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPStruct)] Guid clsid,
            IntPtr reserved,
            [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.IUnknown)] out object obj);

        private static dynamic GetRunningOutlook()
        {
            try
            {
                var clsid = new Guid("0006F03A-0000-0000-C000-000000000046"); // Outlook.Application CLSID
                GetActiveObject(clsid, IntPtr.Zero, out object obj);
                return obj;
            }
            catch
            {
                return null;
            }
        }
    }
}
