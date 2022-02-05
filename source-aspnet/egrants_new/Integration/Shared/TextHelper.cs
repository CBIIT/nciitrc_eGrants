using System;
using System.Text.RegularExpressions;


namespace egrants_new.Integration.Shared
{

    public static class TextHelper
    {

        public static string BustHtml(string input)
        {
            //string rawHtml = input;
            //Clean up the easy to remove part
            string replacechar = "##b";
            input = input.Replace("\r\n", replacechar);
            string regpat = @"<head>.+</head>";
            string prunedHtml = Regex.Replace(input, regpat, String.Empty);
            string regex = "(" + replacechar + replacechar + replacechar + ")+";
            //  prunedHtml = Regex.Replace(prunedHtml, regex, "\r\n");

            char[] array = new char[prunedHtml.Length]; int arrayIndex = 0;
            bool inside = false;
            for (int i = 0; i < prunedHtml.Length; i++)
            {
                char let = input[i];
                if (@let == '<')
                {
                    inside = true;
                    continue;
                }

                if (@let == '>')
                {
                    inside = false;
                    continue;
                }

                if (!inside)
                {
                    array[arrayIndex] = @let; arrayIndex++;
                }

            }
            string clean = new string(array, 0, arrayIndex);

            clean = Regex.Replace(clean, regex, "\r\n");
            clean = Regex.Replace(clean, replacechar, "\r\n");
            return clean;
        }
    }

}