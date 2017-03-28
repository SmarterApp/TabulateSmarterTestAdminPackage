using System;
using System.Globalization;

namespace TabulateSmarterTestPackage.Utilities
{
    public static class FormatHelper
    {
        public static string Strip200(string val)
        {
            if (string.IsNullOrEmpty(val))
            {
                return string.Empty;
            }
            return val.StartsWith("200-", StringComparison.Ordinal) ? val.Substring(4) : val;
        }

        public static string FormatDouble(string val)
        {
            if (string.IsNullOrEmpty(val))
            {
                return string.Empty;
            }
            double v;
            return double.TryParse(val, out v) ? v.ToString("G", CultureInfo.InvariantCulture) : val;
        }
    }
}