using System;
using System.Globalization;
using System.Web.Script.Serialization;

namespace Kunukn.GooglemapsClustering.Data //Kunukn.GooglemapsClustering.Data
{
    [Serializable]
    public class PBase
    {        
        public PBase()
        {
        }

        public PBase(double lon, double lat)
        {
            Lon = lon;
            Lat = lat;
        }

        protected static readonly CultureInfo Culture_enUS = new CultureInfo("en-US");
        protected const string S = "G";        

        public static string ToStringEN(double d)
        {
            double rounded = Math.Round(d, Numbers.Round);
            return rounded.ToString(S, Culture_enUS);
        }
        public static double? ToValue(string s)
        {
            double d;
            bool isParsed = double.TryParse(s, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out d);
            if (isParsed)
                return d; //Math.Round(d, Round);
            return null;
        }

        [ScriptIgnore] //  dont include in JSON data
        private double _Lon;
        [ScriptIgnore]
        public double Lon
        {
            get { return _Lon; }
            set { _Lon = value; }//set { _Lon = value == -LatLonInfo.MaxLonValue ? LatLonInfo.MinLonValue : value; }
        }

        [ScriptIgnore] //  dont include in JSON data
        private double _Lat;
        [ScriptIgnore]
        public double Lat
        {
            get { return _Lat; }
            set { _Lat = value; }//set { _Lon = value == -LatLonInfo.MaxLatValue ? LatLonInfo.MinLatValue : value; }
        }
    }
}
