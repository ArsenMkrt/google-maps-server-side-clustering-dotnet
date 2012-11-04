using System.ServiceModel;
using System.ServiceModel.Web;
using Kunukn.GooglemapsClustering.Data;

namespace Kunukn.GooglemapsClustering.WebGoogleMapClustering.AreaGMC.WebService
{
    /// <summary>
    /// WCF REST
    /// </summary>
    [ServiceContract]    
    public interface IAjaxService
    {
        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "AreaGMC/AjaxService/GetMarkers",
            BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        JsonGetMarkersReply GetMarkers(string access_token, double nelat, double nelon, double swlat, double swlon,
                                 int zoomlevel, int gridx, int gridy, int zoomlevelClusterStop, string filter, int sendid);
        
        [OperationContract]        
        [WebInvoke(Method = "POST", UriTemplate = "AreaGMC/AjaxService/GetMarkerDetail",
            BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        JsonMarkerInfoReply GetMarkerDetail(string access_token, string id, string type, int sendid);

        [OperationContract]        
        [WebInvoke(Method = "POST", UriTemplate = "AreaGMC/AjaxService/GetAccessToken",
            BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        JsonGetAccessTokenReply GetAccessToken(string username, string password, int sendid);        
    }
}