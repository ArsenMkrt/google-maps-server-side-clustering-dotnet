using System.Collections.Generic;


namespace Kunukn.GooglemapsClustering.Data
{
    public class JsonReply
    {
        public List<P> Points { get; set; } // markers or clusters
        public int ReplyId { get; set; } // for async mismatch check
        public List<Line> Polylines { get; set; } // google map draw lines
                
        public JsonReply()
        {
            Points = new List<P>();
            Polylines = new List<Line>();
            ReplyId = 1;
        }

    }
}
