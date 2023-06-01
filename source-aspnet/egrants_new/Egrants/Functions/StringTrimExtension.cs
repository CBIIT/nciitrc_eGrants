using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace egrants_new.Egrants.Functions
{
    public static class StringTrimExtension
    {
        public static string Truncate(this string value, int maxLength, string truncationSuffix = "…")
        {
            if (!string.IsNullOrEmpty(value))
            {
                return value?.Length > maxLength ? value.Substring(0, maxLength) + truncationSuffix : value;
            }

            return value;
        }

    }
}