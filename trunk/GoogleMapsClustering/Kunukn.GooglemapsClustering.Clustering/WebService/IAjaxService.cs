﻿using System.ServiceModel;
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
        JsonGetMarkersReply GetMarkers(double nelat, double nelon, double swlat, double swlon,
                                 int zoomlevel, int gridx, int gridy, int zoomlevelClusterStop, string filter, int sendid);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "GetMarkerDetail",
            BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        JsonMarkerInfoReply GetMarkerDetail(string id, string type, int sendid);

        
        [OperationContract]
        [WebGet(
            UriTemplate = "test",
            ResponseFormat = WebMessageFormat.Json)
        ]
        string Test();
    }

}