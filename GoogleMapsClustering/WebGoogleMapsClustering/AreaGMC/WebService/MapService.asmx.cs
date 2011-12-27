using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using System.Web.Services;
using Kunukn.GooglemapsClustering.Clustering;
using Kunukn.GooglemapsClustering.Data;
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
        public const string _true = "true";
        public const string _false = "false";
        public const string _valid = "valid";
        public const string _0 = "0";
        public const string _1 = "1";


        protected static string NotValidReply(int sendid)
        {
            var jss = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
            var jsonReply = new JsonReply { ReplyId = sendid, TokenValid = _0, Success = _0 };
            var json = jss.Serialize(jsonReply);
            return json;
        }

        [WebMethod(EnableSession = true)]
        [ScriptMethod(UseHttpGet = false, ResponseFormat = ResponseFormat.Json)]
        public string GetMarkers(string access_token, double nelat, double nelon, double swlat, double swlon, int zoomlevel, int gridx, int gridy, int zoomlevelClusterStop, int sendid)
        {
            if (Session == null) throw new ApplicationException("Session is null");

            var sessionStart = Session[Names.SessionStart] as DateTime?;
            if (sessionStart == null) throw new ApplicationException("sessionStart is null, check Global.asax");

            var jss = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
            JsonGetMarkersReply reply;

            var isValid = Validation.ValidateAccessToken(access_token, sessionStart.Value);
            if (!isValid) return NotValidReply(sendid);                
            
            var jsonReceive = new JsonReceive(access_token, nelat, nelon, swlat, swlon, zoomlevel, gridx, gridy, zoomlevelClusterStop, sendid);
            jsonReceive.Viewport.ValidateLatLon(); // validate google map viewport input
            jsonReceive.Viewport.Normalize();

            var allpoints = Application[Names.Dataset] as List<P>; // get cached points from DB simulation
            var dataset = allpoints; //unfiltered atm
            var typeFilter = Session[Names.Filter] as HashSet<string>; // get filter           

            if (allpoints == null || allpoints.Count == 0)
                throw new ApplicationException("DB dataset is null or empty");
            if (typeFilter == null)
                throw new ApplicationException("Error! typeFilter is null, check Session or Global.asax setup");

            if (typeFilter.Count > 0)
            {
                dataset = new List<P>();
                foreach (var p in allpoints)
                    if (typeFilter.Contains(p.T) == false)
                        dataset.Add(p);
            }


            // too far out, world is showing countries multiple times
            if (jsonReceive.Zoomlevel <= 1)
            {
                // no data
                reply = new JsonGetMarkersReply { ReplyId = sendid, Success = _1};
                return jss.Serialize(reply);
            }

            // clustering within this zoom level)
            if (jsonReceive.Zoomlevel < jsonReceive.ZoomlevelClusterStop)
            {
                var clusterAlgo = new GridCluster(dataset, jsonReceive);
                var clusterPoints = clusterAlgo.GetCluster(new ClusterInfo { ZoomLevel = jsonReceive.Zoomlevel });

                reply = new JsonGetMarkersReply { Points = clusterPoints, ReplyId = sendid, Polylines = clusterAlgo.Lines };
                string json = jss.Serialize(reply);
                return json;
            }

            // no clustering                             
            Boundary viewportExtended = GridCluster.GetBoundaryExtended(jsonReceive);
            List<P> filteredDataset = GridCluster.FilterDataset(dataset, viewportExtended);

            reply = new JsonGetMarkersReply {Points = filteredDataset, ReplyId = sendid, Success = _1};
            return jss.Serialize(reply);            
        }

        [WebMethod(EnableSession = true)]
        [ScriptMethod(UseHttpGet = false, ResponseFormat = ResponseFormat.Json)]
        public string GetMarkerDetail(string access_token, string id, string type, int sendid)
        {
            if (Session == null) throw new ApplicationException("Session is null");

            var jss = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
            var reply = new JsonMarkerInfoReply { Id = id, Type = type, ReplyId = sendid };

            var sessionStart = Session[Names.SessionStart] as DateTime?;
            if (sessionStart == null) throw new ApplicationException("sessionStart is null");
            
            var isValid = Validation.ValidateAccessToken(access_token, sessionStart.Value);            
            if (!isValid) return NotValidReply(sendid);
            
            reply.BuildContent();

            reply.Success = _1;
            return jss.Serialize(reply);
        }

        [WebMethod(EnableSession = true)]
        [ScriptMethod(UseHttpGet = false, ResponseFormat = ResponseFormat.Json)]
        public string GetAccessToken(string username, string password, int sendid)
        {
            var jss = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
            // dummy set access token
            var reply = new JsonGetAccessTokenReply {ReplyId = sendid, AccessToken = _valid, Success = _1};
            return jss.Serialize(reply);
        }



        [WebMethod(EnableSession = true)]
        [ScriptMethod(UseHttpGet = false, ResponseFormat = ResponseFormat.Json)]
        public string SetType(string access_token, string type, string isChecked, int sendid)
        {
            if (Session == null) throw new ApplicationException("Session is null");

            var sessionStart = Session[Names.SessionStart] as DateTime?;
            if (sessionStart == null) throw new ApplicationException("sessionStart is null");
            
            var isValid = Validation.ValidateAccessToken(access_token, sessionStart.Value);
            if (!isValid) return NotValidReply(sendid);
            
            var jss = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
            var reply = new JsonSetTypeReply { Type = type, IsChecked = isChecked, ReplyId = sendid };

            // do something with the type, isChecked

            var typeFilter = Session[Names.Filter] as HashSet<string>;
            if(typeFilter==null)
                throw new ApplicationException("Error! typeFilter is null, check Session or Global.asax setup");

            if (isChecked.ToLower() == _true)
            {
                if (typeFilter.Contains(type))
                    typeFilter.Remove(type);
            }
            else
            {
                if (typeFilter.Contains(type) == false)
                    typeFilter.Add(type);
            }

            Session[Names.Filter] = typeFilter;// set filter

            reply.Success = _1;           
            return jss.Serialize(reply);
        }
    }
}
