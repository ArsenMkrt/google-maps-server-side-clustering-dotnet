using System;
using System.Collections.Generic;
using System.Linq;
using Kunukn.SingleDetectLibrary.Code;
using Kunukn.SingleDetectLibrary.Code.Contract;
using P = Kunukn.GooglemapsClustering.Clustering.Data.P;
using IP = Kunukn.GooglemapsClustering.Clustering.Contract.IP;
using IPoints = Kunukn.GooglemapsClustering.Clustering.Contract.IPoints;

// K Nearest Neighbor
using IPsKnn = Kunukn.SingleDetectLibrary.Code.Contract.IPoints;
using IPKnn = Kunukn.SingleDetectLibrary.Code.Contract.IP;
using IPointsKnn = Kunukn.SingleDetectLibrary.Code.Contract.IPoints;
using PointsKnn = Kunukn.SingleDetectLibrary.Code.Data.Points;
using PKnn = Kunukn.SingleDetectLibrary.Code.Data.P;

namespace Kunukn.GooglemapsClustering.Clustering.Data
{
    public static class MemoryDatabase
    {        
        private static bool _flag;
        public static IPoints Points { get; private set; }
        public static object Data { get; private set; } // data container

        public static void SetPoints(IPoints points)
        {
            if(_flag || points == null) return;
            
            
            // Randomize order, when limit take is used for max marker display
            // random locations are selected
            var rand = new Random();
            var c = points.Count;
            for (var i = 0; i < c; i++)
            {
                //var p = points[i];
                
                var a = rand.Next(c);
                var b = rand.Next(c);
                var temp = points[a];
                points[a] = points[b];
                points[b] = temp;
            }

            Points = points;
            SetKnnAlgo(points);
            _flag = true;
        }

        // Used for testing K nearest neighbor
        static void SetKnnAlgo(IPoints points)
        {            
            IPointsKnn dataset = new PointsKnn();
            dataset.Data.AddRange(points.Data.Select(i => i as IPKnn));            
            var rect = new SingleDetectLibrary.Code.Data.Rectangle
            {
                XMin = -190,
                XMax = 190,
                YMin = -100,
                YMax = 100,
                MaxDistance = 20,
            };
            rect.Validate();
            
            ISingleDetectAlgorithm algo = new SingleDetectAlgorithm(dataset, rect, StrategyType.Grid);
            Data = algo;
        }       
    }
}
