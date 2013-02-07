namespace Kunukn.GooglemapsClustering.Clustering.Data.Json
{
    public class JsonReplyBase
    {
        public int Rid { get; set; } // for async mismatch check        
        public string Ok { get; set; } // operation result

        public JsonReplyBase()
        {
            Rid = 1;  // ReplyId
            Ok = "1";
        }
    }
}
