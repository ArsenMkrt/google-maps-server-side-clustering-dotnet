namespace Kunukn.GooglemapsClustering.Clustering.Data.Json
{
    public abstract class JsonReplyBase
    {
        public int Rid { get; set; } // for async mismatch check        
        public string Ok { get; set; } // operation result

        protected JsonReplyBase()
        {
            Rid = 1;  // ReplyId
            Ok = "1";
        }
    }
}
