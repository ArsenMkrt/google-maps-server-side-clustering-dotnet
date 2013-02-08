using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel.Activation;
using Kunukn.GooglemapsClustering.Clustering.Algorithm;
using Kunukn.GooglemapsClustering.Clustering.Contract;
using Kunukn.GooglemapsClustering.Clustering.Data;
using Kunukn.GooglemapsClustering.Clustering.Data.Json;
using SingleDetectLibrary.Code;
using SingleDetectLibrary.Code.Contract;
using SingleDetectLibrary.Code.Data;
using P = Kunukn.GooglemapsClustering.Clustering.Data.P;
using IP = Kunukn.GooglemapsClustering.Clustering.Contract.IP;
using Ps = Kunukn.GooglemapsClustering.Clustering.Data.Points;
using IPs = Kunukn.GooglemapsClustering.Clustering.Contract.IPoints;

namespace Kunukn.GooglemapsClustering.Clustering.WebService
{
    [AspNetCompatibilityRequirements(RequirementsMode
        = AspNetCompatibilityRequirementsMode.Allowed)]
    public class AjaxService : IAjaxService
    {
        protected static JsonReplyBase NotValidReply(int sendid)
        {
            var jsonReply = new JsonReplyBase { Rid = sendid, Ok = "0" };
            return jsonReply;
        }

        public JsonMarkersReply GetMarkers(double nelat, double nelon, double swlat, double swlon, int zoomlevel, int gridx, int gridy, int zoomlevelClusterStop, string filter, int sendid)
        {
            var jsonReceive = new JsonGetMarkersReceive(nelat, nelon, swlat, swlon, zoomlevel, gridx, gridy, zoomlevelClusterStop, filter, sendid);

            var clusteringEnabled = jsonReceive.IsClusteringEnabled || AlgoConfig.AlwaysClusteringEnabledWhenZoomLevelLess > jsonReceive.Zoomlevel;

            JsonMarkersReply reply;

            jsonReceive.Viewport.ValidateLatLon(); // Validate google map viewport input (is always valid)
            jsonReceive.Viewport.Normalize();

            // Get all points from memory
            var allpoints = MemoryDatabase.Points;

            var dataset = allpoints; // unfiltered at the moment            
            if (jsonReceive.TypeFilter.Count > 0)
            {
                // Filter data by typeFilter value
                dataset.Data = allpoints.Data.Where(p => jsonReceive.TypeFilter.Contains(p.T) == false).ToList();
            }

            // Clustering
            if (clusteringEnabled && jsonReceive.Zoomlevel < jsonReceive.ZoomlevelClusterStop)
            {
                // Create new instance for every ajax request with input all points and json data
                var clusterAlgo = new GridCluster(dataset, jsonReceive);

                // Calculate data to be displayed
                var clusterPoints = clusterAlgo.GetCluster(new ClusterInfo { ZoomLevel = jsonReceive.Zoomlevel });

                // Prepare data to the client
                reply = new JsonMarkersReply { Points = DataConvert(clusterPoints), Rid = sendid, Polylines = clusterAlgo.Lines };

                // Return client data
                return reply;
            }

            // If we are here then there are no clustering
            // The number of items returned is restricted to avoid json data overflow
            IPs filteredDataset = ClusterAlgorithmBase.FilterDataset(dataset, jsonReceive.Viewport);
            IPs filteredDatasetMaxPoints = new Ps { Data = filteredDataset.Data.Take(AlgoConfig.MaxMarkersReturned).ToList() };

            reply = new JsonMarkersReply { Points = DataConvert(filteredDatasetMaxPoints), Rid = sendid };
            return reply;
        }

           public JsonMarkerInfoReply GetMarkerInfo(string id, string type, int sendid)
        {
            var reply = new JsonMarkerInfoReply { Id = id, Type = type, Rid = sendid };
            reply.BuildContent();
            return reply;
        }


        public JsonInfoReply GetInfo()
        {            
            var reply = new JsonInfoReply { 
                DbSize = MemoryDatabase.Points.Count,
                Points = DataConvert(new Ps { Data = MemoryDatabase.Points.Data.Take(3).ToList()})
                
            };            
            return reply;
        }


        // Todo finish imple
        // Preparing for K nearest neighbor
        public JsonKnnReply GetKnn(string s)
        {
            var invalid = new JsonKnnReply { Data = string.Format("invalid: {0}", s ?? "null") };
            if (string.IsNullOrEmpty(s)) return invalid;

            var arr = s.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            if (arr.Length != 3) return invalid;

            var lat = arr[0].Replace("_", ".");
            var lon = arr[1].Replace("_", ".");
            var neighbors = arr[2];

            double x, y;
            int k;

            var b = double.TryParse(lon, System.Globalization.NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out x);
            b &= double.TryParse(lat, System.Globalization.NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out y);
            b &= int.TryParse(neighbors, out k);

            if (!b) return invalid;

                  
            // knn algo
            var algo = MemoryDatabase.Data as ISingleDetectAlgorithm;
            if (algo == null) return invalid;

            // Use algo
            var origin = new SingleDetectLibrary.Code.Data.P {X = x, Y = y};
            var duration = algo.UpdateKnn(origin, k);
            
            return new JsonKnnReply { Data = string.Format("{0}; {1}; {2}; msec: {3}", x, y, k, duration), Nns = algo.Knn.NNs.Data};
        }

        
        /// <summary>
        /// Solve serializing to Json issue, use replace or use your own P type as you like 
        /// </summary>
        /// <param name="ps"></param>
        /// <returns></returns>
        protected static IList<P> DataConvert(IPs ps)
        {
            return ps.Data.Select(p => p as P).ToList();
        }

    }

}