using System;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;
using Kunukn.GooglemapsClustering.Clustering.Contract;
using Kunukn.GooglemapsClustering.Clustering.Utility;

namespace Kunukn.GooglemapsClustering.Clustering.Data
{
    /// <summary>
    /// Kunuk Nykjaer
    /// </summary>

    [Serializable]
    public class P : PBase, IP, ISerializable
    {              
        public P(double x, double y) : base(x, y) { Init(); }
        public P() { Init(); }
        void Init() { I = string.Empty; T = string.Empty; C = 1; }

        public int C { get; set; } // count
        public string I { get; set; } // marker id           
        public string T { get; set; } // marker type

        [ScriptIgnore] //  don't include in JSON data
        public string Name { get; set; } // custom

        public P Normalize()
        {
            Lon = Lon.NormalizeLongitude();
            Lat = Lat.NormalizeLatitude();
            return this;
        }

        public P(SerializationInfo info, StreamingContext ctxt)
        {
            this.C = 1;
            this.I = (string)info.GetValue("Id", typeof(string));
            this.T = (string)info.GetValue("Type", typeof(string));
            this.Lon = ((string)info.GetValue("Lon", typeof(string))).ToDouble();
            this.Lat = ((string)info.GetValue("Lat", typeof(string))).ToDouble();
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("I", this.I);
            info.AddValue("T", this.T);
            info.AddValue("X", this.Lon);
            info.AddValue("Y", this.Lat);
            info.AddValue("C", this.C);
        }

        public override string ToString()
        {
            return string.Format("Uid: {0}, Lon:{1}, Lat:{2}, T:{3}, I:{4}",
                Uid, Lon, Lat, T, I);
        }

        #region Not used

        //public int CompareTo(object o) // if used in sorted list
        //{
        //    var other = o as P;
        //    if (other == null) return -1;

        //    if (this.Equals(o)) return 0;

        //    if (this.Lon > other.Lon) return -1;
        //    if (this.Lon < other.Lon) return 1;
        //    if (this.Lat > other.Lat) return -1;
        //    if (this.Lat < other.Lat) return 1;

        //    return 0;
        //}

        //// todo evaluate if this is needed/correct
        //public override int GetHashCode()
        //{
        //    var x = Lon * 100000; //make the decimals be important
        //    var y = Lat * 100000;
        //    var r = x * 17 + y * 37;
        //    return (int)r;
        //}

        //// todo evaluate if this is needed/correct
        //public override bool Equals(Object o)
        //{
        //    if (o == null)
        //    {
        //        return false;
        //    }

        //    var other = o as P;
        //    if (other == null)
        //    {
        //        return false;
        //    }

        //    var x = Math.Abs(this.Lon - other.Lon) < Numbers.Epsilon;
        //    var y = Math.Abs(this.Lat - other.Lat) < Numbers.Epsilon;
        //    return x && y;
        //}

        //public P(P p)
        //{
        //    this.Lon = p.Lon;
        //    this.Lat = p.Lat;
        //    this.C = p.C;
        //    this.T = p.C == 1 ? p.T : string.Empty;
        //    this.I = p.C == 1 ? p.I : string.Empty;
        //}

        #endregion Not used
    }
}
