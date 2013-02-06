using System.Collections.Generic;

namespace Kunukn.GooglemapsClustering.Clustering.Data.Json
{
    public class JsonMarkersReply : JsonReplyBase
    {                        
        public List<P> Points { get; set; } // markers or clusters        
        public List<Line> Polylines { get; set; } // google map draw lines        

        public JsonMarkersReply()
        {
            Points = new List<P>();
            Polylines = new List<Line>();            
        }
    }
}
