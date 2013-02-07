using System.Collections.Generic;

namespace Kunukn.GooglemapsClustering.Clustering.Data.Json
{
    public class JsonInfoReply
    {
        public int DbSize { get; set; } // size of p in database
        public List<P> Points { get; set; }

        public JsonInfoReply()
        {              
            Points = new List<P>();
        }          
    }
}
