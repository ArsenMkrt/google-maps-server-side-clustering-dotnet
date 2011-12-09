using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Web.Script.Serialization;
using Kunukn.GooglemapsClustering.Clustering;
using Kunukn.GooglemapsClustering.Data;
using Kunukn.GooglemapsClustering.DataUtility;
using Kunukn.GooglemapsClustering.MathUtility;
using Kunukn.GooglemapsClustering.WebGoogleMapClustering;


namespace Kunukn.GooglemapsClustering.TestConsole
{
    /// <summary>
    /// Author: Kunuk Nykjaer
    /// Misc. methods for quick testing and converting data
    /// Not NUnit or testcases, just quick testing methods
    /// </summary>
    class Program
    {
        static readonly Random Rand = new Random();
        public static DateTime Starttime;
        static void Main(string[] args)
        {
            Starttime = DateTime.Now;
            

            //PMapTest();
            //GenerateRandomDatasetToCSVFile();
            //MergeDataset("PointsDK.csv", "PointsNZ.csv", "Points.csv");
            //RunData();
            //Read();
            //TestNormalize();
            //TestFloor();
            //TestLonLatDiff();
            //TestDegree();
            //TestRadian();
            //TestBaseGetCentroidFromClusterLatLon();
            //ConvertDataset();
            //ReworkDataset("Points.csv");

            var timespend = DateTime.Now.Subtract(Starttime).TotalSeconds;
            Console.WriteLine(timespend + " sec. press a key ...");
            Console.ReadKey();
        }

        static void GenerateRandomDatasetToCSVFile()
        {
            string execfolder = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);                        
            var path = execfolder + @"\..\..\..\WebGoogleMapsClustering\AreaGMC\Files\PointsNZ.csv";                        
            var fi = new FileInfo(path);
            if (fi.Directory!=null && fi.Directory.Exists)
                SaveCSVData(GenerateRandomDataset(10000, new Boundary{Minx = 172, Maxx = -178, Miny = -48, Maxy = -38}), fi);
            else
                throw new ApplicationException("Path is invalid: "+path);
        }

        static void PMapTest()
        {
            int[] xy;
            var b = new Boundary {Minx = -180, Maxx = 180, Miny = -90, Maxy = 90};
            var dx = 20;
            var dy = 20;

            xy = GridCluster.GetPointMappedIds( new P{Lon = 175, Lat = 35}, b, dx,dy );
            Console.WriteLine("x: {0}   y: {1}", xy[0], xy[1]);

            xy = GridCluster.GetPointMappedIds(new P { Lon = 175, Lat = 35 }, b, dx, dy);
            Console.WriteLine("x: {0}   y: {1}", xy[0], xy[1]);

            xy = GridCluster.GetPointMappedIds(new P { Lon = 180, Lat = 35 }, b, dx, dy);
            Console.WriteLine("x: {0}   y: {1}", xy[0], xy[1]);

            xy = GridCluster.GetPointMappedIds( (new P { Lon = 181, Lat = 35 }).Normalize(), b, dx, dy);
            Console.WriteLine("x: {0}   y: {1}", xy[0], xy[1]);

            xy = GridCluster.GetPointMappedIds((new P { Lon = -181, Lat = 35 }).Normalize(), b, dx, dy);
            Console.WriteLine("x: {0}   y: {1}", xy[0], xy[1]);
        }

        static List<P> GenerateRandomDataset(int n, Boundary b)
        {
            b.Normalize();

            // random points
            var list = new List<P>();            
            for (int i = 0; i < n; i++)
            {                                
                var lat = (b.Miny + b.AbsY * Rand.NextDouble()).NormalizeLatitude();
                var lon = (b.Minx + b.AbsX * Rand.NextDouble()).NormalizeLongitude();
                list.Add(new P { I = Guid.NewGuid().ToString(), C = 1, Lat = lat, Lon = lon, T = i.ToString() });
            }
            
            return list;            
        }

        static void SaveSERData(List<P> dataset)
        {
            FileUtil.SaveDataSetToFile(dataset);
            var list1 = FileUtil.LoadDataSetFromFile();
        }

        static void SaveCSVData(List<P> dataset, FileInfo filepath)
        {
            //var dataset = FileUtil.LoadDataSetFromFile();
            var save = new List<string>();
            foreach (var p in dataset)
                save.Add(string.Format("{0};{1};{2};{3}", p.X, p.Y, p.I, p.T));

            //foreach (var s in save)  Console.WriteLine(s);
            FileUtil.WriteFile(save, filepath);
        }
       
        static void MergeDataset(string ds1, string ds2, string name)
        {
            string execfolder = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
            var path1 = execfolder + @"\..\..\..\WebGoogleMapsClustering\AreaGMC\Files\"+ds1;
            var path2 = execfolder + @"\..\..\..\WebGoogleMapsClustering\AreaGMC\Files\" + ds2;

            var fi1 = new FileInfo(path1);            
            if (fi1.Directory == null || !fi1.Directory.Exists)              
                throw new ApplicationException("Path is invalid: " + path1);
            fi1 = new FileInfo(path2);
            if (fi1.Directory == null || !fi1.Directory.Exists)
                throw new ApplicationException("Path is invalid: " + path2);

            var points1 = Dataset.LoadDatasetFromDatabase(path1, DataUtility.LoadType.Csv);
            var all = points1.ToList();
            var points2 = Dataset.LoadDatasetFromDatabase(path2, DataUtility.LoadType.Csv);
            all.AddRange(points2);

            var path = execfolder + @"\..\..\..\WebGoogleMapsClustering\AreaGMC\Files\" + name;
            SaveCSVData(all,new FileInfo(path));
        }

        static void ReworkDataset(string name)
        {
            Random r = new Random();
            string execfolder = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
            var path = execfolder + @"\..\..\..\WebGoogleMapsClustering\AreaGMC\Files\" + name;            
            var fi = new FileInfo(path);
            if (fi.Directory == null || !fi.Directory.Exists)
                throw new ApplicationException("Path is invalid: " + path);
         
            var points = Dataset.LoadDatasetFromDatabase(path, DataUtility.LoadType.Csv);
            foreach (var p in points)
            {
                // something               
            }

            SaveCSVData(points, new FileInfo(path));
        }

        static void ConvertDataset()
        {
            var list = FileUtil.ReadFile(@"c:\temp\temp.csv");
            var dataset = new List<P>();

            foreach (var s in list)
            {
                string[] arr = s.Split(';');
                if(arr.Length==4)
                {
                    double x = PBase.ToValue(arr[0]).Value;
                    double y = PBase.ToValue(arr[1]).Value;
                    var i = arr[2];
                    var t = arr[3];

                    dataset.Add(new P(x, y){I=i, T=t}  );
                }
            }

            FileUtil.SaveDataSetToFile(dataset, "temp.ser");
        }


        static void TestLonLatDiff()
        {
            Console.WriteLine(MathTool.LatLonDiff(-170,170));
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
            Console.WriteLine(DataExtensions.LatLonToDegree(-180).ToString() );
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
            list.Add(new P(50,20) );
            list.Add(new P(70, 60));
            //list.Add(new P(-160, 40));
            //list.Add(new P(-170, 40));


            var p = ClusterAlgorithmBase.GetCentroidFromClusterLatLon(list);
            Console.WriteLine(p);
        }

        static void Read()
        {
            var list = FileUtil.LoadDataSetFromFile();
            var b = new Boundary {Minx = 179, Maxx = -179, Miny = -90, Maxy = 90};
            int i = 0;
            foreach (var p in list)
            {
                if(MathTool.IsInside(b, p))
                {
                    //Console.WriteLine(p);
                    i++;
                }
            }
            Console.WriteLine("count "+i);
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
