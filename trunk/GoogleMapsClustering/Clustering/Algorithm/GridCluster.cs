using System;
using System.Collections.Generic;
using Kunukn.GooglemapsClustering.Clustering.Data;
using Kunukn.GooglemapsClustering.Clustering.Data.Json;
using Kunukn.GooglemapsClustering.Clustering.Utility;

namespace Kunukn.GooglemapsClustering.Clustering.Algorithm
{
    /// <summary>
    /// Author: Kunuk Nykjaer
    /// </summary>
    public class GridCluster : ClusterAlgorithmBase
    {
        // Absolut position
        protected readonly Boundary _grid = new Boundary();

        // Bucket placement calc, grid cluster algo
        protected readonly double _deltaX;
        protected readonly double _deltaY;

        public static Boundary GetBoundaryExtended(JsonGetMarkersReceive jsonReceive)
        {
            double[] deltas = GetDelta(jsonReceive);
            double deltaX = deltas[0];
            double deltaY = deltas[1];

            // Grid with extended outer grid-area non-visible            
            var a = MathTool.FloorLatLon(jsonReceive.Viewport.Minx, deltaX) - deltaX * AlgoConfig.OuterGridExtend;
            var b = MathTool.FloorLatLon(jsonReceive.Viewport.Miny, deltaY) - deltaY * AlgoConfig.OuterGridExtend;
            var a2 = MathTool.FloorLatLon(jsonReceive.Viewport.Maxx, deltaX) + deltaX * (1 + AlgoConfig.OuterGridExtend);
            var b2 = MathTool.FloorLatLon(jsonReceive.Viewport.Maxy, deltaY) + deltaY * (1 + AlgoConfig.OuterGridExtend);

            // Latitude is special with Google Maps, they don't wrap around, then do constrain
            b = MathTool.ConstrainLatitude(b);
            b2 = MathTool.ConstrainLatitude(b2);

            var grid = new Boundary { Minx = a, Miny = b, Maxx = a2, Maxy = b2 };
            grid.Normalize();
            return grid;
        }


        public static double[] GetDelta(JsonGetMarkersReceive jsonReceive)
        {
            // Heuristic specific values and grid size dependent.
            // used in combination with zoom level.

            // xZoomLevel1 and yZoomLevel1 is used to define the size of one grid-cell

            // Absolute base value of longitude distance
            const int xZoomLevel1 = 480;
            // Absolute base value of latitude distance
            const int yZoomLevel1 = 240;
                       
            // Relative values, used for adjusting grid size
            var gridScaleX = jsonReceive.Gridx;
            var gridScaleY = jsonReceive.Gridy;

            double x = MathTool.Half(xZoomLevel1, jsonReceive.Zoomlevel - 1) / gridScaleX;
            double y = MathTool.Half(yZoomLevel1, jsonReceive.Zoomlevel - 1) / gridScaleY;
            return new [] { x, y };
        }

        public List<Line> Lines { get; private set; }

        public GridCluster(List<P> dataset, JsonGetMarkersReceive jsonReceive)
            : base(dataset)
        {
            // Important, set _delta and _grid values in constructor as first step
            double[] deltas = GetDelta(jsonReceive);
            _deltaX = deltas[0];
            _deltaY = deltas[1];
            _grid = GetBoundaryExtended(jsonReceive);

            if (AlgoConfig.DoShowGridLinesInGoogleMap)
            {
                MakeLines();
            }
                
        }

        void MakeLines()
        {
            // Note, Google Maps does not seem to draw every lines if zoomed far out using this approach
            // a fix could be to split up the lines based on the coordinate values

            Lines = new List<Line>();
            var p2Lines = new List<Point2>();

            const int borderLinesAdding = 1;
            int linesStepsX = (int)(Math.Round(_grid.AbsX / _deltaX) + borderLinesAdding);
            int linesStepsY = (int)(Math.Round(_grid.AbsY / _deltaY) + borderLinesAdding);

            var b = new Boundary(_grid);
            b.Miny = MathTool.ConstrainLatitude(b.Miny, 5.5); // Google Maps didn't draw lines near lat -90 or 90 last time I checked
            b.Maxy = MathTool.ConstrainLatitude(b.Maxy, 5.5);

            // make the red lines data to be drawn in Google map
            // vertical lines
            for (int i = 0; i < linesStepsX; i++)
            {
                var xx = b.Minx + i * _deltaX;

                double x = xx;
                double x2 = xx;
                double y = b.Miny;
                double y2 = b.Maxy;
                p2Lines.Add(new Point2 { Minx = x, Miny = y, Maxx = x2, Maxy = y2 });
            }

            // horizontal lines            
            for (int i = 0; i < linesStepsY; i++)
            {
                var yy = b.Miny + i * _deltaY;
                if (MathTool.IsLowerThanLatMin(yy) || MathTool.IsGreaterThanLatMax(yy))
                {
                    continue;
                }                    

                double y = yy;
                double y2 = yy;
                double x = b.Minx;
                double x2 = b.Maxx;
                p2Lines.Add(new Point2 { Minx = x, Miny = y, Maxx = x2, Maxy = y2 });
            }

            foreach (var p2 in p2Lines)
            {
                string x = PBase.ToStringEN((p2.Minx).NormalizeLongitude());
                string x2 = PBase.ToStringEN((p2.Maxx).NormalizeLongitude());
                string y = PBase.ToStringEN((p2.Miny).NormalizeLatitude());
                string y2 = PBase.ToStringEN((p2.Maxy).NormalizeLatitude());
                Lines.Add(new Line { X = x, Y = y, X2 = x2, Y2 = y2 });
            }
        }

        public override List<P> GetCluster(ClusterInfo clusterInfo)
        {
            return RunClusterAlgo(clusterInfo);
        }


        // dictionary lookup key used by grid cluster algo
        public static string GetId(int idx, int idy) //O(1)
        {
            return idx + ";" + idy;
        }

        // Average running time (m*n)
        // worst case might actually be ~ O(n^2) if most of centroids are merged, due to centroid re-calculation, very very unlikely
        void MergeClustersGrid()
        {
            foreach (var key in BucketsLookup.Keys)
            {
                var bucket = BucketsLookup[key];
                if (bucket.IsUsed == false)
                {
                    continue;
                }                    

                var x = bucket.Idx;
                var y = bucket.Idy;

                // get keys for neighbors
                var N = GetId(x, y + 1);
                var NE = GetId(x + 1, y + 1);
                var E = GetId(x + 1, y);
                var SE = GetId(x + 1, y - 1);
                var S = GetId(x, y - 1);
                var SW = GetId(x - 1, y - 1);
                var W = GetId(x - 1, y);
                var NW = GetId(x - 1, y - 1);
                var neighbors = new[] { N, NE, E, SE, S, SW, W, NW };

                MergeClustersGridHelper(key, neighbors);
            }
        }
        void MergeClustersGridHelper(string currentKey, IEnumerable<string> neighborKeys)
        {
            double minDistX = _deltaX / AlgoConfig.MergeWithin;
            double minDistY = _deltaY / AlgoConfig.MergeWithin;
            // If clusters in grid are too close to each other, merge them
            double withinDist = MathTool.Max(minDistX, minDistY);

            foreach (var neighborKey in neighborKeys)
            {
                if (!BucketsLookup.ContainsKey(neighborKey))
                {
                    continue;
                }                    

                var neighbor = BucketsLookup[neighborKey];
                if (neighbor.IsUsed == false)
                {
                    continue;
                }
                    
                var current = BucketsLookup[currentKey];
                var dist = MathTool.Distance(current.Centroid, neighbor.Centroid);
                if (dist > withinDist)
                {
                    continue;
                }
                    
                current.Points.AddRange(neighbor.Points);//O(n)

                // recalc centroid
                var cp = GetCentroidFromClusterLatLon(current.Points);
                current.Centroid = cp;
                neighbor.IsUsed = false; // merged, then not used anymore
                neighbor.Points.Clear(); // clear mem
            }
        }        

        // To work properly it requires the p is already normalized
        public static int[] GetPointMappedIds(P p, Boundary grid, double deltax, double deltay)
        {
            var relativeX = p.Lon - grid.Minx;
            var relativeY = p.Lat - grid.Miny;
            int idx, idy;

            // Naive version, lon points near 180 and lat points near 90 are not clustered together
            //idx = (int)(relativeX / deltax);
            //idy = (int)(relativeY / deltay);
            // end Naive version

            /*
            You have to draw a line with longitude values 180, -180 on papir to understand this            
                
             e.g. _deltaX = 20
longitude        150   170  180  -170   -150
                 |      |          |     |
                 
       
   idx =         7      8    9    -9    -8
                            -10    
                                  
here we want idx 8, 9, -10 and -9 be equal to each other, we set them to idx=8
then the longitudes from 170 to -170 will be clustered together
             */
            
            int overlapMapMinX = (int)(LatLonInfo.MinLonValue / deltax) - 1;
            int overlapMapMaxX = (int)(LatLonInfo.MaxLonValue / deltax);

            // the deltaX = 20 example scenario, then set the value 9 to 8 and -10 to -9            

            // similar to if (LatLonInfo.MaxLonValue % deltax == 0) without floating presicion issue
            if (Math.Abs(LatLonInfo.MaxLonValue % deltax - 0) < Numbers.Epsilon)            
            {
                overlapMapMaxX--;
                overlapMapMinX++;
            }
            
            int idxx = (int)(p.Lon / deltax);
            if (p.Lon < 0) idxx--;
            
            if (Math.Abs(LatLonInfo.MaxLonValue % p.Lon - 0) < Numbers.Epsilon)
            {
                if (p.Lon < 0) idxx++;
                else idxx--;
            }
                                    
            if (idxx == overlapMapMinX) idxx = overlapMapMaxX;

            idx = idxx;

            // Latitude never wraps around with Google Maps, ignore 90, -90 wrap-around for latitude
            idy = (int)(relativeY / deltay);

            return new []{idx, idy};
        }


        public List<P> RunClusterAlgo(ClusterInfo clusterInfo)
        {
            // skip points outside the grid
            List<P> filtered = clusterInfo.IsFilterData ? FilterDataset(Dataset, _grid) : Dataset;

            // put points in buckets
            foreach (var p in filtered)
            {
                int[] idxy = GetPointMappedIds(p, _grid, _deltaX, _deltaY);
                int idx = idxy[0];
                int idy = idxy[1];

                // bucket id
                string id = GetId(idx, idy);

                // bucket exists, add point
                if (BucketsLookup.ContainsKey(id))
                {
                    BucketsLookup[id].Points.Add(p);   
                }
                    
                // new bucket, create and add point
                else
                {
                    var bucket = new Bucket(idx, idy, id);
                    bucket.Points.Add(p);
                    BucketsLookup.Add(id, bucket);
                }
            }

            // calculate centroid for all buckets
            SetCentroidForAllBuckets(BucketsLookup.Values);

            // merge if gridpoint is to close
            if (AlgoConfig.DoMergeGridIfCentroidsAreCloseToEachOther)
            {
                MergeClustersGrid();
            }

            if (AlgoConfig.DoUpdateAllCentroidsToNearestContainingPoint)
            {
                UpdateAllCentroidsToNearestContainingPoint();
            }
                
            // Check again
            // Merge if gridpoint is to close
            if (AlgoConfig.DoMergeGridIfCentroidsAreCloseToEachOther
                && AlgoConfig.DoUpdateAllCentroidsToNearestContainingPoint)
            {
                MergeClustersGrid();
                // and again set centroid to closest point in bucket 
                UpdateAllCentroidsToNearestContainingPoint();
            }

            return GetClusterResult(_grid);
        }
    }
}