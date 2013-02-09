using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel.Activation;
using Kunukn.GooglemapsClustering.Clustering.Algorithm;
using Kunukn.GooglemapsClustering.Clustering.Data;
using Kunukn.GooglemapsClustering.Clustering.Data.Json;
using Kunukn.GooglemapsClustering.Clustering.Utility;

using P = Kunukn.GooglemapsClustering.Clustering.Data.P;
using Points = Kunukn.GooglemapsClustering.Clustering.Data.Points;
using IPoints = Kunukn.GooglemapsClustering.Clustering.Contract.IPoints;

using ISingleDetectAlgorithm = Kunukn.SingleDetectLibrary.Code.Contract.ISingleDetectAlgorithm;
using PDist = Kunukn.SingleDetectLibrary.Code.Data.PDist;

namespace Kunukn.GooglemapsClustering.Clustering.WebService
{
    [AspNetCompatibilityRequirements(RequirementsMode
        = AspNetCompatibilityRequirementsMode.Allowed)]
    public class AjaxService : IAjaxService
    {
        // Post
        public JsonMarkersReply Markers(double nelat, double nelon, double swlat, double swlon, int zoomlevel, int gridx, int gridy, int zoomlevelClusterStop, string filter, int sendid)
        {
            var jsonReceive = new JsonGetMarkersReceive(nelat, nelon, swlat, swlon, zoomlevel, gridx, gridy, zoomlevelClusterStop, filter, sendid);

            var clusteringEnabled = jsonReceive.IsClusteringEnabled || AlgoConfig.AlwaysClusteringEnabledWhenZoomLevelLess > jsonReceive.Zoomlevel;

            JsonMarkersReply reply;

            jsonReceive.Viewport.ValidateLatLon(); // Validate google map viewport input (is always valid)
            jsonReceive.Viewport.Normalize();

            // Get all points from memory
            IPoints points = MemoryDatabase.GetPoints();
            if (jsonReceive.TypeFilter.Count > 0)
            {
                // Filter data by typeFilter value
                // Make new obj, don't overwrite obj data
                points = new Points
                              {
                                  Data = points.Data
                                  .Where(p => jsonReceive.TypeFilter.Contains(p.T) == false)
                                  .ToList()
                              };
            }

            // Create new instance for every ajax request with input all points and json data
            var clusterAlgo = new GridCluster(points, jsonReceive); // create polylines

            // Clustering
            if (clusteringEnabled && jsonReceive.Zoomlevel < jsonReceive.ZoomlevelClusterStop)
            {
                // Calculate data to be displayed
                var clusterPoints = clusterAlgo.GetCluster(new ClusterInfo
                                                               {
                                                                   ZoomLevel = jsonReceive.Zoomlevel
                                                               });

                // Prepare data to the client
                reply = new JsonMarkersReply
                            {
                                Markers = DataConvert(clusterPoints),
                                Rid = sendid,
                                Polylines = clusterAlgo.Lines
                            };

                // Return client data
                return reply;
            }

            // If we are here then there are no clustering
            // The number of items returned is restricted to avoid json data overflow
            IPoints filteredDataset = ClusterAlgorithmBase.FilterDataset(points, jsonReceive.Viewport);
            IPoints filteredDatasetMaxPoints = new Points
                                                   {
                                                       Data = filteredDataset.Data
                                                       .Take(AlgoConfig.MaxMarkersReturned)
                                                       .ToList()
                                                   };

            reply = new JsonMarkersReply
                        {
                            Markers = DataConvert(filteredDatasetMaxPoints),
                            Rid = sendid,
                            Polylines = clusterAlgo.Lines,
                            Mia = filteredDataset.Count - filteredDatasetMaxPoints.Count,
                        };
            return reply;
        }

        // Get
        public JsonMarkersReply GetMarkers(string s)
        {
            var invalid = new JsonMarkersReply { Ok = "0" };

            if (string.IsNullOrWhiteSpace(s)) return invalid;

            var arr = s.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            if (arr.Length != 10) return invalid;

            try
            {
                var i = 0;
                var nelat = arr[i++].Replace("_", ".").ToDouble();
                var nelon = arr[i++].Replace("_", ".").ToDouble();
                var swlat = arr[i++].Replace("_", ".").ToDouble();
                var swlon = arr[i++].Replace("_", ".").ToDouble();
                var zoomlevel = int.Parse(arr[i++]);
                var gridx = int.Parse(arr[i++]);
                var gridy = int.Parse(arr[i++]);
                var zoomlevelClusterStop = int.Parse(arr[i++]);
                var filter = arr[i++];
                var sendid = int.Parse(arr[i++]);
                return Markers(nelat, nelon, swlat, swlon, zoomlevel, gridx, gridy, zoomlevelClusterStop, filter, sendid);
            }
            catch (Exception ex)
            {
                // suppress
            }

            return invalid;
        }

        // Post
        public JsonMarkerInfoReply MarkerInfo(string id, string type, int sendid)
        {
            var reply = new JsonMarkerInfoReply { Id = id, Type = type, Rid = sendid };
            reply.BuildContent();
            return reply;
        }

        // Get
        public JsonMarkerInfoReply GetMarkerInfo(string s)
        {
            var invalid = new JsonMarkerInfoReply { Ok = "0" };

            if (string.IsNullOrWhiteSpace(s)) return invalid;

            var arr = s.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            if (arr.Length != 3) return invalid;

            try
            {
                var i = 0;
                var id = arr[i++];
                var type = arr[i++];
                var sendid = int.Parse(arr[i++]);
                return MarkerInfo(id,type,sendid);
            }
            catch (Exception ex)
            {
                // suppress
            }

            return invalid;
        }


        public JsonInfoReply Info()
        {
            var reply = new JsonInfoReply
            {
                DbSize = MemoryDatabase.GetPoints().Count,
                Points = DataConvert(new Points
                                         {
                                             Data = MemoryDatabase.GetPoints().Data.Take(3).ToList()
                                         })
            };
            return reply;
        }


        // Preparing for K nearest neighbor
        // example of usage
        // /AreaGMC/gmc.svc/GetKnn/10_5;10_20;3
        public JsonKnnReply Knn(string s)
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
            var origin = new SingleDetectLibrary.Code.Data.P { X = x, Y = y };
            var duration = algo.UpdateKnn(origin, k);

            return new JsonKnnReply
            {
                Data = string.Format("{0}; {1}; {2}; msec: {3}", x.DoubleToString(), y.DoubleToString(), k, duration),
                Nns = algo.Knn.NNs.Select(p => p as PDist).ToList(),
            };
        }


        /// <summary>
        /// Solve serializing to Json issue, use replace or use your own P type as you like 
        /// </summary>
        /// <param name="ps"></param>
        /// <returns></returns>
        protected static IList<P> DataConvert(IPoints ps)
        {
            return ps.Data.Select(p => p as P).ToList();
        }

    }

}