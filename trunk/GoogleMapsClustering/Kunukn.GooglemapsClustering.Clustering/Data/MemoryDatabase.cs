using System;
using System.Collections.Generic;
using System.Linq;
using Kunukn.GooglemapsClustering.Clustering.Contract;

using SingleDetectLibrary.Code;
using SingleDetectLibrary.Code.Contract;
using SingleDetectLibrary.Code.Data;
using SingleDetectLibrary.Code.StrategyPattern;

namespace Kunukn.GooglemapsClustering.Clustering.Data
{
    public static class MemoryDatabase
    {
        private static bool _flag;
        public static Kunukn.GooglemapsClustering.Clustering.Contract.IPoints Points { get; private set; }
        public static object Data { get; private set; } // data container

        public static void SetPoints(Kunukn.GooglemapsClustering.Clustering.Contract.IPoints points)
        {
            if(_flag || points == null) return;
            
            // Used for testing K nearest neighbor
            SingleDetectLibrary.Code.Contract.IPoints knnDataset = new SingleDetectLibrary.Code.Data.Points(); 
            knnDataset.Data = new List<SingleDetectLibrary.Code.Data.P>();

            // Randomize order, when limit take is used, data are random located
            var rand = new Random();
            var c = points.Count;
            for (var i = 0; i < c; i++)
            {
                var p = points[i];
                knnDataset.Data.Add(new SingleDetectLibrary.Code.Data.P { X = p.X, Y = p.Y, }); // used for testing K nearest neighbor

                var a = rand.Next(c);
                var b = rand.Next(c);
                var temp = points[a];
                points[a] = points[b];
                points[b] = temp;
            }

            Points = points;


            //// Used for testing K nearest neighbor
            //var rect = new SingleDetectLibrary.Code.Data.Rectangle
            //{
            //    XMin = -180,
            //    XMax = 180,
            //    YMin = -90,
            //    YMax = 90,
            //    MaxDistance = 20,
            //};
            //rect.Validate();
            //ISingleDetectAlgorithm algo = new SingleDetectAlgorithm(knnDataset, rect, StrategyType.Grid);
            //Data = algo;

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
