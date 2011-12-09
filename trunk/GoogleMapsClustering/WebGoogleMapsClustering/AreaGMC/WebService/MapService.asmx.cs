using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using System.Web.Services;
using Kunukn.GooglemapsClustering.Clustering;
using Kunukn.GooglemapsClustering.Data;
using Kunukn.GooglemapsClustering.DataUtility;
using Kunukn.GooglemapsClustering.MathUtility;
using Kunukn.GooglemapsClustering.WebGoogleMapClustering.AreaGMC.Business;


namespace Kunukn.GooglemapsClustering.WebGoogleMapClustering.AreaGMC.WebService
{
    /// <summary>
    /// Summary description for MapService
    /// Author: Kunuk Nykjaer
    /// </summary>
    [WebService(Namespace = "dk.jory.gmc.GooglemapClusteringDemo")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]

    // Web Service to be called from script, using ASP.NET AJAX
    [System.Web.Script.Services.ScriptService]
    public class MapService : System.Web.Services.WebService
    {              
        [WebMethod]
        [ScriptMethod(UseHttpGet = false, ResponseFormat = ResponseFormat.Json)]
        public string GetMarkers(string access_token, double nelat, double nelon, double swlat, double swlon, int zoomlevel, int gridx, int gridy, int zoomlevelClusterStop, int sendid)
        {
            var jss = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
            JsonReply jsonReply;
            string json;

            var isValid = Validation.ValidateAccessToken(access_token);
            if(! isValid)
                throw new ApplicationException( string.Format("access_token is invalid: {0}",access_token) );


            var jsonReceive = new JsonReceive(access_token, nelat, nelon, swlat, swlon, zoomlevel, gridx, gridy, zoomlevelClusterStop, sendid);
            jsonReceive.Viewport.ValidateLatLon(); // validate google map viewport input
            jsonReceive.Viewport.Normalize();
            
            List<P> dataset = Application[Names.Dataset] as List<P>; // get cached points from DB simulation
             if(dataset==null || dataset.Count==0)       
                 throw new ApplicationException("DB dataset is null or empty");

            
            // too far out, world is showing countries multiple times
            //if (jsonReceive.Zoomlevel <= 1)
            //{
            //    // no data
            //    jsonReply = new JsonReply { ReplyId = sendid };
            //    return jss.Serialize(jsonReply);                
            //}

            // clustering within this zoom level)
            if (jsonReceive.Zoomlevel < jsonReceive.ZoomlevelClusterStop)
            {
                var clusterAlgo = new GridCluster(dataset, jsonReceive);
                var clusterPoints = clusterAlgo.GetCluster(new ClusterInfo(){ZoomLevel = jsonReceive.Zoomlevel});

                jsonReply = new JsonReply { Points = clusterPoints, ReplyId = sendid, Polylines = clusterAlgo.Lines };
                json = jss.Serialize(jsonReply);
                return json;
            }

            // no clustering                             
            Boundary viewportExtended = GridCluster.GetBoundaryExtended(jsonReceive);
            List<P> filteredDataset = GridCluster.FilterDataset(dataset, viewportExtended);

            jsonReply = new JsonReply { Points = filteredDataset, ReplyId = sendid };
            json = jss.Serialize(jsonReply);
            return json;
        }

        [WebMethod]
        [ScriptMethod(UseHttpGet = false, ResponseFormat = ResponseFormat.Json)]
        public string GetMarkerDetail(string access_token, string id, string type, int sendid)
        {
            var jss = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
            var reply = new JsonMarkerInfoReply{Id=id, Type = type, ReplyId = sendid};

            var isValid = Validation.ValidateAccessToken(access_token);
            
            if(isValid) reply.BuildContent();
            else reply.BuildInvalidAccessTokenContent();

            return jss.Serialize(reply);
        }
       
    }
}
