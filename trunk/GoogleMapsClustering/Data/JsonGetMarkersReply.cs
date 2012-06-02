using System.Collections.Generic;

namespace Kunukn.GooglemapsClustering.Data
{
    public class JsonGetMarkersReply : JsonReply
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
