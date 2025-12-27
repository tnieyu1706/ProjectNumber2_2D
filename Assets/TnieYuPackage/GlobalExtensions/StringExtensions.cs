using System;

namespace TnieYuPackage.GlobalExtensions
{
    public static class StringExtensions
    {
        public static string Space(this string str) => str + " ";
        public static string NewLine(this string str) => str + "\n";
        public static string Add(this string str, string str2) => str + str2;

        public static int ParseInt(this string str, int defaultValue)
        {
            try
            {
                return int.Parse(str);
            }
            catch (FormatException)
            {
                return defaultValue;
            }
        }
    }
}
