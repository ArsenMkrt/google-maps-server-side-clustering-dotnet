using System.Collections.Generic;
using Kunukn.SingleDetectLibrary.Code.Contract;
using Kunukn.SingleDetectLibrary.Code.Data;

namespace Kunukn.GooglemapsClustering.Clustering.Data.Json
{
    public class JsonKnnReply : JsonReplyBase
    {                
        public IList<PDist> Nns { get; set; } // nearest neighbors
        public string Data { get; set; }
        
        public JsonKnnReply()
        {           
        }
    }
}
