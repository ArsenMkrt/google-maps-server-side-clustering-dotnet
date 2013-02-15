using Kunukn.GooglemapsClustering.Clustering.Data;

namespace Kunukn.GooglemapsClustering.Clustering.Utility
{
    public static class GmcInit
    {
        private static bool _done;
        /// <summary>
        /// Init with file path to csv file
        /// </summary>
        /// <param name="path"></param>
        public static void Init(string path)
        {
            // Only run once
            if(_done) return;
            
            _done = true;            
            // Database load simulation            
            MemoryDatabase.SetFilepath(path);
            MemoryDatabase.GetPoints(); // preload points into memory
        }
    }
}
