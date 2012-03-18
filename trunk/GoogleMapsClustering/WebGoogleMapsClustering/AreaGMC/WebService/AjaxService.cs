using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel.Activation;
using System.Web;
using System.Web.Script.Serialization;
using Kunukn.GooglemapsClustering.Clustering;
using Kunukn.GooglemapsClustering.Data;
using Kunukn.GooglemapsClustering.WebGoogleMapClustering.AreaGMC.Business;
using Kunukn.GooglemapsClustering.WebGoogleMapClustering.AreaGMC.Code.Helpers;

namespace Kunukn.GooglemapsClustering.WebGoogleMapClustering.AreaGMC.WebService
{
    [AspNetCompatibilityRequirements(RequirementsMode
        = AspNetCompatibilityRequirementsMode.Allowed)]
    public class AjaxService : IAjaxService
    {
        protected static string NotValidReply(int sendid)
        {
            var jss = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
            var jsonReply = new JsonReply { ReplyId = sendid, TokenValid = Names._0, Success = Names._0 };
            var json = jss.Serialize(jsonReply);
            return json;
        }

        public string GetMarkers(string access_token, double nelat, double nelon, double swlat, double swlon, int zoomlevel, int gridx, int gridy, int zoomlevelClusterStop, int sendid)
        {
            SystemHelper.Assert(HttpContext.Current.Session != null, "Session is null");

            var isValid = Validation.ValidateAccessToken(access_token);
            if (!isValid) return NotValidReply(sendid);
            
            var jsonReceive = new JsonReceive(access_token, nelat, nelon, swlat, swlon, zoomlevel, gridx, gridy, zoomlevelClusterStop, sendid);

            var GMC_ClusteringEnabled = SessionHelper.GetClusteringEnabled();
            var clusteringEnabled = GMC_ClusteringEnabled != Names._0 || Config.AlwaysClusteringEnabledWhenZoomLevelLess > jsonReceive.Zoomlevel;

            var jss = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
            JsonGetMarkersReply reply;
            
            jsonReceive.Viewport.ValidateLatLon(); // validate google map viewport input
            jsonReceive.Viewport.Normalize();

            var allpoints = SessionHelper.GetDataset();

            var typeFilter = SessionHelper.GetTypeFilter();           
            var dataset = allpoints; //unfiltered atm            
            if (typeFilter.Count > 0)
            {
                dataset = new List<P>();
                foreach (var p in allpoints)
                    if (typeFilter.Contains(p.T) == false)
                        dataset.Add(p);
            }

            // Clustering
            if (clusteringEnabled && jsonReceive.Zoomlevel < jsonReceive.ZoomlevelClusterStop)
            {
                var clusterAlgo = new GridCluster(dataset, jsonReceive);
                var clusterPoints = clusterAlgo.GetCluster(new ClusterInfo { ZoomLevel = jsonReceive.Zoomlevel });

                reply = new JsonGetMarkersReply { Points = clusterPoints, ReplyId = sendid, Polylines = clusterAlgo.Lines };
                string json = jss.Serialize(reply);
                return json;
            }

            // No clustering but number of items returned is restricted
            List<P> filteredDataset = ClusterAlgorithmBase.FilterDataset(dataset, jsonReceive.Viewport);
            List<P> filteredDatasetMaxPoints = filteredDataset.Take(Config.MaxMarkersReturned).ToList();

            reply = new JsonGetMarkersReply { Points = filteredDatasetMaxPoints, ReplyId = sendid, Success = Names._1 };
            return jss.Serialize(reply);
        }

        public string GetMarkerDetail(string access_token, string id, string type, int sendid)
        {
            SystemHelper.Assert(HttpContext.Current.Session != null, "Session is null");
            
            var isValid = Validation.ValidateAccessToken(access_token);
            if (!isValid) return NotValidReply(sendid);

            var jss = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
            var reply = new JsonMarkerInfoReply { Id = id, Type = type, ReplyId = sendid };
                        
            reply.BuildContent();

            reply.Success = Names._1;
            return jss.Serialize(reply);
        }

        public string GetAccessToken(string username, string password, int sendid)
        {
            SystemHelper.Assert(HttpContext.Current.Session != null, "Session is null");

            var jss = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
            // dummy set access token
            var reply = new JsonGetAccessTokenReply { ReplyId = sendid, AccessToken = "dummyValidValue", Success = Names._1 };
            return jss.Serialize(reply);
        }

        public string SetType(string access_token, string type, string isChecked, int sendid)
        {
            SystemHelper.Assert(HttpContext.Current.Session != null, "Session is null");
               
            var isValid = Validation.ValidateAccessToken(access_token);
            if (!isValid) return NotValidReply(sendid);

            var jss = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
            var reply = new JsonSetTypeReply { Type = type, IsChecked = isChecked, ReplyId = sendid };

            // do something with the type, isChecked                        
            if (string.IsNullOrWhiteSpace(type) == false)
            {
                // is meta type?
                if (type.StartsWith(Names.gmc_meta))
                {
                    if (type == Names.gmc_meta_clustering) // toggle clustering
                    {
                        string isEnabled = isChecked.ToLower() == Names._true ? Names._1 : Names._0;
                        SessionHelper.SetClusteringEnabled(isEnabled);                        
                    }
                }
                // marker type filtering
                else
                {
                    var typeFilter = SessionHelper.GetTypeFilter();

                    if (isChecked.ToLower() == Names._true)
                    {
                        if (typeFilter.Contains(type))
                            typeFilter.Remove(type);
                    }
                    else
                    {
                        if (typeFilter.Contains(type) == false)
                            typeFilter.Add(type);
                    }

                    SessionHelper.SetTypeFilter(typeFilter);                    
                }
            }

            reply.Success = Names._1;
            return jss.Serialize(reply);
        }



        # region :: TEST WCF AJAX

        public AjaxDataTest GetData()
        {
            var data = new AjaxDataTest { Value = " WCF Ajax get ok", };
            return data;
        }
        public AjaxDataTest DoPost(string input)
        {
            var data = new AjaxDataTest { Value = input + " reply", };
            return data;
        }
        public AjaxDataTest GetDataByArg(string arg)
        {
            var data = new AjaxDataTest { Value = arg, };
            return data;
        }
        # endregion :: TEST WCF AJAX


    }

}