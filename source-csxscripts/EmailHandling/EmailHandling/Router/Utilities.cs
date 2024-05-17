using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Router
{
    internal class Utilities
    {
        public static string LogDir { get; set; }

        public Utilities()
        {
            LogDir = string.Empty;
        }

        public static void ShowDiagnosticIfVerbose(string Message, string Verbose)
        {
            if (Verbose.ToLower().Contains("y"))
            {
                Console.WriteLine(Message);
                Debug.WriteLine(Message);
            }
        }


        public static string GetConfigVal(string name)
        {
            string delimiter = ",,,,,";
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
            return "FAILED TO FIND VALUE";
        }

        public static void WriteLog(int code, string message, Exception errorInfo, DateTime timeStamp)
        {
            var fileName = $"eMailRouter-Log-{timeStamp.Year}-{timeStamp.Month}-{timeStamp.Day}.txt";

            var outputContent = string.Empty;
            if (errorInfo == null)
            {
                outputContent = $"{timeStamp}  -\t{message}";
            }
            else
            {
                outputContent = $"{timeStamp}  -\t{message}\t\t\t{errorInfo.Message}";
            }

            File.AppendAllText(LogDir + "\\" + fileName, outputContent + Environment.NewLine);
        }
    }
}
