using System.Collections.Generic;
using Kunukn.GooglemapsClustering.Clustering.Contract;

namespace Kunukn.GooglemapsClustering.Clustering.Data.Json
{
    public class JsonMarkersReply : JsonReplyBase
    {                        
        public IList<P> Points { get; set; } // markers or clusters
        public IList<Line> Polylines { get; set; } // google map draw lines        

        public JsonMarkersReply()
        {
            Points = new List<P>();
            Polylines = new List<Line>();            
        }
    }
}
