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
                CommonUtilities.InitializeLogging("Router", _logDir);
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

                CommonUtilities.Logger?.Information("Program: Creating Processor and starting Process with dirPath='{DirPath}', verbose='{Verbose}', debug='{Debug}', routingBreakDuration={BreakDuration}",
                    _dirPath, _verbose, _debug, _routingBreakDuration);
                var processor = new Processor(config);
                var _itemsProcessed = processor.Process(_dirPath, _con, _verbose, _debug, _routingBreakDuration);
                CommonUtilities.Logger?.Information("Program: Process completed. Items processed: {Count}", _itemsProcessed);

                var _taskEndMssg = $"******* Task Completed! ******* {_itemsProcessed} Mail Items Have Been Processed";
                var _endTimeStamp = DateTime.Now;
                CommonUtilities.WriteLog(_forAppending, _taskEndMssg, null, _endTimeStamp);

                CommonUtilities.ShowDiagnosticIfVerbose("Router.cs completed successfully.", _verbose);
            }
            catch (System.Exception ex)
            {
                string message = $"An unanticipated failure was caught at the global level at {DateTime.UtcNow} UTC. Here is some info : {ex.Message} \r\n {ex.ToString()}";
                Console.WriteLine(message);
                CommonUtilities.ShowDiagnosticIfVerbose(message, "y");
                CommonUtilities.Logger?.Fatal(message);

                try
                {
                    // Send error notification via SMTP (no Outlook dependency)
                    CommonUtilities.Logger?.Information("Program: Attempting to send global error notification via SMTP...");
                    var config = AppConfig.Load();
                    var smtpService = new SmtpEmailService(config);
                    var errorRecipients = config["EmailRecipients:ErrorNotificationRecipients"] ?? "egrantsdevs@mail.nih.gov;leul.ayana@nih.gov";
                    var envPrefix = GetEnvironmentPrefix();
                    CommonUtilities.Logger?.Information("Program: Sending global error email to '{Recipients}'", errorRecipients);
                    smtpService.SendEmail(errorRecipients, envPrefix + "Global level email failure.", message);
                    CommonUtilities.Logger?.Information("Program: Global error notification sent successfully.");
                }
                catch (System.Exception emailEx)
                {
                    Console.WriteLine($"Failed to send error notification email: {emailEx.Message}");
                }
            }
            finally
            {
                CommonUtilities.CloseLogging();
            }

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
    }
}
