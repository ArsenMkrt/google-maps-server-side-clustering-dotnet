using System;
using System.Collections.Generic;
using System.Web;
using Kunukn.GooglemapsClustering.Data;

namespace Kunukn.GooglemapsClustering.WebGoogleMapClustering.AreaGMC.Code.Helpers
{
    public static class SessionHelper
    {
        public static DateTime GetStartTime()
        {
            var d = HttpContext.Current.Session[SessionKeys.GMC_SessionStart] as DateTime?;
            SystemHelper.Assert(d != null, "sessionStart is null");            
            return d == null ? new DateTime(1800,1,1) : d.Value;
        }

        public static HashSet<string> GetTypeFilter()
        {
            var typeFilter = HttpContext.Current.Session[SessionKeys.GMC_Filter] as HashSet<string>;
            SystemHelper.Assert(typeFilter != null, "Error! typeFilter is null, check Session or Global.asax setup");
            return typeFilter;
        }

        public static void SetClusteringEnabled(string value)
        {
            HttpContext.Current.Session[SessionKeys.GMC_ClusteringEnabled] = value;
        }
        public static string GetClusteringEnabled()
        {
            var GMC_ClusteringEnabled = HttpContext.Current.Session[SessionKeys.GMC_ClusteringEnabled] as string;
            return GMC_ClusteringEnabled;
        }

        public static void SetTypeFilter(HashSet<string> typeFilter)
        {
            HttpContext.Current.Session[SessionKeys.GMC_Filter] = typeFilter;// set filter    
        }

        public static List<P> GetDataset()
        {
            var dataset = HttpContext.Current.Application[SessionKeys.GMC_Dataset] as List<P>;            
            SystemHelper.AssertNotNullOrEmpty(dataset,
                                              "Error! dataset is null or empty, check HttpContext.Current.Application or Global.asax setup");
            return dataset;
        }
    }
}