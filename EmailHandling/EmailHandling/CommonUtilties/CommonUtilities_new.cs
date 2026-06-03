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
    /// PURPOSE:
    /// Provides common utility functions used across all email processing applications
    /// in the EmailHandling solution. This includes configuration reading, structured logging
    /// with Serilog, string manipulation, and file utilities.
    /// 
    /// LOGGING:
    /// Uses Serilog for structured logging with the following features:
    /// - Daily rolling log files with 31-day retention
    /// - Console output with color-coded log levels
 /// - File size limit of 10MB with automatic rollover
    /// - Structured logging with named parameters
    /// 
    /// Log files are created in the configured log directory with names like:
    ///   {ApplicationName}-{yyyy-MM-dd}.log
    /// 
    /// Log Levels:
    /// - Verbose: Detailed tracing (subfolder navigation, etc.)
    /// - Debug: Diagnostic information (config values, item details)
    /// - Information: Normal operations (start/stop, items processed)
    /// - Warning: Non-critical issues (legacy log failures)
    /// - Error: Exceptions and failures
    /// 
    /// CONFIGURATION:
  /// All applications use a config.csv file with the format:
    ///   key,,,,,value
    /// The delimiter is five commas (,,,,,) to allow values containing single commas.
    /// 
/// COMMON CONFIG KEYS:
    /// - logDir: Directory path for log files
    /// - conStr: SQL Server connection string
    /// - Verbose: "y" or "n" for diagnostic output
    /// - dBug: "y" or "n" for debug mode (prevents actual email sending)
  /// - Various dirpath* keys for Outlook folder paths
    /// 
    /// USAGE:
    /// Initialize logging in your application's Main method:
    /// <code>
    ///   CommonUtilities.InitializeLogging("MyApp", logDir);
    ///   CommonUtilities.Logger.Information("Processing started");
    ///   CommonUtilities.Logger.Error(ex, "An error occurred processing {ItemId}", itemId);
    ///   // ... application code ...
    ///   CommonUtilities.CloseLogging();
    /// </code>
    /// </summary>
    public class CommonUtilities
    {
        /// <summary>
        /// Gets or sets the log directory path.
  /// Automatically set when <see cref="InitializeLogging"/> is called.
   /// </summary>
        public static string LogDir { get; set; }

    /// <summary>
        /// Gets the Serilog logger instance for structured logging.
        /// Must call <see cref="InitializeLogging"/> before using.
        /// 
        /// Usage examples:
      /// <code>
        /// Logger.Information("Processing {Count} items", itemCount);
        /// Logger.Debug("Item details: {Subject}", item.Subject);
        /// Logger.Error(ex, "Failed to process {ItemId}", itemId);
   /// </code>
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
      /// Creates daily rolling log files in the specified directory.
  /// 
/// Features:
        /// - Daily rolling log files (e.g., AddSuppEmailer-2024-01-15.log)
        /// - Retained for 31 days
     /// - 10MB file size limit with automatic rollover
        /// - Console output with timestamps and colored log levels
        /// - Application name enrichment for filtering
        /// </summary>
        /// <param name="applicationName">Name of the application (used in log file name)</param>
        /// <param name="logDirectory">Directory where log files will be stored</param>
    /// <param name="minimumLevel">Minimum log level (default: Information)</param>
  /// <example>
        /// <code>
        /// // In Program.Main():
        /// var logDir = CommonUtilities.GetConfigVal("logDir");
        /// CommonUtilities.InitializeLogging("AddSuppEmailer", logDir);
        /// 
        /// // Now use the logger:
        /// CommonUtilities.Logger.Information("Application started");
     /// </code>
        /// </example>
        public static void InitializeLogging(string applicationName, string logDirectory, LogEventLevel minimumLevel = LogEventLevel.Information)
     {
            LogDir = logDirectory;

   // Ensure log directory exists
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
        /// Call this in a finally block when your application is shutting down
        /// to ensure all log entries are written.
        /// </summary>
        /// <example>
   /// <code>
     /// try
        /// {
     ///     // Application code
  /// }
  /// finally
        /// {
      ///     CommonUtilities.CloseLogging();
 /// }
        /// </code>
 /// </example>
      public static void CloseLogging()
        {
 Logger?.Information("Logging shutting down");
            (Logger as IDisposable)?.Dispose();
            Log.CloseAndFlush();
        }

        /// <summary>
      /// Outputs diagnostic message to console and debug output if verbose mode is enabled.
        /// Also logs to Serilog at Debug level if logger is initialized.
        /// </summary>
        /// <param name="Message">The diagnostic message to display</param>
  /// <param name="Verbose">Verbose flag - outputs if contains "y" (case-insensitive)</param>
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
        /// Reads a configuration value from config.csv file.
  /// </summary>
        public static string GetConfigVal(string name)
        {
      string delimiter = ",,,,,";
            try
            {
          foreach (string line in File.ReadLines(@"config.csv"))
      {
                    string[] delimiterAsArray = new string[] { delimiter };
        var sections = line.Split(delimiterAsArray, StringSplitOptions.None);

    if (sections.Length > 1)
             {
   var key = sections[0];
     var value = sections[1];
           if (key.Equals(name))
     {
   return value;
  }
  }
         }
            }
     catch (Exception ex)
   {
   Logger?.Error(ex, "Failed to read config value for key: {Key}", name);
       }
         return "FAILED TO FIND VALUE";
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
                outputContent = $"{timeStamp}-\t{message}\t\t\t{errorInfo}";
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
    }
}
