using System;
using System.Globalization;
using System.Web.Script.Serialization;

namespace Kunukn.GooglemapsClustering.Clustering.Data
{
    // todo refactor, make it cleaner and simpler

    [Serializable]
    public class PBase
    {        
        public PBase(){
            Id = ++_counter;
        }

        public PBase(double lon, double lat)
        {
            Lon = lon;
            Lat = lat;
            Id = ++_counter;
        }

        [ScriptIgnore] // don't include in JSON data
        private static int _counter;
        
        [ScriptIgnore] // don't include in JSON data
        public int Id { get; private set; }

        [ScriptIgnore] // don't include in JSON data
        public object Data { get; set; } // Data container for anything

        protected static readonly CultureInfo Culture_enUS = new CultureInfo("en-US");
        protected const string S = "G";


        public virtual double Distance(PBase p)
        {
            return Distance(p.Lon, p.Lat);
        }

        // Euclidean distance
        public virtual double Distance(double x, double y)
        {
            var dx = Lon - x;
            var dy = Lat - y;
            var dist = (dx * dx) + (dy * dy);
            dist = Math.Sqrt(dist);
            //dist = Math.Round(dist, 6);
            return dist;
        }

        public override bool Equals(object obj)
        {
            var o = obj as PBase;
            if (o == null) { return false; }
            return GetHashCode() == o.GetHashCode();
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }


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
            {
                return d; // Math.Round(d, Round);
            }                
            return null;
        }

        [ScriptIgnore] // don't include in JSON data
        private double _lon;
        [ScriptIgnore]
        public double Lon
        {
            get { return _lon; }
            set { _lon = value; }
        }

        [ScriptIgnore] // don't include in JSON data
        private double _lat;
        [ScriptIgnore]
        public double Lat
        {
            get { return _lat; }
            set { _lat = value; }
        }
    }
}
