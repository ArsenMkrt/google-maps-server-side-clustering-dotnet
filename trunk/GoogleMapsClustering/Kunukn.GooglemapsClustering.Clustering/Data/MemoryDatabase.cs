using System;
using System.Collections.Generic;

namespace Kunukn.GooglemapsClustering.Clustering.Data
{
    public static class MemoryDatabase
    {
        private static bool _flag;
        public static List<P> Points { get; private set; }

        public static void SetPoints(List<P> points)
        {
            if(_flag || points == null)
            {
                return;
            }

            // Randomize order, when limit take is used, data are random located
            var rand = new Random();
            var c = points.Count;
            for (var i = 0; i < c; i++)
            {
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
