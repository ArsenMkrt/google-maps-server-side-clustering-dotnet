using System.Collections.Generic;
using SingleDetectLibrary.Code.Data;

namespace Kunukn.GooglemapsClustering.Clustering.Data.Json
{
    public class JsonKnnReply
    {
        public string Data { get; set; }
        public List<PDist> Nns { get; set; }

        public JsonKnnReply()
        {                  
            Nns = new List<PDist>();     
        }          
    }
}
