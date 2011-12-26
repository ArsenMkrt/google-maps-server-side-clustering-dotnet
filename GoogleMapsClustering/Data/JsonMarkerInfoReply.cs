using System;
using System.Text;
using System.Web.Script.Serialization;

namespace Kunukn.GooglemapsClustering.Data
{
    public class JsonMarkerInfoReply
    {
        public string Content { get; set; }
        public int ReplyId { get; set; }

        [ScriptIgnore]  //dont include in json reply
        public string Type  { get; set; }
        [ScriptIgnore]
        public string Id { get; set; }

        public JsonMarkerInfoReply()
        {
            Id = string.Empty;
            Type = string.Empty;            
            ReplyId = 1;
        }

        public void BuildContent()
        {
            var sb = new StringBuilder();            
            sb.AppendLine("<div>");
            //sb.AppendLine("<b>todo implement marker info </b> ");
            sb.AppendFormat("Time: {0}<br/>",DateTime.Now);
            sb.AppendFormat("Id: {0}<br/> Type: {1}", Id, Type);
            sb.AppendLine("</div>");

            Content = sb.ToString();
        }


        public void BuildInvalidAccessTokenContent()
        {
            var sb = new StringBuilder();
            sb.AppendLine("<div>");
            sb.AppendLine("<b>You have an invalid token, please relogin</b> ");
            sb.AppendFormat("Time: {0}<br/>", DateTime.Now);            
            sb.AppendLine("</div>");

            Content = sb.ToString();
        }
    }
}
