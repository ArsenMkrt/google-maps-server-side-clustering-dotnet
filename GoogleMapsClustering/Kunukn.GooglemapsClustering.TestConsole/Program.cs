using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Kunukn.GooglemapsClustering.Clustering.Algorithm;
using Kunukn.GooglemapsClustering.Clustering.Data;
using Kunukn.GooglemapsClustering.Clustering.Utility;

using P = Kunukn.GooglemapsClustering.Clustering.Data.P;
using IP = Kunukn.GooglemapsClustering.Clustering.Contract.IP;
using IPoints = Kunukn.GooglemapsClustering.Clustering.Contract.IPoints;

using Kunukn.SingleDetectLibrary.Code;
using Kunukn.SingleDetectLibrary.Code.Contract;
using Kunukn.SingleDetectLibrary.Code.StrategyPattern;
using IPsKnn = Kunukn.SingleDetectLibrary.Code.Contract.IPoints;
using IPKnn = Kunukn.SingleDetectLibrary.Code.Contract.IP;
using IPointsKnn = Kunukn.SingleDetectLibrary.Code.Contract.IPoints;
using PointsKnn = Kunukn.SingleDetectLibrary.Code.Data.Points;
using PKnn = Kunukn.SingleDetectLibrary.Code.Data.P;

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

            //Temp();
            //Knn();
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

            WL(string.Format("Elapsed: {0}\n", stopwatch.Elapsed.ToString()));
            WL("\npress a key to exit ...");
            Console.ReadKey();
        }

        static void Temp()
        {
          
        }

        static void Knn()
        {
            IPoints points = Dataset.LoadDataset(@"c:\temp\points.csv");

            // Used for testing K nearest neighbor
            IPointsKnn dataset = new PointsKnn();
            dataset.Data.AddRange(points.Data.Select(i => i as IPKnn));

            // Used for testing K nearest neighbor
            var rect = new SingleDetectLibrary.Code.Data.Rectangle
            {
                XMin = -180,
                XMax = 180,
                YMin = -90,
                YMax = 90,
                MaxDistance = 20,
            };
            rect.Validate();
            const int k = 3;

            ISingleDetectAlgorithm algo = new SingleDetectAlgorithm(dataset, rect, StrategyType.Grid);

            var origin = new SingleDetectLibrary.Code.Data.P { X = 0, Y = 0 };            
            algo.UpdateIndex(origin);

            var duration = algo.UpdateKnn(origin, k);

            // Print result
            WL(string.Format("{0} msec. {1}:", algo.Strategy.Name, duration));
            WL("K Nearest Neighbors:");
            WL(string.Format("Origin: {0}", origin));
            WL(string.Format("Distance sum: {0}", algo.Knn.GetDistanceSum()));
            algo.Knn.NNs.OrderBy(i => i.Distance).ToList().ForEach(WL);


            // Update strategy
            algo.SetAlgorithmStrategy(new NaiveStrategy());

            // Use algo
            duration = algo.UpdateKnn(origin, k);

            // Print result
            WL(string.Format("\n{0} msec. {1}:", algo.Strategy.Name, duration));
            WL("K Nearest Neighbors:");
            WL(string.Format("Distance sum: {0}", algo.Knn.GetDistanceSum()));
            algo.Knn.NNs.OrderBy(i => i.Distance).ToList().ForEach(WL);
        }

        // lat lon points data
        static void LatLonParse()
        {
            //http://download.geonames.org/export/dump/

            var rand = new Random();
            const string name = "cities1000";
            var lines = FileUtil.ReadFile(string.Format("c:\\temp\\{0}.txt", name));
            var dataset = new Points();
            const int numOfType = 3;

            foreach (var line in lines)
            {
                var delimiters = new[] { ' ', '\t' };
                var arr = line.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                if (arr.Length < 10) continue;

                double? lon = null;
                double? lat = null;
                var id = arr[0].ToInt();

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

                if (lon.HasValue && lat.HasValue && MathTool.IsLonValid(lon.Value) && MathTool.IsLatValid(lat.Value))
                {
                    dataset.Add(new P { X = lon.Value, Y = lat.Value, I = id, T = (rand.Next(numOfType) + 1) });
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
                SaveCsvData(GenerateRandomDataset(10000,
                    new Boundary { Minx = 172, Maxx = -178, Miny = -48, Maxy = -38 }), fi);
            }
            else
            {
                throw new ApplicationException("Path is invalid: " + path);
            }
        }

        static void PMapTest()
        {
            var b = new Boundary { Minx = -180, Maxx = 180, Miny = -90, Maxy = 90 };
            const int dx = 20;
            const int dy = 20;

            var xy = GridCluster.GetPointMappedIds(new P { X = 175, Y = 35 }, b, dx, dy);
            Console.WriteLine("x: {0}   y: {1}", xy[0], xy[1]);

            xy = GridCluster.GetPointMappedIds(new P { X = 175, Y = 35 }, b, dx, dy);
            Console.WriteLine("x: {0}   y: {1}", xy[0], xy[1]);

            xy = GridCluster.GetPointMappedIds(new P { X = 180, Y = 35 }, b, dx, dy);
            Console.WriteLine("x: {0}   y: {1}", xy[0], xy[1]);

            xy = GridCluster.GetPointMappedIds((new P { X = 181, Y = 35 }).Normalize(), b, dx, dy);
            Console.WriteLine("x: {0}   y: {1}", xy[0], xy[1]);

            xy = GridCluster.GetPointMappedIds((new P { X = -181, Y = 35 }).Normalize(), b, dx, dy);
            Console.WriteLine("x: {0}   y: {1}", xy[0], xy[1]);
        }

        static IPoints GenerateRandomDataset(int n, Boundary b)
        {
            b.Normalize();

            // Random points
            var list = new List<IP>();
            for (var i = 0; i < n; i++)
            {
                var lat = (b.Miny + b.AbsY * Rand.NextDouble()).NormalizeLatitude();
                var lon = (b.Minx + b.AbsX * Rand.NextDouble()).NormalizeLongitude();
                list.Add(new P { I = Rand.Next(1000000000), C = 1, Y = lat, X = lon, T = i });
            }

            return new Points {Data = list};
        }

        static void SaveSerData(IPoints dataset)
        {
            FileUtil.SaveDataSetToFile(dataset);
            var list = FileUtil.LoadDataSetFromFile();
        }

        static void SaveCsvData(IPoints dataset, FileInfo filepath)
        {
            //var dataset = FileUtil.LoadDataSetFromFile();
            var save = dataset.Data.Select(p => string.Format("{0};{1};{2};{3}",
                p.X, p.Y, p.I, p.T)).ToList();

            //foreach (var s in save) WL(s);
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

            IPoints all = Dataset.LoadDataset(path1);
            var points2 = Dataset.LoadDataset(path2);
            all.Data.AddRange(points2.Data);
            

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

            var points = Dataset.LoadDataset(path);
            foreach (var p in points.Data)
            {
                // something               
            }

            SaveCsvData(points, new FileInfo(path));
        }

        static void ConvertDataset()
        {
            var list = FileUtil.ReadFile(@"c:\temp\temp.csv");
            var dataset = new Points();

            foreach (var s in list)
            {
                var arr = s.Split(';');
                if (arr.Length == 4)
                {
                    double x = arr[0].ToDouble();
                    double y = arr[1].ToDouble();
                    var i = arr[2].ToInt();
                    var t = arr[3].ToInt();

                    dataset.Add(new P { X = x, Y = y, I = i, T = t });
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
            var list = new List<IP>();
            list.Add(new P { X = 50, Y = 20 });
            list.Add(new P { X = 70, Y = 60 });
            //list.Add(new P { X = -160, Y = 40 });
            //list.Add(new P { X = -170, Y = 40 });


            var p = ClusterAlgorithmBase.GetCentroidFromClusterLatLon(
                new Points{Data = list});

            Console.WriteLine(p);
        }

        static void ReadSerializedFileAndCount()
        {
            var points = FileUtil.LoadDataSetFromFile();
            var b = new Boundary { Minx = -179, Maxx = 179, Miny = -90, Maxy = 90 };
            int i = points.Data.Count(p => MathTool.IsInside(b, p));

            Console.WriteLine("count: " + i);
        }

        static void ReadCsvFileAndCount()
        {
            string execfolder = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
            var path = execfolder + @"\..\..\..\Kunukn.GooglemapsClustering.Web\AreaGMC\Files\Points.csv";
            var points = Dataset.LoadDataset(path);

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
