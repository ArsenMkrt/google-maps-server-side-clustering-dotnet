﻿using System;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;
using Kunukn.GooglemapsClustering.Clustering.Contract;
using Kunukn.GooglemapsClustering.Clustering.Utility;

namespace Kunukn.GooglemapsClustering.Clustering.Data
{
    /// <summary>
    /// Kunuk Nykjaer
    /// </summary>

    // todo refactor, make it cleaner and simpler

    [Serializable]
    public class P : PBase, IP, ISerializable, IComparable
    {
        public string Y // lat json
        {
            get { return ToStringEN(Lat); }
            set { Lat = ToValue(value).Value; } // Do throw exception if null
        }
        public string X // lon json
        {
            get { return ToStringEN(Lon); }
            set { Lon = ToValue(value).Value; }
        }
        
        public P(double x, double y) : base(x, y) { I = string.Empty; T = string.Empty; C = 1; }
        public P() { I = string.Empty; T = string.Empty; C = 1; }
        public P(P p) 
        {
            this.Lon = p.Lon;
            this.Lat = p.Lat;
            this.C = p.C;
            this.T = p.C == 1 ? p.T : string.Empty;
            this.I = p.C == 1 ? p.I : string.Empty;
        }

        public int C { get; set; } // count
        public string I { get; set; } // marker id           
        public string T { get; set; } // marker type

        [ScriptIgnore] //  don't include in JSON data
        public string Name { get; set; } // custom

            
        public P(SerializationInfo info, StreamingContext ctxt)
        {
            //this.Lat = (double)info.GetValue("Lat", typeof(double));
            //this.Lon = (double)info.GetValue("Lon", typeof(double));
            this.C = 1;
            this.I = (string)info.GetValue("I", typeof(string));
            this.T = (string)info.GetValue("T", typeof(string));
            this.X = (string)info.GetValue("X", typeof(string));
            this.Y = (string)info.GetValue("Y", typeof(string));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            //info.AddValue("Lat", this.Lat);
            //info.AddValue("Lon", this.Lon);
            info.AddValue("I", this.I);            
            info.AddValue("T", this.T);
            info.AddValue("X", this.X);
            info.AddValue("Y", this.Y);
            info.AddValue("C", this.C);
        }

        public int CompareTo(object o) // if used in sorted list
        {
            if (o == null)
            {
                return -1;
            }                
            var other = o as P;
            if (other == null)
            {
                return -1;
            }
                
            if (this.Equals(o))
            {
                return 0;
            }
                
            if (this.Lon > other.Lon)
            {
                return -1;
            }
                
            if (this.Lon < other.Lon)
            {
                return 1;
            }                

            if (this.Lat > other.Lat)
            {
                return -1;
            }
                
            if (this.Lat < other.Lat)
            {
                return 1;
            }
                
            return 0;
        }

        public P Normalize()
        {
            Lon = Lon.NormalizeLongitude();
            Lat = Lat.NormalizeLatitude();
            return this;
        }


        // todo evaluate if this is needed/correct
        public override int GetHashCode()
        {
            var x = Lon * 100000; //make the decimals be important
            var y = Lat * 100000;
            var r = x * 17 + y * 37;
            return (int)r;
        }


        // todo evaluate if this is needed/correct
        public override bool Equals(Object o)
        {
            if (o == null)
            {
                return false;
            }
                
            var other = o as P;
            if (other == null)
            {
                return false;
            }

            var x = Math.Abs(this.Lon - other.Lon) < Numbers.Epsilon;
            var y = Math.Abs(this.Lat - other.Lat) < Numbers.Epsilon;
            return x && y;
        }

        public override string ToString()
        {
            return string.Format("X:{0} Y:{1} ",X,Y);            
        }
    }
}
