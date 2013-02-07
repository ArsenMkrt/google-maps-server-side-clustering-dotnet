using System.ServiceModel;
using System.ServiceModel.Web;
using Kunukn.GooglemapsClustering.Clustering.Data.Json;

namespace Kunukn.GooglemapsClustering.Clustering.WebService
{
    /// <summary>
    /// WCF REST
    /// </summary>
    [ServiceContract]    
    public interface IAjaxService
    {        
        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "GetMarkers",
            BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        JsonMarkersReply GetMarkers(double nelat, double nelon, double swlat, double swlon,
                                 int zoomlevel, int gridx, int gridy, int zoomlevelClusterStop, string filter, int sendid);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "GetMarkerInfo",
            BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        JsonMarkerInfoReply GetMarkerInfo(string id, string type, int sendid);
           
   
        [OperationContract]
        [WebGet(
            UriTemplate = "GetInfo",
            ResponseFormat = WebMessageFormat.Json)
        ]
        JsonInfoReply GetInfo();


        [OperationContract]
        [WebGet(
            UriTemplate = "GetKnn/{s}",
            ResponseFormat = WebMessageFormat.Json)
        ]
        JsonKnnReply GetKnn(string s);
    }

}