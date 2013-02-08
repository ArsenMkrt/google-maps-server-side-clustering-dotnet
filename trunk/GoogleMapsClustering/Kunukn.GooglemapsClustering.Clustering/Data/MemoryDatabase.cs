using System;
using System.Collections.Generic;
using System.Linq;

using P = Kunukn.GooglemapsClustering.Clustering.Data.P;
using IP = Kunukn.GooglemapsClustering.Clustering.Contract.IP;
using IPoints = Kunukn.GooglemapsClustering.Clustering.Contract.IPoints;

//using IPsKnn = SingleDetectLibrary.Code.Contract.IPoints;
//using IPKnn = SingleDetectLibrary.Code.Contract.IP;
//using IPointsKnn = SingleDetectLibrary.Code.Contract.IPoints;
//using PointsKnn = SingleDetectLibrary.Code.Data.Points;
//using PKnn = SingleDetectLibrary.Code.Data.P;

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
                        
            // Randomize order, when limit take is used, data are random located
            var rand = new Random();
            var c = points.Count;
            for (var i = 0; i < c; i++)
            {
                var p = points[i];
                
                var a = rand.Next(c);
                var b = rand.Next(c);
                var temp = points[a];
                points[a] = points[b];
                points[b] = temp;
            }

            Points = points;

            _flag = true;
        }
    }
}
