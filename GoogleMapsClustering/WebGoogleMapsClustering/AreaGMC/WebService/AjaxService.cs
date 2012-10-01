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
        protected static JsonReply NotValidReply(int sendid)
        {            
            var jsonReply = new JsonReply { ReplyId = sendid, TokenValid = Names._0, Success = Names._0 };            
            return jsonReply;
        }

        public JsonGetMarkersReply GetMarkers(string access_token, double nelat, double nelon, double swlat, double swlon, int zoomlevel, int gridx, int gridy, int zoomlevelClusterStop, int sendid)
        {
            // Guard clause
            SystemHelper.Assert(HttpContext.Current.Session != null, "Session is null");

            var isValid = Validation.ValidateAccessToken(access_token);
            if (!isValid)
            {
                var nvr = NotValidReply(sendid);
                return new JsonGetMarkersReply { ReplyId = nvr.ReplyId, Success = nvr.Success, TokenValid = nvr.TokenValid};
            }
            
            var jsonReceive = new JsonReceive(access_token, nelat, nelon, swlat, swlon, zoomlevel, gridx, gridy, zoomlevelClusterStop, sendid);

            var GMC_ClusteringEnabled = SessionHelper.GetClusteringEnabled();
            var clusteringEnabled = GMC_ClusteringEnabled != Names._0 || Config.AlwaysClusteringEnabledWhenZoomLevelLess > jsonReceive.Zoomlevel;
            
            JsonGetMarkersReply reply;
            
            jsonReceive.Viewport.ValidateLatLon(); // validate google map viewport input
            jsonReceive.Viewport.Normalize();

            // Get all points from memory, application cache
            var allpoints = SessionHelper.GetDataset();

            // Filter data is saved in user session
            // todo This can be avoided by saving it in client area hidden fields
            var typeFilter = SessionHelper.GetTypeFilter();           

            var dataset = allpoints; //unfiltered at the moment
            if (typeFilter.Count > 0)
            {
                // Filter data by typeFilter value
                dataset = allpoints.Where(p => typeFilter.Contains(p.T) == false).ToList();
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
                var nvr = NotValidReply(sendid);
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

        public JsonSetTypeReply SetType(string access_token, string type, string isChecked, int sendid)
        {
            // Guard clause
            SystemHelper.Assert(HttpContext.Current.Session != null, "Session is null");
            
            var isValid = Validation.ValidateAccessToken(access_token);
            if (!isValid)
            {
                var nvr = NotValidReply(sendid);
                return new JsonSetTypeReply { TokenValid = nvr.TokenValid, Success = nvr.Success, ReplyId = nvr.ReplyId};
            }
            
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
                        {
                            typeFilter.Remove(type);
                        }                            
                    }
                    else
                    {
                        if (typeFilter.Contains(type) == false)
                        {
                            typeFilter.Add(type);
                        }                            
                    }

                    SessionHelper.SetTypeFilter(typeFilter);                    
                }
            }

            reply.Success = Names._1;            
            return reply;
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