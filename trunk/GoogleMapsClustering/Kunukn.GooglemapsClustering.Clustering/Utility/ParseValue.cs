using System;
using System.Globalization;
using Kunukn.GooglemapsClustering.Clustering.Data;

namespace Kunukn.GooglemapsClustering.Clustering.Utility
{
    public static class ParseValue
    {
        static readonly CultureInfo CultureEnUs = new CultureInfo("en-US");
        const string S = "G";

        public static double ToDouble(this string s)
        {
            return double.Parse(s, NumberStyles.Float, NumberFormatInfo.InvariantInfo);
        }

        public static string DoubleToString(this double d)
        {
            double rounded = Math.Round(d, Numbers.Round);
            return rounded.ToString(S, CultureEnUs);
        }

        public static double Round(this double d)
        {
            return Math.Round(d, Numbers.Round);
        }

        //public static double? ToDouble(this string s)
        //{
        //    double d;
        //    var isParsed = double.TryParse(s, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out d);
        //    if (isParsed) return d;
        //    return null;
        //}
    }
}
