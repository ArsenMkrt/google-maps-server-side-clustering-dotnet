using System.Collections.Generic;

namespace Kunukn.GooglemapsClustering.Clustering.Data
{
    public class Bucket
    {
        public string Id { get; private set; }
        public List<P> Points { get; private set; }
        public P Centroid { get; set; }
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
