using System;
using System.Web.Script.Serialization;
using Kunukn.GooglemapsClustering.Clustering.Utility;

namespace Kunukn.GooglemapsClustering.Clustering.Data
{
    [Serializable]
    public abstract class PBase
    {
        protected PBase(){ Uid = ++_counter;}

        protected PBase(double lon, double lat)
        {
            X = lon;
            Lat = lat;
            Uid = ++_counter;
        }

        [ScriptIgnore] // don't include in JSON data
        private static int _counter;

        [ScriptIgnore] // don't include in JSON data
        public int Uid { get; private set; }

        [ScriptIgnore] // don't include in JSON data
        public object Data { get; set; } // Data container for anything

        [ScriptIgnore] // don't include in JSON data
        public double X { get { return Lon; } set { Lon = value; } }

        [ScriptIgnore] // don't include in JSON data
        public double Y { get { return Lat; } set { Lat = value; } }
       
        private double _lat;
        public double Lat
        {
            get { return ParseValue.Round(_lat); }
            set { _lat = value; }
        }
        private double _lon;
        public double Lon
        {
            get { return ParseValue.Round(_lon); }
            set { _lon = value; }
        }

        public virtual double Distance(PBase p)
        {
            return Distance(p.X, p.Y);
        }

        // Euclidean distance
        public virtual double Distance(double x, double y)
        {
            var dx = X - x;
            var dy = Y - y;
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
    }
}
