using System;
using System.Collections.Generic;
using System.Linq;
using SingleDetectLibrary.Code;
using SingleDetectLibrary.Code.Contract;
using SingleDetectLibrary.Code.Data;
using SingleDetectLibrary.Code.StrategyPattern;

namespace Kunukn.GooglemapsClustering.Clustering.Data
{
    public static class MemoryDatabase
    {
        private static bool _flag;
        public static List<Kunukn.GooglemapsClustering.Clustering.Data.P> Points { get; private set; }
        public static object Data { get; private set; } // data container

        public static void SetPoints(List<Kunukn.GooglemapsClustering.Clustering.Data.P> points)
        {
            if(_flag || points == null)
            {
                return;
            }

            // Used for testing K nearest neighbor
            SingleDetectLibrary.Code.Contract.IPoints ps = new Points(); 
            ps.Data = new List<SingleDetectLibrary.Code.Data.P>();

            // Randomize order, when limit take is used, data are random located
            var rand = new Random();
            var c = points.Count;
            for (var i = 0; i < c; i++)
            {
                var p = points[i];
                ps.Data.Add(new SingleDetectLibrary.Code.Data.P { X = p.Lon, Y = p.Lat, }); // used for testing K nearest neighbor

                var a = rand.Next(c);
                var b = rand.Next(c);
                var temp = points[a];
                points[a] = points[b];
                points[b] = temp;
            }

            Points = points;


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
            ISingleDetectAlgorithm algo = new SingleDetectAlgorithm(ps, rect, StrategyType.Grid);
            Data = algo;

            //var origin = new SingleDetectLibrary.Code.Data.P { X = 0, Y = 0 };
            //var duration = algo.UpdateKnn(origin, 3);

            //var res = algo.Knn.NNs.Data.OrderBy(i => i.Distance).ToList();
            //var rr = res;
             
            //// Update strategy
            //algo.SetAlgorithmStrategy(new NaiveStrategy());

            //// Use algo
            //duration = algo.UpdateKnn(origin, 3);

            //var res2 = algo.Knn.NNs.Data.OrderBy(i => i.Distance).ToList();
            //var assd = res2;

            _flag = true;
        }
    }
}
