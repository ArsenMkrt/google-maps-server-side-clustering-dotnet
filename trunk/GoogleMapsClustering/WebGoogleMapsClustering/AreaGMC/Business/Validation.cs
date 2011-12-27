using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Kunukn.GooglemapsClustering.Data;

namespace Kunukn.GooglemapsClustering.WebGoogleMapClustering.AreaGMC.Business
{
    public static class Validation
    {
        public static bool ValidateAccessToken(string access_token, DateTime sessionStart)
        {            
            var timeSpan = DateTime.UtcNow.Subtract(sessionStart);
            if (timeSpan.Minutes < 10)
                return true; //demo time

            return access_token != "todo"; // simple validate dummy test
            return true; //not used, all has access
        }

    }
}