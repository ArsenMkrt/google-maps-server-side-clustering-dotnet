using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kunukn.GooglemapsClustering.Data
{
    public class JsonReply
    {
        public int ReplyId { get; set; } // for async mismatch check
        public string TokenValid { get; set; } // is access token valid
        public string Success { get; set; } // operation result

        public JsonReply()
        {
            ReplyId = 1;
            TokenValid = "1";
            Success = "1";
        }
    }
}
