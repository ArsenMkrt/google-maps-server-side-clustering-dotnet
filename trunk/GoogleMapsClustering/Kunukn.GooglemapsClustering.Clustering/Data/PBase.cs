using System;
using System.Web.Script.Serialization;

namespace Kunukn.GooglemapsClustering.Clustering.Data
{ 
    public abstract class PBase
    {
        protected PBase()
        {
            Uid = ++_counter;
        }

        protected PBase(double lon, double lat)
        {
            Lon = lon;
            Lat = lat;
            Uid = ++_counter;
        }

        [ScriptIgnore] // don't include in JSON data
        private static int _counter;
        
        [ScriptIgnore] // don't include in JSON data
        public int Uid { get; private set; }

        [ScriptIgnore] // don't include in JSON data
        public object Data { get; set; } // Data container for anything
 
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
            return Uid.GetHashCode();
        }

        public static string ToString(double d)
        {
            var rounded = Math.Round(d, Numbers.Round);
            return Utility.ParseValue.DoubleToString(rounded);
        }
        public static double? ToValue(string s)
        {
            return Utility.ParseValue.ToDouble(s);            
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
