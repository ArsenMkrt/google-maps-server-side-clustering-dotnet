namespace Kunukn.GooglemapsClustering.Data
{
    public class JsonReplyBase
    {
        public int ReplyId { get; set; } // for async mismatch check
        public string TokenValid { get; set; } // is access token valid
        public string Success { get; set; } // operation result

        public JsonReplyBase()
        {
            ReplyId = 1;
            TokenValid = "1";
            Success = "1";
        }
    }
}
