using System;
using System.Diagnostics;
using System.IO;
using Serilog;
using Serilog.Events;

namespace CommonUtilties
{
    /// <summary>
    /// CommonUtilities - Shared Utility Library for eGrants Email Handling Projects
    ///
    /// Provides common utility functions for:
    /// - Configuration reading (with environment variable expansion)
    /// - Structured logging (Serilog)
    /// - String and file utilities
    /// - Secure secrets management (never hardcoded, always via environment variables)
    ///
    /// # Best Practices
    /// - Never hardcode or commit secrets.
    /// - Use environment variables for all secrets.
    /// - Use a gitignored secrets file (secrets.local.csv) for local dev, loaded at startup.
    /// - Reference secrets in config using environment variable syntax (e.g., %DB_USER%).
    /// - Provide a template secrets file for onboarding.
    /// - Use secret scanning tools in your CI pipeline.
    /// </summary>
    public class CommonUtilities
    {
        /// <summary>
        /// Gets or sets the log directory path.
        /// </summary>
        public static string LogDir { get; set; }

        /// <summary>
        /// Gets the Serilog logger instance for structured logging.
        /// </summary>
        public static ILogger Logger { get; private set; }

        /// <summary>
        /// Default constructor. Initializes LogDir to empty string.
        /// </summary>
        public CommonUtilities()
        {
            LogDir = string.Empty;
        }

        /// <summary>
        /// Initializes Serilog logging with file and console sinks.
        /// </summary>
        public static void InitializeLogging(string applicationName, string logDirectory, LogEventLevel minimumLevel = LogEventLevel.Information)
        {
            LogDir = logDirectory;

            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            var logFilePath = Path.Combine(logDirectory, $"{applicationName}-.log");

            Logger = new LoggerConfiguration()
                        .MinimumLevel.Is(minimumLevel)
                 .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                 .WriteTo.File(
                      logFilePath,
                     rollingInterval: RollingInterval.Day,
               outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                    retainedFileCountLimit: 31,
              fileSizeLimitBytes: 10_000_000,
                       rollOnFileSizeLimit: true)
             .Enrich.WithProperty("Application", applicationName)
                .CreateLogger();

            Logger.Information("Logging initialized for {ApplicationName}", applicationName);
        }

        /// <summary>
        /// Closes and flushes all pending log entries.
        /// </summary>
        public static void CloseLogging()
        {
            Logger?.Information("Logging shutting down");
            (Logger as IDisposable)?.Dispose();
            Log.CloseAndFlush();
        }

        /// <summary>
        /// Outputs diagnostic message to console and debug output if verbose mode is enabled.
        /// </summary>
        public static void ShowDiagnosticIfVerbose(string Message, string Verbose)
        {
            if (Verbose.ToLower().Contains("y"))
            {
                Console.WriteLine(Message);
                Debug.WriteLine(Message);
                Logger?.Debug("{Message}", Message);
            }
        }

        /// <summary>
        /// Writes a log entry to the daily log file (legacy method).
        /// </summary>
        public static void WriteLog(int code, string message, string errorInfo, DateTime timeStamp)
        {
            if (Logger != null)
            {
                if (string.IsNullOrEmpty(errorInfo))
                {
                    Logger.Information("{Message}", message);
                }
                else
                {
                    Logger.Error("{Message} - {ErrorInfo}", message, errorInfo);
                }
            }

            var fileName = $"eMailRouter-Log-{timeStamp.Year}-{timeStamp.Month}-{timeStamp.Day}.txt";

            var outputContent = string.Empty;
            if (errorInfo == null)
            {
                outputContent = $"{timeStamp}-\t{message}";
            }
            else
            {
                outputContent = $"{timeStamp}  -\t{message}\t\t\t{errorInfo}";
            }

            try
            {
                File.AppendAllText(Path.Combine(LogDir, fileName), outputContent + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Logger?.Error(ex, "Failed to write to legacy log file");
            }
        }

        /// <summary>
        /// Removes special characters from a string for safe use in file names or database queries.
        /// </summary>
        public static string RemoveSpaceCharacters(string inbound)
        {
            var txt = inbound.Replace("vbLf", "vbCrLF");
            txt = txt.Replace(":", " ");
            txt = txt.Replace("/", " ");
            txt = txt.Replace("\\", " ");
            txt = txt.Replace("&", "and");
            txt = txt.Replace(";", " ");
            txt = txt.Replace("<", " ");
            txt = txt.Replace(">", " ");
            txt = txt.Replace("<<", " ");
            txt = txt.Replace(">>", " ");
            txt = txt.Replace("^", " ");
            txt = txt.Replace("%", " ");
            txt = txt.Replace("@", " ");
            txt = txt.Replace("'", " ");
            txt = txt.Replace(" ", "");
            return txt.Trim();
        }

        #region Subject Line Parsing

        public static string ExtractElement(string str, int n)
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;

            string[] parts = str.Split(',');
            return (n > 0 && n <= parts.Length) ? parts[n - 1].Trim() : string.Empty;
        }

        public static string ExtractValue(string p, string name)
        {
            if (string.IsNullOrEmpty(p) || string.IsNullOrEmpty(name))
                return null;

            string[] parts = p.Split('=');
            if (parts.Length == 2 && parts[0].Trim().ToLower().Contains(name.ToLower()))
            {
                return parts[1].Trim();
            }
            return null;
        }

        #endregion

        #region File Utilities

        public static string GetFileType(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return "txt";

            int lastDot = fileName.LastIndexOf('.');
            return lastDot >= 0 && lastDot < fileName.Length - 1
                        ? fileName.Substring(lastDot + 1)
                        : "txt";
        }

        public static string RemoveJunk(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return string.Empty;

            var result = fileName;
            result = result.Replace(":", " ");
            result = result.Replace("/", " ");
            result = result.Replace("\\", " ");
            result = result.Replace("&", "and");
            result = result.Replace(";", " ");
            return result.Trim();
        }

        #endregion

        #region String Utilities

        public static string GetLastWord(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var words = input.Split(' ');
            return words[words.Length - 1];
        }

        public static string GetNthWord(string input, int n)
        {
            if (string.IsNullOrWhiteSpace(input) || n < 1)
                return string.Empty;

            var words = input.Split(' ');
            return n <= words.Length ? words[n - 1] : string.Empty;
        }

        #endregion

        /// <summary>
        /// Sets local test environment variables for database user and password.
        /// Used by integration tests to ensure environment variables are set.
        /// </summary>
        public static void SetLocalTestEnvironmentVariables(string user, string password)
        {
            Environment.SetEnvironmentVariable("DB_USER", user);
            Environment.SetEnvironmentVariable("DB_PASSWORD", password);
        }
    }
}
