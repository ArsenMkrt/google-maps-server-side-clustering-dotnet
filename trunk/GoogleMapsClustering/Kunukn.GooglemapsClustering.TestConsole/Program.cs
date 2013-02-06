﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using Kunukn.GooglemapsClustering.Clustering;
using Kunukn.GooglemapsClustering.Clustering.Algorithm;
using Kunukn.GooglemapsClustering.Clustering.Data;
using Kunukn.GooglemapsClustering.Clustering.Utility;
using Kunukn.GooglemapsClustering.WebGoogleMapClustering;

namespace Kunukn.GooglemapsClustering.TestConsole
{
    /// <summary>
    /// Author: Kunuk Nykjaer
    /// Misc. methods for quick testing and converting data
    /// Not NUnit or testcases, just quick testing and parsing methods
    /// </summary>
    class Program
    {
        private static readonly Action<object> WL = Console.WriteLine;

        static readonly Random Rand = new Random();
        public static DateTime Starttime;
        static void Main(string[] args)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            //LatLonParse();
            //PMapTest();
            //GenerateRandomDatasetToCsvFile();
            //MergeDataset("PointsDK.csv", "PointsNZ.csv", "Points.csv");            
            //ReadSerializedFileAndCount();
            //ReadCsvFileAndCount();
            //TestNormalize();
            //TestFloor();
            //TestLonLatDiff();
            //TestDegree();
            //TestRadian();
            //TestBaseGetCentroidFromClusterLatLon();
            //ConvertDataset();
            //ReworkDataset("Points.csv");

            stopwatch.Stop();

            WL(string.Format("Elapsed: {0}\n",stopwatch.Elapsed.ToString()));
            WL("\npress a key to exit ...");
            Console.ReadKey();
        }

        // lat lon points data
        static void LatLonParse()
        {
            //http://download.geonames.org/export/dump/

            var rand = new Random();
            const string name = "cities1000";
            List<string> lines = FileUtil.ReadFile(string.Format("c:\\temp\\{0}.txt", name));
            var dataset = new List<P>();

            foreach (var line in lines)
            {
                var delimiters = new [] { ' ', '\t' };
                var arr = line.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                if (arr.Length < 10) continue;

                double? lon = null;
                double? lat = null;
                string id = arr[0];

                for (var i = 1; i < arr.Length - 2; i++)
                {
                    double d1, d2, d3;
                    var dp1 = Double.TryParse(arr[i], NumberStyles.Float, NumberFormatInfo.InvariantInfo, out d1);
                    var dp2 = Double.TryParse(arr[i + 1], NumberStyles.Float, NumberFormatInfo.InvariantInfo, out d2);
                    var dp3 = Double.TryParse(arr[i + 2], NumberStyles.Float, NumberFormatInfo.InvariantInfo, out d3);

                    if (dp1 && dp2 && dp3)
                    {
                        lat = d2;
                        lon = d3;
                        break;
                    }
                    if (dp1 && dp2)
                    {
                        lat = d1;
                        lon = d2;
                        break;
                    }
                }

                if (lon != null && lat != null && MathTool.IsLonValid(lon.Value) && MathTool.IsLatValid(lat.Value))
                {
                    dataset.Add(new P(lon.Value, lat.Value) { I = id, T = (rand.Next(3) + 1).ToString() });
                }
            }

            SaveCsvData(dataset, new FileInfo(string.Format("c:\\temp\\{0}.csv", name)));
        }

        static void GenerateRandomDatasetToCsvFile()
        {
            string execfolder = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
            var path = execfolder + @"\..\..\..\Kunukn.GooglemapsClustering.Web\AreaGMC\Files\PointsNZ.csv";
            var fi = new FileInfo(path);
            if (fi.Directory != null && fi.Directory.Exists)
            {
                SaveCsvData(GenerateRandomDataset(10000, new Boundary { Minx = 172, Maxx = -178, Miny = -48, Maxy = -38 }), fi);
            }
                
            else
            {
                throw new ApplicationException("Path is invalid: " + path);
            }                
        }

        static void PMapTest()
        {
            var b = new Boundary { Minx = -180, Maxx = 180, Miny = -90, Maxy = 90 };
            var dx = 20;
            var dy = 20;

            var xy = GridCluster.GetPointMappedIds(new P { Lon = 175, Lat = 35 }, b, dx, dy);
            Console.WriteLine("x: {0}   y: {1}", xy[0], xy[1]);

            xy = GridCluster.GetPointMappedIds(new P { Lon = 175, Lat = 35 }, b, dx, dy);
            Console.WriteLine("x: {0}   y: {1}", xy[0], xy[1]);

            xy = GridCluster.GetPointMappedIds(new P { Lon = 180, Lat = 35 }, b, dx, dy);
            Console.WriteLine("x: {0}   y: {1}", xy[0], xy[1]);

            xy = GridCluster.GetPointMappedIds((new P { Lon = 181, Lat = 35 }).Normalize(), b, dx, dy);
            Console.WriteLine("x: {0}   y: {1}", xy[0], xy[1]);

            xy = GridCluster.GetPointMappedIds((new P { Lon = -181, Lat = 35 }).Normalize(), b, dx, dy);
            Console.WriteLine("x: {0}   y: {1}", xy[0], xy[1]);
        }

        static IEnumerable<P> GenerateRandomDataset(int n, Boundary b)
        {
            b.Normalize();

            // random points
            var list = new List<P>();
            for (var i = 0; i < n; i++)
            {
                var lat = (b.Miny + b.AbsY * Rand.NextDouble()).NormalizeLatitude();
                var lon = (b.Minx + b.AbsX * Rand.NextDouble()).NormalizeLongitude();
                list.Add(new P { I = Guid.NewGuid().ToString(), C = 1, Lat = lat, Lon = lon, T = i.ToString() });
            }

            return list;
        }

        static void SaveSerData(List<P> dataset)
        {
            FileUtil.SaveDataSetToFile(dataset);
            var list = FileUtil.LoadDataSetFromFile();
        }

        static void SaveCsvData(IEnumerable<P> dataset, FileInfo filepath)
        {
            //var dataset = FileUtil.LoadDataSetFromFile();
            var save = dataset.Select(p => string.Format("{0};{1};{2};{3}", p.X, p.Y, p.I, p.T)).ToList();
            
            //foreach (var s in save)  Console.WriteLine(s);
            FileUtil.WriteFile(save, filepath);
        }

        static void MergeDataset(string ds1, string ds2, string name)
        {
            string execfolder = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
            var path1 = execfolder + @"\..\..\..\Kunukn.GooglemapsClustering.Web\AreaGMC\Files\" + ds1;
            var path2 = execfolder + @"\..\..\..\Kunukn.GooglemapsClustering.Web\AreaGMC\Files\" + ds2;

            var fi1 = new FileInfo(path1);
            if (fi1.Directory == null || !fi1.Directory.Exists)
            {
                throw new ApplicationException("Path is invalid: " + path1);
            }
                
            fi1 = new FileInfo(path2);
            if (fi1.Directory == null || !fi1.Directory.Exists)
            {
                throw new ApplicationException("Path is invalid: " + path2);
            }
                
            var points1 = Dataset.LoadDatasetFromDatabase(path1, LoadType.Csv);
            var all = points1.ToList();
            var points2 = Dataset.LoadDatasetFromDatabase(path2, LoadType.Csv);
            all.AddRange(points2);

            var path = execfolder + @"\..\..\..\Kunukn.GooglemapsClustering.Web\AreaGMC\Files\" + name;
            SaveCsvData(all, new FileInfo(path));
        }

        static void ReworkDataset(string name)
        {            
            string execfolder = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
            var path = execfolder + @"\..\..\..\Kunukn.GooglemapsClustering.Web\AreaGMC\Files\" + name;
            var fi = new FileInfo(path);
            if (fi.Directory == null || !fi.Directory.Exists)
            {
                throw new ApplicationException("Path is invalid: " + path);
            }
                
            var points = Dataset.LoadDatasetFromDatabase(path, LoadType.Csv);
            foreach (var p in points)
            {
                // something               
            }

            SaveCsvData(points, new FileInfo(path));
        }

        static void ConvertDataset()
        {
            var list = FileUtil.ReadFile(@"c:\temp\temp.csv");
            var dataset = new List<P>();

            foreach (var s in list)
            {
                var arr = s.Split(';');
                if (arr.Length == 4)
                {
                    double x = PBase.ToValue(arr[0]).Value;
                    double y = PBase.ToValue(arr[1]).Value;
                    var i = arr[2];
                    var t = arr[3];

                    dataset.Add(new P(x, y) { I = i, T = t });
                }
            }

            FileUtil.SaveDataSetToFile(dataset, "temp.ser");
        }


        static void TestLonLatDiff()
        {
            Console.WriteLine(MathTool.LatLonDiff(-170, 170));
            Console.WriteLine(MathTool.LatLonDiff(170, -170));

            Console.WriteLine(MathTool.LatLonDiff(-10, 10));
            Console.WriteLine(MathTool.LatLonDiff(10, -10));

            Console.WriteLine(MathTool.LatLonDiff(10, 30));
            Console.WriteLine(MathTool.LatLonDiff(30, 10));

            Console.WriteLine(MathTool.LatLonDiff(-10, -30));
            Console.WriteLine(MathTool.LatLonDiff(-30, -10));

            Console.WriteLine(MathTool.LatLonDiff(-80, 80));
            Console.WriteLine(MathTool.LatLonDiff(80, -80));
        }

        static void TestDegree()
        {
            Console.WriteLine(DataExtensions.LatLonToDegree(-180).ToString());
            Console.WriteLine(DataExtensions.LatLonToDegree(-170).ToString());
            Console.WriteLine(DataExtensions.LatLonToDegree(10).ToString());
            Console.WriteLine(DataExtensions.LatLonToDegree(180).ToString());
            Console.WriteLine(DataExtensions.LatLonToDegree(0).ToString());
        }

        static void TestRadian()
        {
            Console.WriteLine(DataExtensions.LatLonToRadian(-180).ToString());
            Console.WriteLine(DataExtensions.LatLonToRadian(-170).ToString());
            Console.WriteLine(DataExtensions.LatLonToRadian(10).ToString());
            Console.WriteLine(DataExtensions.LatLonToRadian(180).ToString());
            Console.WriteLine(DataExtensions.LatLonToRadian(0).ToString());
        }

        static void TestFloor()
        {
            int delta = 10;
            //Console.WriteLine(((int)(179 / delta)) * delta);
            //Console.WriteLine(((int)(-179 / delta)) * delta);

            double a = -185;
            double b = 175;
            //Console.WriteLine(a.NormalizeLongitude() );            
            //Console.WriteLine(b.NormalizeLongitude() );

            var v = MathTool.FloorLatLon(a, delta) - delta;
            Console.WriteLine(v);
            var w = MathTool.FloorLatLon(b, delta) - delta;
            Console.WriteLine(w);

            //var v = MathTool.FloorLatLon(a, delta);
            //Console.WriteLine(v);
            //var w = MathTool.FloorLatLon(b, delta);
            //Console.WriteLine(w);

            Console.WriteLine(v.NormalizeLongitude());
            Console.WriteLine(w.NormalizeLongitude());
        }

        static void TestBaseGetCentroidFromClusterLatLon()
        {
            var list = new List<P>();
            list.Add(new P(50, 20));
            list.Add(new P(70, 60));
            //list.Add(new P(-160, 40));
            //list.Add(new P(-170, 40));


            var p = ClusterAlgorithmBase.GetCentroidFromClusterLatLon(list);
            Console.WriteLine(p);
        }

        static void ReadSerializedFileAndCount()
        {
            List<P> points = FileUtil.LoadDataSetFromFile();
            var b = new Boundary { Minx = -179, Maxx = 179, Miny = -90, Maxy = 90 };
            int i = points.Count(p => MathTool.IsInside(b, p));
                        
            Console.WriteLine("count: " + i);
        }

        static void ReadCsvFileAndCount()
        {
            string execfolder = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
            var path = execfolder + @"\..\..\..\Kunukn.GooglemapsClustering.Web\AreaGMC\Files\Points.csv";
            var points = Dataset.LoadDatasetFromDatabase(path,LoadType.Csv);
            
            //var b = new Boundary { Minx = -179, Maxx = 179, Miny = -90, Maxy = 90 };
            //int i = points.Count(p => MathTool.IsInside(b, p));

            Console.WriteLine("count: " + points.Count);
        }


        static void TestNormalize()
        {
            Console.WriteLine("*** Lon");
            Console.WriteLine(DataExtensions.NormalizeLongitude(0));
            Console.WriteLine(DataExtensions.NormalizeLongitude(190));
            Console.WriteLine(DataExtensions.NormalizeLongitude(-190));
            Console.WriteLine(DataExtensions.NormalizeLongitude(-180));
            Console.WriteLine(DataExtensions.NormalizeLongitude(-181));
            Console.WriteLine(DataExtensions.NormalizeLongitude(180));
            Console.WriteLine(DataExtensions.NormalizeLongitude(181));
            Console.WriteLine(DataExtensions.NormalizeLongitude(200));
            Console.WriteLine(DataExtensions.NormalizeLongitude(-200));
            Console.WriteLine(DataExtensions.NormalizeLongitude(360));

            Console.WriteLine("*** Lat");
            Console.WriteLine(DataExtensions.NormalizeLatitude(0));
            Console.WriteLine(DataExtensions.NormalizeLatitude(-90));
            Console.WriteLine(DataExtensions.NormalizeLatitude(-91));
            Console.WriteLine(DataExtensions.NormalizeLatitude(90));
            Console.WriteLine(DataExtensions.NormalizeLatitude(91));
            Console.WriteLine(DataExtensions.NormalizeLatitude(120));
            Console.WriteLine(DataExtensions.NormalizeLatitude(-120));
            Console.WriteLine(DataExtensions.NormalizeLatitude(180));
        }
    }
}
