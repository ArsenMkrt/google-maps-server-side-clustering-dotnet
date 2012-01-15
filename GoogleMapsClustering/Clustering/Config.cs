namespace Kunukn.GooglemapsClustering.Clustering
{
    /// <summary>
    /// Author: Kunuk Nykjaer
    /// </summary>
    public static class  Config
    {        
        // debug related
        public const bool DoShowGridLinesInGoogleMap = true; // generate draw grid lines info to google map
        
        // how much data that is send to client
        // EDIT to extend to widen or shorten gridview for outside view, must be minimum 0
        // default value is 1 which returns same data as illustrated in the picture from my blog
        // (see googlemaps-clustering-viewport_ver1.png inside the Docements/Design folder)
        public const int  OuterGridExtend = 1; //1

        // merge cluster points
        public const bool DoUpdateAllCentroidsToNearestContainingPoint = false; // move centroid point to nearest existing point?
        public const bool DoMergeGridIfCentroidsAreCloseToEachOther = true; // merge clusterpoints if close to each other?
        public const double MergeWithin = 3; // if neighbor cluster is within 1/n dist then merge, heuristic, higher value gives less merging

        // cluster decision
        public const int MinClusterSize = 2; // only cluster if minimum this number of points

        public const int MaxMarkersReturned = 500; //if clustering is disabled, restrict number of markers returned
        // always cluster if equal or below this zoom level
        // to disable this effect set the value to -1
        public const int AlwaysClusteringEnabledWhenZoomLevelLess = 8; // same value as in js-file: if (gmcKN.map.getZoom() < 7) {$('#gmcKN_Clustering_span').hide();
    }
}
