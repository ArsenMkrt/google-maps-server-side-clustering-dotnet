using System;
using System.Collections.Generic;

namespace Kunukn.GooglemapsClustering.WebGoogleMapClustering.AreaGMC.Code.Helpers
{
    public class SystemHelper
    {
        public static void Assert(bool b, string s)
        {
            if (!b)
            {
                throw new ApplicationException(s);
            }                
        }

        public static void AssertNotNullOrEmpty<T>(List<T> list, string s)
        {
            if (list==null || list.Count==0)
            {
                throw new ApplicationException(s);
            }                
        }
    }
}