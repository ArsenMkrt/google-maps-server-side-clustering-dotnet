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
        public const int  OuterGridExtend = 1; // Standard value is 1. EDIT to extend to widen or shorten gridview for outside view, must be min 0

        // merge cluster points
        public const bool DoUpdateAllCentroidsToNearestContainingPoint = false; // move centroid point to nearest existing point?
        public const bool DoMergeGridIfCentroidsAreCloseToEachOther = true; // merge clusterpoints if close to each other?
        public const int MergeWithin = 3; // if neighbor cluster is within 1/n dist then merge, heuristic, higher value gives less merging
    }
}
