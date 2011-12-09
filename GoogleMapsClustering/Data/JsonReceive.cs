
namespace Kunukn.GooglemapsClustering.Data
{
    public class JsonReceive
    {
        // dont trust user input

        private const int ZoomLevelMax = 30;
        private const int GridMax = 20;


        public string AccessToken { get; set; }


        private int _zoomlevel;
        public int Zoomlevel
        {
            get { return _zoomlevel; }
            private set
            {
                _zoomlevel = value;
                if (_zoomlevel < 0) _zoomlevel = 0;
                else if (_zoomlevel > ZoomLevelMax) _zoomlevel = ZoomLevelMax;
            }
        }
                
        private int _zoomlevelClusterStop;
        public int ZoomlevelClusterStop
        {
            get { return _zoomlevelClusterStop; }
            private set
            {
                _zoomlevelClusterStop = value;
                if (_zoomlevelClusterStop < 0) _zoomlevelClusterStop = 0;
                else if (_zoomlevelClusterStop > ZoomLevelMax) _zoomlevelClusterStop = ZoomLevelMax;
            }
        }

        private int _gridx;
        public int Gridx
        {
            get { return _gridx; }
            private set {                 
                _gridx = value;
                if (_gridx <= 0) _gridx = 1;
                else if (_gridx > GridMax) _gridx = GridMax;
            }
        }
        private int _gridy;
        public int Gridy
        {
            get { return _gridy; }
            private set
            {
                _gridy = value;
                if (_gridy <= 0) _gridy = 1;
                else if (_gridy > GridMax) _gridy = GridMax;
            }
        }
        
                
        public int Sendid { get; private set; }
        public Boundary Viewport { get; private set; }
        
        public JsonReceive(string accessToken, double nelat, double nelon, double swlat, double swlon, int zoomlevel, int gridx, int gridy, int zoomlevelClusterStop, int sendid)
        {
            AccessToken = accessToken;
            Zoomlevel = zoomlevel;
            ZoomlevelClusterStop = zoomlevelClusterStop;
            Gridx = gridx;
            Gridy = gridy;
            Sendid = sendid;
            Viewport = new Boundary { Minx = swlon, Maxx =  nelon, Miny = swlat, Maxy = nelat };                        
        }        
    }
}
