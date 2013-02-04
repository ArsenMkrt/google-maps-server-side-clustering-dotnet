using System.ServiceModel;
using System.ServiceModel.Web;
using Kunukn.GooglemapsClustering.Clustering.Data;

namespace Kunukn.GooglemapsClustering.Clustering.WebService
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
        JsonGetMarkersReply GetMarkers(double nelat, double nelon, double swlat, double swlon,
                                 int zoomlevel, int gridx, int gridy, int zoomlevelClusterStop, string filter, int sendid);
        
        [OperationContract]        
        [WebInvoke(Method = "POST", UriTemplate = "AreaGMC/AjaxService/GetMarkerDetail",
            BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        JsonMarkerInfoReply GetMarkerDetail(string id, string type, int sendid);
        
        
        [OperationContract]
        [WebGet(
            UriTemplate = "hey",
            ResponseFormat = WebMessageFormat.Json)
        ]
        string Hey();
    }
}