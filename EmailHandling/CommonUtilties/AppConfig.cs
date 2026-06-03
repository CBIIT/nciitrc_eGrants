using Microsoft.Extensions.Configuration;
using System;

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

            var builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
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
                var varValue = Environment.GetEnvironmentVariable(varName) ?? "";
                result = result.Substring(0, start) + varValue + result.Substring(end + 1);
            }
            return result;
        }
    }
}