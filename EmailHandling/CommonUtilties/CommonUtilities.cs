using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonUtilties
{
    public class CommonUtilities
    {
        public static string LogDir { get; set; }

        public CommonUtilities()
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

        public static void WriteLog(int code, string message, string errorInfo, DateTime timeStamp)
        {
            var fileName = $"eMailRouter-Log-{timeStamp.Year}-{timeStamp.Month}-{timeStamp.Day}.txt";

            var outputContent = string.Empty;
            if (errorInfo == null)
            {
                outputContent = $"{timeStamp}  -\t{message}";
            }
            else
            {
                outputContent = $"{timeStamp}  -\t{message}\t\t\t{errorInfo}";
            }

            File.AppendAllText(LogDir + "\\" + fileName, outputContent + Environment.NewLine);
        }

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
    }
}
