﻿using System;
using System.Text;
using System.Web.Script.Serialization;

namespace Kunukn.GooglemapsClustering.Clustering.Data.Json
{
    public class JsonMarkerInfoReply : JsonReplyBase
    {
        public string Content { get; set; }        

        [ScriptIgnore] // don't include in json reply
        public string Type  { get; set; }
        [ScriptIgnore]
        public string Id { get; set; }

        public JsonMarkerInfoReply()
        {
            Id = string.Empty;
            Type = string.Empty;                        
        }

        public void BuildContent()
        {
            var sb = new StringBuilder();            
            sb.AppendLine("<div>");            
            sb.AppendFormat("Time: {0}<br/>",DateTime.Now);
            sb.AppendFormat("Id: {0}<br /> Type: {1}", Id, Type);            
            sb.AppendLine("</div>");

            Content = sb.ToString();
        }        
    }
}