using System.Collections.Generic;
using Kunukn.GooglemapsClustering.Clustering.Contract;

namespace Kunukn.GooglemapsClustering.Clustering.Data.Json
{
    public class JsonMarkersReply : JsonReplyBase
    {                        
        public IList<P> Markers { get; set; } // markers or clusters
        public IList<Line> Polylines { get; set; } // google map draw lines
        public int Count {get { return Markers.Count;} set { }}

        public JsonMarkersReply()
        {
            Markers = new List<P>();
            Polylines = new List<Line>();            
        }
    }
}
