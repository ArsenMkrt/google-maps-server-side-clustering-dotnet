﻿using System.Collections.Generic;


namespace Kunukn.GooglemapsClustering.Data
{
    public class Bucket
    {
        public string Id { get; private set; }
        public List<P> Points { get; private set; }

        private P _Centroid;
        public P Centroid
        {
            get { return _Centroid; }
            set
            {
                _Centroid = value;
                //if (_Centroid == null) return;
                //_Centroid.Lon = _Centroid.Lon.NormalizeLongitude();
                //_Centroid.Lat = _Centroid.Lat.NormalizeLatitude();
            }
        }

        public int Idx { get; private set; }
        public int Idy { get; private set; }
        public double ErrorLevel { get; set; } // clusterpoint and points avg dist
        private bool _IsUsed;
        public bool IsUsed
        {
            get { return _IsUsed && Centroid != null; }
            set { _IsUsed = value; }
        }
        public Bucket(string id)
        {
            IsUsed = true;
            Centroid = null;
            Points = new List<P>();
            Id = id;
        }
        public Bucket(int idx, int idy, string id)
        {
            IsUsed = true;
            Centroid = null;
            Points = new List<P>();
            Idx = idx;
            Idy = idy;
            Id = id;
        }
    }
}