using System;
using System.Collections.Generic;
using System.Diagnostics;
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

using IKnnAlgorithm = Kunukn.SingleDetectLibrary.Code.Contract.IAlgorithm;
using PDist = Kunukn.SingleDetectLibrary.Code.Data.PDist;

namespace Kunukn.GooglemapsClustering.Clustering.WebService
{
    [AspNetCompatibilityRequirements(RequirementsMode
        = AspNetCompatibilityRequirementsMode.Allowed)]
    public class AjaxService : IAjaxService
    {

        long Sw(Stopwatch sw)
        {
            sw.Stop();
            return sw.ElapsedMilliseconds;
        }

        #region Post

        // Post
        public JsonMarkersReply Markers(double nelat, double nelon, double swlat, double swlon, int zoomlevel, int gridx, int gridy, int zoomlevelClusterStop, string filter, int sendid)
        {
            var sw = new Stopwatch();
            sw.Start();

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
                
                var converted = DataConvert(clusterPoints);
                                
                // Prepare data to the client
                reply = new JsonMarkersReply
                            {
                                Markers = converted,
                                Rid = sendid,
                                Polylines = clusterAlgo.Lines,
                                Msec = Sw(sw),
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
                            Msec = Sw(sw),
                        };            
            return reply;
        }

      

        // Post
        public JsonMarkerInfoReply MarkerInfo(string id, string type, int sendid)
        {
            var sw = new Stopwatch();
            sw.Start();

            var reply = new JsonMarkerInfoReply
                            {
                                Id = id, 
                                Type = type, 
                                Rid = sendid,                                
                            };
            reply.BuildContent();
            reply.Msec = Sw(sw);
            return reply;
        }

        #endregion Post


        #region Get        

        // Get
        public JsonMarkersReply GetMarkers(string s)
        {
            var invalid = new JsonMarkersReply { Ok = "0" };

            if (string.IsNullOrWhiteSpace(s))
            {
                invalid.EMsg = "params is empty";
                return invalid;
            }

            var arr = s.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            if (arr.Length != 10)
            {
                invalid.EMsg = "params length incorrect";
                return invalid; 
            }

            var i = 0;
            try
            {                
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

                // values are validated there
                return Markers(nelat, nelon, swlat, swlon, zoomlevel, gridx, gridy, zoomlevelClusterStop, filter, sendid);
            }
            catch (Exception ex)
            {
                invalid.EMsg = string.Format("Parsing error at param: {0}, {1}",
                    i - 1, ex.Message);
            }

            return invalid;
        }

        // Get
        public JsonMarkerInfoReply GetMarkerInfo(string s)
        {
            var invalid = new JsonMarkerInfoReply { Ok = "0" };

            if (string.IsNullOrWhiteSpace(s))
            {
                invalid.EMsg = "params is empty";
                return invalid;
            }

            var arr = s.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            if (arr.Length != 3) return invalid;

            var i = 0;
            try
            {                
                var id = arr[i++];
                var type = arr[i++];
                var sendid = int.Parse(arr[i++]);

                // values are validated there
                return MarkerInfo(id,type,sendid);
            }
            catch (Exception ex)
            {
                invalid.EMsg = string.Format("Parsing error at param: {0}, {1}", 
                    i - 1, ex.Message);
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
        // /AreaGMC/gmc.svc/Knn/8_5;10_25;3
        // /AreaGMC/gmc.svc/Knn/8_5;10_25;3;1
        public JsonKnnReply Knn(string s)
        {
            var sw = new Stopwatch();
            sw.Start();

            var invalid = new JsonKnnReply {};
            if (string.IsNullOrEmpty(s))
            {
                invalid.EMsg = "param is empty";
                return invalid;
            }

            var arr = s.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            if (arr.Length != 3 && arr.Length != 4)
            {
                invalid.EMsg = string.Format("param length is not valid: {0}",arr.Length);
                return invalid;
            }

            var lat = arr[0].Replace("_", ".");
            var lon = arr[1].Replace("_", ".");
            var neighbors = arr[2];
            var type = -1;
            if (arr.Length == 4) type = arr[3].ToInt();

            double x, y;
            int k;

            var b = double.TryParse(lon, System.Globalization.NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out x);
            b &= double.TryParse(lat, System.Globalization.NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out y);
            b &= int.TryParse(neighbors, out k);

            if (!b)
            {
                invalid.EMsg = "params were not parsed correctly";
                return invalid;
            }

            // knn algo
            var algo = MemoryDatabase.Data as IKnnAlgorithm;
            if (algo == null)
            {
                invalid.EMsg = "algorithm is not available";
                return invalid;
            }

            // Use algo
            var origin = new SingleDetectLibrary.Code.Data.P { X = x, Y = y, Type = type};
            var knnSameTypeOnly = type != -1;
            var duration = algo.UpdateKnn(origin, k, knnSameTypeOnly);

            return new JsonKnnReply
            {
                Data = string.Format("x: {0}; y: {1}; k: {2}; sameTypeOnly: {3}, algo msec: {4}", 
                    x.DoubleToString(), y.DoubleToString(), k, knnSameTypeOnly, duration),
                Nns = algo.Knn.NNs.Select(p => p as PDist).ToList(), // cannot be interface, thus casting
                Msec = Sw(sw),
            };
        }

        #endregion Get

        /// <summary>
        /// Solve serializing to Json issue (Cannot be interface type)
        /// Use replace or use your own P type as you like 
        /// </summary>
        /// <param name="ps"></param>
        /// <returns></returns>
        protected static IList<P> DataConvert(IPoints ps)
        {
            return ps.Data.Select(p => p as P).ToList();
        }
    }
}