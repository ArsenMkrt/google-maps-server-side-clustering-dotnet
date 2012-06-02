using System.Web.Script.Serialization;

namespace Kunukn.GooglemapsClustering.Data
{
    public class JsonSetTypeReply : JsonReply
    {
        public string Content { get; set; }        
        
        // Don't include in json reply        
        [ScriptIgnore]
        public string Type { get; set; }
        [ScriptIgnore]
        public string IsChecked { get; set; }

        public JsonSetTypeReply()
        {            
            Type = string.Empty;
            IsChecked = string.Empty;                       
        }      
    }
}
