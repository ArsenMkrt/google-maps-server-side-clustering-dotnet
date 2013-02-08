using System.Collections.Generic;
using Kunukn.SingleDetectLibrary.Code.Contract;

namespace Kunukn.GooglemapsClustering.Clustering.Data.Json
{
    public class JsonKnnReply
    {
        public string Data { get; set; }
        public IList<IPDist> Nns { get; set; } // nearest neighbors

        public JsonKnnReply()
        {
            Nns = new List<IPDist>();
        }
    }
}
