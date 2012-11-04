using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Activation;
using System.Web;
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
        protected static JsonReplyBase NotValidReply(int sendid)
        {            
            var jsonReply = new JsonReplyBase { ReplyId = sendid, TokenValid = Names._0, Success = Names._0 };            
            return jsonReply;
        }

        public JsonGetMarkersReply GetMarkers(string access_token, double nelat, double nelon, double swlat, double swlon, int zoomlevel, int gridx, int gridy, int zoomlevelClusterStop, string filter, int sendid)
        {            
            var isValid = Validation.ValidateAccessToken(access_token);
            if (!isValid)
            {
                JsonReplyBase nvr = NotValidReply(sendid);
                return new JsonGetMarkersReply { ReplyId = nvr.ReplyId, Success = nvr.Success, TokenValid = nvr.TokenValid};
            }
            
            var jsonReceive = new JsonGetMarkersReceive(access_token, nelat, nelon, swlat, swlon, zoomlevel, gridx, gridy, zoomlevelClusterStop, filter, sendid);
            
            var clusteringEnabled = jsonReceive.IsClusteringEnabled || Config.AlwaysClusteringEnabledWhenZoomLevelLess > jsonReceive.Zoomlevel;
            
            JsonGetMarkersReply reply;
            
            jsonReceive.Viewport.ValidateLatLon(); // Validate google map viewport input
            jsonReceive.Viewport.Normalize();

            // Get all points from memory, application cache
            var allpoints = MemoryDatabase.Points;
                                        
            var dataset = allpoints; // unfiltered at the moment
            if (jsonReceive.TypeFilter.Count > 0)
            {
                // Filter data by typeFilter value
                dataset = allpoints.Where(p => jsonReceive.TypeFilter.Contains(p.T) == false).ToList();
            }

            // Clustering
            if (clusteringEnabled && jsonReceive.Zoomlevel < jsonReceive.ZoomlevelClusterStop)
            {
                // Create new instance for every ajax request with input all points and json data
                var clusterAlgo = new GridCluster(dataset, jsonReceive);

                // Calculate data to be displayed
                var clusterPoints = clusterAlgo.GetCluster(new ClusterInfo { ZoomLevel = jsonReceive.Zoomlevel });

                // Prepare data to the client
                reply = new JsonGetMarkersReply { Points = clusterPoints, ReplyId = sendid, Polylines = clusterAlgo.Lines };                

                // Return client data
                return reply;
            }

            // If we are here then there are no clustering
            // The number of items returned is restricted to avoid json data overflow
            List<P> filteredDataset = ClusterAlgorithmBase.FilterDataset(dataset, jsonReceive.Viewport);
            List<P> filteredDatasetMaxPoints = filteredDataset.Take(Config.MaxMarkersReturned).ToList();

            reply = new JsonGetMarkersReply { Points = filteredDatasetMaxPoints, ReplyId = sendid, Success = Names._1 };            
            return reply;
        }

        public JsonMarkerInfoReply GetMarkerDetail(string access_token, string id, string type, int sendid)
        {
            // Guard clause
            SystemHelper.Assert(HttpContext.Current.Session != null, "Session is null");
            
            var isValid = Validation.ValidateAccessToken(access_token);
            if (!isValid)
            {
                JsonReplyBase nvr = NotValidReply(sendid);
                return new JsonMarkerInfoReply { TokenValid = nvr.TokenValid, Success = nvr.Success, ReplyId = nvr.ReplyId };
            }
            
            var reply = new JsonMarkerInfoReply { Id = id, Type = type, ReplyId = sendid };                        
            reply.BuildContent();
            reply.Success = Names._1;            
            return reply;
        }

        public JsonGetAccessTokenReply GetAccessToken(string username, string password, int sendid)
        {
            // Guard clause
            SystemHelper.Assert(HttpContext.Current.Session != null, "Session is null");
            
            // Dummy set access token
            var reply = new JsonGetAccessTokenReply { ReplyId = sendid, AccessToken = "dummyValidValue", Success = Names._1 };
            return reply;            
        }        
    }

}