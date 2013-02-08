using System.Collections.Generic;

namespace Kunukn.GooglemapsClustering.Clustering.Data.Json
{
    public class JsonKnnReply
    {
        public string Data { get; set; }
        public IList<SingleDetectLibrary.Code.Data.PDist> Nns { get; set; } // nearest neighbors

        public JsonKnnReply()
        {
            Nns = new List<SingleDetectLibrary.Code.Data.PDist>();
        }
    }
}
