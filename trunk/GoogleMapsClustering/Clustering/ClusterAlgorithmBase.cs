using System;
using System.Collections.Generic;
using System.Linq;
using Kunukn.GooglemapsClustering.Data;
using Kunukn.GooglemapsClustering.MathUtility;


namespace Kunukn.GooglemapsClustering.Clustering
{
    /// <summary>
    /// /// Author: Kunuk Nykjaer
    /// </summary>
    public abstract class ClusterAlgorithmBase
    {
        protected readonly static Random Rand = new Random();
        protected readonly List<P> Dataset; // all points
        //id, bucket
        public readonly Dictionary<string, Bucket> BucketsLookup =
            new Dictionary<string, Bucket>();

        protected ClusterAlgorithmBase() { }
        protected ClusterAlgorithmBase(List<P> dataset)
        {
            if (dataset == null)
                throw new ApplicationException(
                    string.Format("dataset is null"));

            Dataset = dataset;
        }

        public abstract List<P> GetCluster(ClusterInfo clusterInfo);

        public List<P> GetClusterResult()
        {
            // collect used buckets and return the result
            var clusterPoints = new List<P>();
            //O(m*n)
            foreach (var item in BucketsLookup)
            {
                var bucket = item.Value;
                if (bucket.IsUsed)
                {
                    if (bucket.Points.Count < Config.MinClusterSize)
                    {
                        foreach (var p in bucket.Points)
                            clusterPoints.Add(p);
                    }
                    else
                    {
                        bucket.Centroid.C = bucket.Points.Count;
                        clusterPoints.Add(bucket.Centroid);
                    }

                }
            }

            return clusterPoints;
        }

        /*
        // Basic Centroid Calculation of N Points
        public static P GetCentroidFromCluster(List<P> list) //O(n)
        {          
            int count;
            if (list == null || (count = list.Count) == 0)
                return null;

            if (list.Count == 1)
                return list.First();

            var centroid = new P(0, 0) { C = count };//O(1)
            foreach (var p in list)
            {
                centroid.Lon += p.Lon;
                centroid.Lat += p.Lat;
            }
            centroid.Lon /= count;
            centroid.Lat /= count;                        
            return centroid;
        }
        */

        // Circular mean, very relevant for points around New Zealand, where lon -180 to 180 overlap
        // Adapted Centroid Calculation of N Points for Google Maps usage
        public static P GetCentroidFromClusterLatLon(List<P> list) //O(n)
        {
            int count;
            if (list == null || (count = list.Count) == 0)
                return null;
            if (count == 1)
                return list.First();


            //http://en.wikipedia.org/wiki/Circular_mean
            //http://stackoverflow.com/questions/491738/how-do-you-calculate-the-average-of-a-set-of-angles
            /*
                                  1/N*  sum_i_from_1_to_N sin(a[i])
                a = atan2      ---------------------------
                                  1/N*  sum_i_from_1_to_N cos(a[i])
             */

            double lonSin = 0;
            double lonCos = 0;
            double latSin = 0;
            double latCos = 0;
            foreach (var p in list)
            {
                lonSin += Math.Sin(p.Lon.LatLonToRadian());
                lonCos += Math.Cos(p.Lon.LatLonToRadian());
                latSin += Math.Sin(p.Lat.LatLonToRadian());
                latCos += Math.Cos(p.Lat.LatLonToRadian());
            }

            lonSin /= count;
            lonCos /= count;

            double radx = 0;
            double rady = 0;

            if (lonSin != 0 && lonCos != 0)
            {
                radx = Math.Atan2(lonSin, lonCos);
                rady = Math.Atan2(latSin, latCos);
            }
            var x = radx.RadianToLatLon();
            var y = rady.RadianToLatLon();

            var centroid = new P(x, y) { C = count };
            return centroid;
        }


        //O(k*n)
        public static void SetCentroidForAllBuckets(IEnumerable<Bucket> buckets)
        {
            foreach (var item in buckets)
                item.Centroid = GetCentroidFromClusterLatLon(item.Points);

        }

        public P GetClosestPoint(P from, List<P> list) //O(n)
        {
            double min = double.MaxValue;
            P closests = null;
            foreach (var p in list)
            {
                var d = MathTool.Distance(from, p);
                if (d >= min)
                    continue;

                // update
                min = d;
                closests = p;
            }
            return closests;
        }

        // assign all points to nearest cluster
        public void UpdatePointsByCentroid()//O(n*k)
        {
            // clear points in the buckets, they will be re-inserted
            foreach (var bucket in BucketsLookup.Values)
                bucket.Points.Clear();

            foreach (P p in Dataset)
            {
                double minDist = Double.MaxValue;
                string index = string.Empty;
                foreach (var i in BucketsLookup.Keys)
                {
                    var bucket = BucketsLookup[i];
                    if (bucket.IsUsed == false)
                        continue;

                    var centroid = bucket.Centroid;
                    var dist = MathTool.Distance(p, centroid);
                    if (dist < minDist)
                    {
                        // update
                        minDist = dist;
                        index = i;
                    }
                }
                // re-insert
                var closestBucket = BucketsLookup[index];
                closestBucket.Points.Add(p);
            }
        }

        // update centroid location to nearest point, 
        // e.g. if you want to show cluster point on a real existing point area
        //O(n)
        public void UpdateCentroidToNearestContainingPoint(Bucket bucket)
        {
            if (bucket == null || bucket.Centroid == null ||
                bucket.Points == null || bucket.Points.Count == 0)
                return;

            var closest = GetClosestPoint(bucket.Centroid, bucket.Points);
            bucket.Centroid.Lon = closest.Lon; // no normalize, points are already normalized by default
            bucket.Centroid.Lat = closest.Lat;
        }
        //O(k*n)
        public void UpdateAllCentroidsToNearestContainingPoint()
        {
            foreach (var bucket in BucketsLookup.Values)
                UpdateCentroidToNearestContainingPoint(bucket);
        }
    }
}
