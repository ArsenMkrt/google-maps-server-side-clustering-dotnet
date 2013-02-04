using System.Collections.Generic;

namespace Kunukn.GooglemapsClustering.Clustering.Data
{
    public static class MemoryDatabase
    {
        private static bool _flag = false;
        public static List<P> Points { get; private set; }

        public static void SetPoints(List<P> points)
        {
            if(_flag)
            {
                return;
            }
            Points = points;
            _flag = true;
        }
    }
}
