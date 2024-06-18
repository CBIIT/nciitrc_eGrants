using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailTests
{
    internal class TestUtilities
    {
        public static string LogDir { get; set; }

        public TestUtilities()
        {
            LogDir = string.Empty;
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
    }
}
