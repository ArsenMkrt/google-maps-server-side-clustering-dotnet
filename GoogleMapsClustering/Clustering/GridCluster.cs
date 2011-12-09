﻿using System;
using System.Collections.Generic;
using System.Linq;
using Kunukn.GooglemapsClustering.Data;
using Kunukn.GooglemapsClustering.MathUtility;


namespace Kunukn.GooglemapsClustering.Clustering
{
    /// <summary>
    /// Author: Kunuk Nykjaer
    /// </summary>
    public class GridCluster : ClusterAlgorithmBase
    {
        // absolut pos                 
        protected readonly Boundary _grid = new Boundary();

        // for bucket placement calc, grid cluster algo
        protected readonly double _deltaX;
        protected readonly double _deltaY;

        public static Boundary GetBoundaryExtended(JsonReceive jsonReceive)
        {
            double[] deltas = GetDelta(jsonReceive);
            double deltaX = deltas[0];
            double deltaY = deltas[1];

            // grid with extended outer grid-area non-visible            
            var a = MathTool.FloorLatLon(jsonReceive.Viewport.Minx, deltaX) - deltaX * Config.OuterGridExtend;
            var b = MathTool.FloorLatLon(jsonReceive.Viewport.Miny, deltaY) - deltaY * Config.OuterGridExtend;
            var a2 = MathTool.FloorLatLon(jsonReceive.Viewport.Maxx, deltaX) + deltaX * (1 + Config.OuterGridExtend);
            var b2 = MathTool.FloorLatLon(jsonReceive.Viewport.Maxy, deltaY) + deltaY * (1 + Config.OuterGridExtend);

            // lat is special with Google Maps, they don't wrap around, then do constrain
            b = MathTool.ConstrainLatitude(b);
            b2 = MathTool.ConstrainLatitude(b2);

            var grid = new Boundary { Minx = a, Miny = b, Maxx = a2, Maxy = b2 };
            grid.Normalize();
            return grid;
        }


        public static double[] GetDelta(JsonReceive jsonReceive)
        {
            // Heuristic specific values and grid size dependent, this might needs to be altered for diff window size
            // used in combination with with zoom level
            const int xZoomLevel1 = 480; //560
            const int yZoomLevel1 = 240; //240

            // relative values, used for adjusting grid size
            int gridScaleX = jsonReceive.Gridx;
            int gridScaleY = jsonReceive.Gridy;

            double x = MathTool.Half(xZoomLevel1, jsonReceive.Zoomlevel - 1) / gridScaleX;
            double y = MathTool.Half(yZoomLevel1, jsonReceive.Zoomlevel - 1) / gridScaleY;
            return new double[] { x, y };
        }



        public List<Line> Lines { get; private set; }

        public GridCluster(List<P> dataset, JsonReceive jsonReceive)
            : base(dataset)
        {
            //important, set _delta and _grid values in constructor as first step
            double[] deltas = GetDelta(jsonReceive);
            _deltaX = deltas[0];
            _deltaY = deltas[1];
            _grid = GetBoundaryExtended(jsonReceive);

            if (Config.DoShowGridLinesInGoogleMap)
                MakeLines();

        }


        void MakeLines()
        {
            //obs, Google Maps does not draw every lines if zoomed far out and lines are wide

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
                    continue;

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

        // O(k*n)
        void MergeClustersGrid()
        {
            foreach (var key in BucketsLookup.Keys)
            {
                var bucket = BucketsLookup[key];
                if (bucket.IsUsed == false)
                    continue;

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
        void MergeClustersGridHelper(string currentKey, string[] neighborKeys)
        {
            double minDistX = _deltaX / Config.MergeWithin;
            double minDistY = _deltaY / Config.MergeWithin;
            //if clusters in grid are too close to each other, merge them
            double withinDist = MathTool.Max(minDistX, minDistY);

            foreach (var neighborKey in neighborKeys)
            {
                if (!BucketsLookup.ContainsKey(neighborKey))
                    continue;

                var neighbor = BucketsLookup[neighborKey];
                if (neighbor.IsUsed == false)
                    continue;

                var current = BucketsLookup[currentKey];
                var dist = MathTool.Distance(current.Centroid, neighbor.Centroid);
                if (dist > withinDist)
                    continue;

                current.Points.AddRange(neighbor.Points);//O(n)

                // recalc centroid
                var cp = GetCentroidFromClusterLatLon(current.Points);
                current.Centroid = cp;
                neighbor.IsUsed = false; //merged, then not used anymore
                neighbor.Points.Clear(); //clear mem
            }
        }


        // O(n), could be O(logn-ish) using range search or similar, no problem when points are <500.000
        public static List<P> FilterDataset(List<P> dataset, Boundary viewport)
        {
            //List<P> filtered = dataset.Where(p => MathTool.IsInsideWiden(viewport, p)).ToList();

            var filtered = new List<P>();
            foreach (var p in dataset)
                if (MathTool.IsInside(viewport, p))
                    filtered.Add(p);

            return filtered;
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
            if (LatLonInfo.MaxLonValue % deltax == 0)
            {
                overlapMapMaxX--;
                overlapMapMinX++;
            }
            
            int idxx = (int)(p.Lon / deltax);
            if (p.Lon < 0) idxx--;
            
            if (LatLonInfo.MaxLonValue % p.Lon == 0)
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
                    BucketsLookup[id].Points.Add(p);

                // new bucket, create and add point
                else
                {
                    Bucket bucket = new Bucket(idx, idy, id);
                    bucket.Points.Add(p);
                    BucketsLookup.Add(id, bucket);
                }
            }

            // calc centrod for all buckets
            SetCentroidForAllBuckets(BucketsLookup.Values);

            // merge if gridpoint is to close
            if (Config.DoMergeGridIfCentroidsAreCloseToEachOther)
                MergeClustersGrid();

            if (Config.DoUpdateAllCentroidsToNearestContainingPoint)
                UpdateAllCentroidsToNearestContainingPoint();

            // check again
            // merge if gridpoint is to close
            if (Config.DoMergeGridIfCentroidsAreCloseToEachOther
                && Config.DoUpdateAllCentroidsToNearestContainingPoint)
            {
                MergeClustersGrid();
                // and again set centroid to closest point in bucket 
                UpdateAllCentroidsToNearestContainingPoint();
            }

            return GetClusterResult();
        }
    }

}
