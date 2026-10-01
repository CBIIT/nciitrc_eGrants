using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace CommonUtilties
{
    /// <summary>
    /// Shared application configuration using appsettings.json with environment-specific overrides.
    /// Used by all projects in the EmailHandling solution.
    /// </summary>
    public static class AppConfig
    {
        public static IConfiguration Load()
        {
            var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                ?? "Production";

            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var baseConfigFile = Path.Combine(baseDir, "appsettings.json");
            var envConfigFile = Path.Combine(baseDir, $"appsettings.{environment}.json");

            Console.WriteLine($"Loading configuration:");
            Console.WriteLine($"  Base directory: {baseDir}");
            Console.WriteLine($"  Environment: {environment}");
            Console.WriteLine($"  Base config: {baseConfigFile} (exists: {File.Exists(baseConfigFile)})");
            Console.WriteLine($"  Env config: {envConfigFile} (exists: {File.Exists(envConfigFile)})");

            var builder = new ConfigurationBuilder()
                .SetBasePath(baseDir)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables();

            return builder.Build();
        }

        public static string GetConnectionString(IConfiguration config, string name)
        {
            var conStr = config.GetConnectionString(name) ?? "";
            return ResolveEnvironmentVariables(conStr);
        }

        /// <summary>
        /// Resolves environment variable placeholders in a string.
        /// Placeholders are in the format %VARIABLE_NAME%.
        /// Throws an exception if required database credentials are missing.
        /// </summary>
        /// <param name="input">String with environment variable placeholders</param>
        /// <returns>String with placeholders replaced by actual values</returns>
        /// <exception cref="InvalidOperationException">Thrown when required DB credentials are not found</exception>
        public static string ResolveEnvironmentVariables(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            var result = input;
            int start;
            while ((start = result.IndexOf('%')) >= 0)
            {
                var end = result.IndexOf('%', start + 1);
                if (end < 0) break;

                var varName = result.Substring(start + 1, end - start - 1);
                var varValue = Environment.GetEnvironmentVariable(varName);

                // Check if this is a required database credential that's missing
                if (string.IsNullOrEmpty(varValue) && 
                    (varName == "DB_USER" || varName == "DB_PASSWORD" || varName == "EGRANTS_DB_USER" || varName == "EGRANTS_DB_PASSWORD"))
                {
                    throw new InvalidOperationException(
                        $"Required environment variable '{varName}' is not set. " +
                        $"Please set this environment variable before running the application. " +
                        $"For local development: [System.Environment]::SetEnvironmentVariable('{varName}', 'your_value', [System.EnvironmentVariableTarget]::User) " +
                        $"For servers: [System.Environment]::SetEnvironmentVariable('{varName}', 'your_value', [System.EnvironmentVariableTarget]::Machine)");
                }

                result = result.Substring(0, start) + (varValue ?? "") + result.Substring(end + 1);
            }
            return result;
        }
    }
}