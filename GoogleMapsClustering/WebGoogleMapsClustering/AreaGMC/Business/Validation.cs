using System;
using Kunukn.GooglemapsClustering.WebGoogleMapClustering.AreaGMC.Code.Helpers;

namespace Kunukn.GooglemapsClustering.WebGoogleMapClustering.AreaGMC.Business
{
    public static class Validation
    {
        public static bool ValidateAccessToken(string access_token)
        {
            var sessionStart = SessionHelper.GetStartTime();

            var timeSpan = DateTime.UtcNow.Subtract(sessionStart);
            if (timeSpan.Minutes < 10)
            {
                return true; // demo time
            }
                
            return true; // validation not used, all has access            
        }
    }
}