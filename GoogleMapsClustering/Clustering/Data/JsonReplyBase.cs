namespace Kunukn.GooglemapsClustering.Clustering.Data
{
    public class JsonReplyBase
    {
        public int ReplyId { get; set; } // for async mismatch check        
        public string Success { get; set; } // operation result

        public JsonReplyBase()
        {
            ReplyId = 1;            
            Success = "1";
        }
    }
}
