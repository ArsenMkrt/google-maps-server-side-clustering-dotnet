using System.ServiceModel;
using System.ServiceModel.Web;
using System.Web.Services;
using Kunukn.GooglemapsClustering.Data;

namespace Kunukn.GooglemapsClustering.WebGoogleMapClustering.AreaGMC.WebService
{
    [ServiceContract]    
    public interface IAjaxService
    {
        [OperationContract]
        [WebMethod(EnableSession = true)]
        [WebInvoke(Method = "POST", UriTemplate = "AreaGMC/AjaxService/GetMarkers",
            BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        JsonGetMarkersReply GetMarkers(string access_token, double nelat, double nelon, double swlat, double swlon,
                                 int zoomlevel, int gridx, int gridy, int zoomlevelClusterStop, int sendid);

        [OperationContract]
        [WebMethod(EnableSession = true)]
        [WebInvoke(Method = "POST", UriTemplate = "AreaGMC/AjaxService/GetMarkerDetail",
            BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        JsonMarkerInfoReply GetMarkerDetail(string access_token, string id, string type, int sendid);

        [OperationContract]
        [WebMethod(EnableSession = true)]
        [WebInvoke(Method = "POST", UriTemplate = "AreaGMC/AjaxService/GetAccessToken",
            BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        JsonGetAccessTokenReply GetAccessToken(string username, string password, int sendid);

        [OperationContract]
        [WebMethod(EnableSession = true)]
        [WebInvoke(Method = "POST", UriTemplate = "AreaGMC/AjaxService/SetType",
            BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        JsonSetTypeReply SetType(string access_token, string type, string isChecked, int sendid);



        // TEST WCF AJAX
        [OperationContract]
        [WebGet(UriTemplate = "AreaGMC/AjaxService/GetData", ResponseFormat = WebMessageFormat.Json)]
        AjaxDataTest GetData();


        [OperationContract]
        [WebGet(UriTemplate = "AreaGMC/AjaxService/GetDataByArg({arg})", ResponseFormat = WebMessageFormat.Json)]
        AjaxDataTest GetDataByArg(string arg);


        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "AreaGMC/AjaxService/PostData",
            BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        AjaxDataTest DoPost(string input);
    }
}