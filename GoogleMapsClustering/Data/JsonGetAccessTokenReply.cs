namespace Kunukn.GooglemapsClustering.Data
{
    public class JsonGetAccessTokenReply : JsonReply
    {
        public string AccessToken { get; set; }
        public JsonGetAccessTokenReply()
        {            
            AccessToken = "todo";
        }
    }
}
