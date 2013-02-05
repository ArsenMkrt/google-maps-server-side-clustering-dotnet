using System.Collections.Generic;

namespace Kunukn.GooglemapsClustering.Clustering.Data.Json
{
    public class JsonGetMarkersReply : JsonReplyBase
    {                        
        public List<P> Points { get; set; } // markers or clusters        
        public List<Line> Polylines { get; set; } // google map draw lines

        public JsonGetMarkersReply()
        {
            Points = new List<P>();
            Polylines = new List<Line>();            
        }
    }
}
