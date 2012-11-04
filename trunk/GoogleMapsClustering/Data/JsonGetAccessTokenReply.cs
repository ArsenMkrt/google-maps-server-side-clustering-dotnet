namespace Kunukn.GooglemapsClustering.Data
{
    public class JsonGetAccessTokenReply : JsonReplyBase
    {
        public string AccessToken { get; set; }
        public JsonGetAccessTokenReply()
        {            
            AccessToken = "dummy";
        }
    }
}
