﻿using System;
using System.Collections.Generic;
using Kunukn.GooglemapsClustering.Clustering.Contract;
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
        protected readonly Boundary Grid = new Boundary();

        // Bucket placement calc, grid cluster algo
        protected readonly double DeltaX;
        protected readonly double DeltaY;

        public static Boundary GetBoundaryExtended(JsonGetMarkersReceive jsonReceive)
        {
            var deltas = GetDelta(jsonReceive);
            var deltaX = deltas[0];
            var deltaY = deltas[1];

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
            var gridScaleX = AlgoConfig.Gridx;
            var gridScaleY = AlgoConfig.Gridy;

            var x = MathTool.Half(xZoomLevel1, jsonReceive.Zoomlevel - 1) / gridScaleX;
            var y = MathTool.Half(yZoomLevel1, jsonReceive.Zoomlevel - 1) / gridScaleY;
            return new double[] { x, y };
        }

        public List<Line> Lines { get; private set; }

        public GridCluster(IPoints dataset, JsonGetMarkersReceive jsonReceive)
            : base(dataset)
        {
            // Important, set _delta and _grid values in constructor as first step
            var deltas = GetDelta(jsonReceive);
            DeltaX = deltas[0];
            DeltaY = deltas[1];
            Grid = GetBoundaryExtended(jsonReceive);
            Lines = new List<Line>();

            if (AlgoConfig.DoShowGridLinesInGoogleMap) MakeLines(jsonReceive);
        }

        void MakeLines(JsonGetMarkersReceive jsonReceive)
        {
            // Make the red lines data to be drawn in Google map
            
            var temp = new List<Rectangle>();

            const int borderLinesAdding = 1;
            var linesStepsX = (int)(Math.Round(Grid.AbsX / DeltaX) + borderLinesAdding);
            var linesStepsY = (int)(Math.Round(Grid.AbsY / DeltaY) + borderLinesAdding);

            var b = new Boundary(Grid);
            const double restrictLat = 5.5;
            b.Miny = MathTool.ConstrainLatitude(b.Miny, restrictLat); // Make sure it is visible on screen, restrict by some value
            b.Maxy = MathTool.ConstrainLatitude(b.Maxy, restrictLat);

            // Vertical lines
            for (var i = 0; i < linesStepsX; i++)
            {
                var xx  = b.Minx + i * DeltaX;
                
                // Draw region
                if (jsonReceive.Zoomlevel > 3)
                {
                    temp.Add(new Rectangle { Minx = xx, Miny = b.Miny, Maxx = xx, Maxy = b.Maxy });
                }
                // World wrap issue when same latlon area visible multiple times
                // Make sure line is drawn from left to right on screen
                else
                {
                    temp.Add(new Rectangle { Minx = xx, Miny = LatLonInfo.MinLatValue + restrictLat, Maxx = xx, Maxy = 0 });
                    temp.Add(new Rectangle { Minx = xx, Miny = 0, Maxx = xx, Maxy = LatLonInfo.MaxLatValue-restrictLat });
                }

            }

            // Horizontal lines            
            for (var i = 0; i < linesStepsY; i++)
            {
                var yy = b.Miny + i * DeltaY;
                                
                // Draw region
                if (jsonReceive.Zoomlevel > 3)
                {
                    // Don't draw lines outsize the world
                    if (MathTool.IsLowerThanLatMin(yy) || MathTool.IsGreaterThanLatMax(yy)) continue;

                    temp.Add(new Rectangle { Minx = b.Minx, Miny = yy, Maxx = b.Maxx, Maxy = yy });
                }                
                // World wrap issue when same latlon area visible multiple times
                // Make sure line is drawn from left to right on screen
                else
                {
                    temp.Add(new Rectangle { Minx = LatLonInfo.MinLonValue, Miny = yy, Maxx = 0, Maxy = yy });
                    temp.Add(new Rectangle { Minx = 0, Miny = yy, Maxx = LatLonInfo.MaxLonValue, Maxy = yy });
                }
            }

            // Normalize the lines and add as string
            foreach (var line in temp)
            {
                var x = (line.Minx).NormalizeLongitude().DoubleToString();
                var x2 = (line.Maxx).NormalizeLongitude().DoubleToString();
                var y = (line.Miny).NormalizeLatitude().DoubleToString();
                var y2 = (line.Maxy).NormalizeLatitude().DoubleToString();                
                Lines.Add(new Line { X = x, Y = y, X2 = x2, Y2 = y2 });
            }
        }
       

        public override IPoints GetCluster(ClusterInfo clusterInfo)
        {
            return RunClusterAlgo(clusterInfo);
        }


        // Dictionary lookup key used by grid cluster algo
        public static string GetId(int idx, int idy) //O(1)
        {
            return idx + ";" + idy;
        }

        // Average running time (m*n)
        // worst case might actually be 
        // ~ O(n^2) if most of centroids are merged, due to centroid re-calculation, very very unlikely
        void MergeClustersGrid()
        {
            foreach (var key in BucketsLookup.Keys)
            {
                var bucket = BucketsLookup[key];
                if (!bucket.IsUsed) continue; // skip not used

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
            double minDistX = DeltaX / AlgoConfig.MergeWithin;
            double minDistY = DeltaY / AlgoConfig.MergeWithin;
            // If clusters in grid are too close to each other, merge them
            double withinDist = Math.Max(minDistX, minDistY);

            foreach (var neighborKey in neighborKeys)
            {
                if (!BucketsLookup.ContainsKey(neighborKey)) continue;

                var neighbor = BucketsLookup[neighborKey];
                if (neighbor.IsUsed == false) continue;

                var current = BucketsLookup[currentKey];
                var dist = MathTool.Distance(current.Centroid, neighbor.Centroid);
                if (dist > withinDist) continue;

                current.Points.Data.AddRange(neighbor.Points.Data);//O(n)

                // recalc centroid
                var cp = GetCentroidFromClusterLatLon(current.Points);
                current.Centroid = cp;
                neighbor.IsUsed = false; // merged, then not used anymore
                neighbor.Points.Data.Clear(); // clear mem
            }
        }

        // To work properly it requires the p is already normalized
        public static int[] GetPointMappedIds(IP p, Boundary grid, double deltax, double deltay)
        {
            var relativeX = p.X - grid.Minx;
            var relativeY = p.Y - grid.Miny;
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

            var overlapMapMinX = (int)(LatLonInfo.MinLonValue / deltax) - 1;
            var overlapMapMaxX = (int)(LatLonInfo.MaxLonValue / deltax);

            // The deltaX = 20 example scenario, then set the value 9 to 8 and -10 to -9            

            // Similar to if (LatLonInfo.MaxLonValue % deltax == 0) without floating presicion issue
            if (Math.Abs(LatLonInfo.MaxLonValue % deltax - 0) < Numbers.Epsilon)
            {
                overlapMapMaxX--;
                overlapMapMinX++;
            }

            var idxx = (int)(p.X / deltax);
            if (p.X < 0) idxx--;

            if (Math.Abs(LatLonInfo.MaxLonValue % p.X - 0) < Numbers.Epsilon)
            {
                if (p.X < 0) idxx++;
                else idxx--;
            }

            if (idxx == overlapMapMinX) idxx = overlapMapMaxX;

            idx = idxx;

            // Latitude never wraps around with Google Maps, ignore 90, -90 wrap-around for latitude
            idy = (int)(relativeY / deltay);

            return new[] { idx, idy };
        }


        public IPoints RunClusterAlgo(ClusterInfo clusterInfo)
        {
            // Skip points outside the grid
            IPoints filtered = clusterInfo.IsFilterData ? FilterDataset(Dataset, Grid) : Dataset;

            // Put points in buckets
            foreach (var p in filtered.Data)
            {
                var idxy = GetPointMappedIds(p, Grid, DeltaX, DeltaY);
                var idx = idxy[0];
                var idy = idxy[1];

                // Bucket id
                var id = GetId(idx, idy);

                // Bucket exists, add point
                if (BucketsLookup.ContainsKey(id))
                {
                    BucketsLookup[id].Points.Add(p);
                }
                // New bucket, create and add point
                else
                {
                    var bucket = new Bucket(idx, idy, id);
                    bucket.Points.Add(p);
                    BucketsLookup.Add(id, bucket);
                }
            }

            // Calculate centroid for all buckets
            SetCentroidForAllBuckets(BucketsLookup.Values);

            // Merge if gridpoint is to close
            if (AlgoConfig.DoMergeGridIfCentroidsAreCloseToEachOther) MergeClustersGrid();

            if (AlgoConfig.DoUpdateAllCentroidsToNearestContainingPoint) UpdateAllCentroidsToNearestContainingPoint();

            // Check again
            // Merge if gridpoint is to close
            if (AlgoConfig.DoMergeGridIfCentroidsAreCloseToEachOther
                && AlgoConfig.DoUpdateAllCentroidsToNearestContainingPoint)
            {
                MergeClustersGrid();
                // And again set centroid to closest point in bucket 
                UpdateAllCentroidsToNearestContainingPoint();
            }

            return GetClusterResult(Grid);
        }
    }
}