﻿using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Activation;
using Kunukn.GooglemapsClustering.Clustering.Algorithm;
using Kunukn.GooglemapsClustering.Clustering.Data;
using Kunukn.GooglemapsClustering.Clustering.Data.Json;

namespace Kunukn.GooglemapsClustering.Clustering.WebService
{
    [AspNetCompatibilityRequirements(RequirementsMode
        = AspNetCompatibilityRequirementsMode.Allowed)]
    public class AjaxService : IAjaxService
    {
        protected static JsonReplyBase NotValidReply(int sendid)
        {            
            var jsonReply = new JsonReplyBase { ReplyId = sendid, Success = "0" };            
            return jsonReply;
        }

        public JsonGetMarkersReply GetMarkers(double nelat, double nelon, double swlat, double swlon, int zoomlevel, int gridx, int gridy, int zoomlevelClusterStop, string filter, int sendid)
        {                                    
            var jsonReceive = new JsonGetMarkersReceive(nelat, nelon, swlat, swlon, zoomlevel, gridx, gridy, zoomlevelClusterStop, filter, sendid);
            
            var clusteringEnabled = jsonReceive.IsClusteringEnabled || AlgoConfig.AlwaysClusteringEnabledWhenZoomLevelLess > jsonReceive.Zoomlevel;
            
            JsonGetMarkersReply reply;
            
            jsonReceive.Viewport.ValidateLatLon(); // Validate google map viewport input
            jsonReceive.Viewport.Normalize();

            // Get all points from memory, application cache
            var allpoints = MemoryDatabase.Points;
                                        
            var dataset = allpoints; // unfiltered at the moment
            if (jsonReceive.TypeFilter.Count > 0)
            {
                // Filter data by typeFilter value
                dataset = allpoints.Where(p => jsonReceive.TypeFilter.Contains(p.T) == false).ToList();
            }

            // Clustering
            if (clusteringEnabled && jsonReceive.Zoomlevel < jsonReceive.ZoomlevelClusterStop)
            {
                // Create new instance for every ajax request with input all points and json data
                var clusterAlgo = new GridCluster(dataset, jsonReceive);

                // Calculate data to be displayed
                var clusterPoints = clusterAlgo.GetCluster(new ClusterInfo { ZoomLevel = jsonReceive.Zoomlevel });

                // Prepare data to the client
                reply = new JsonGetMarkersReply { Points = clusterPoints, ReplyId = sendid, Polylines = clusterAlgo.Lines };                

                // Return client data
                return reply;
            }

            // If we are here then there are no clustering
            // The number of items returned is restricted to avoid json data overflow
            List<P> filteredDataset = ClusterAlgorithmBase.FilterDataset(dataset, jsonReceive.Viewport);
            List<P> filteredDatasetMaxPoints = filteredDataset.Take(AlgoConfig.MaxMarkersReturned).ToList();

            reply = new JsonGetMarkersReply { Points = filteredDatasetMaxPoints, ReplyId = sendid, Success = "1" };            
            return reply;
        }

        public JsonMarkerInfoReply GetMarkerDetail(string id, string type, int sendid)
        {                                                
            var reply = new JsonMarkerInfoReply { Id = id, Type = type, ReplyId = sendid };                        
            reply.BuildContent();
            reply.Success = "1";
            return reply;
        }
        
        public string Test()
        {
            return "test";
        }
    }

}