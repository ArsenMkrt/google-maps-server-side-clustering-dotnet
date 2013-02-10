using System;
using System.Collections.Generic;

namespace Kunukn.GooglemapsClustering.Clustering.Data.Json
{
    /// <summary>
    /// Parse json receive data
    /// </summary>
    public class JsonGetMarkersReceive
    {
        // Don't trust user input, data range is restricted if user abuse or hacks client side
        private const int ZoomLevelMax = 30;
        private const int GridMax = 20;
                
        public int Sendid { get; private set; }
        public Boundary Viewport { get; private set; }

        private int _zoomlevel;
        public int Zoomlevel
        {
            get
            {
                return _zoomlevel;
            }
            private set
            {
                _zoomlevel = value;
                if (_zoomlevel < 0)
                {
                    _zoomlevel = 0;
                }
                else if (_zoomlevel > ZoomLevelMax)
                {
                    _zoomlevel = ZoomLevelMax;
                }
            }
        }
                
        private int _zoomlevelClusterStop;
        public int ZoomlevelClusterStop
        {
            get
            {
                return _zoomlevelClusterStop;
            }
            private set
            {
                _zoomlevelClusterStop = value;
                if (_zoomlevelClusterStop < 0)
                {
                   _zoomlevelClusterStop = 0;
                }
                else if (_zoomlevelClusterStop > ZoomLevelMax)
                {
                   _zoomlevelClusterStop = ZoomLevelMax;
                }
            }
        }

        private int _gridx;
        public int Gridx
        {
            get
            {
                return _gridx;
            }
            private set {
                _gridx = value;
                if (_gridx <= 0)
                {
                    _gridx = 1;
                }
                else if (_gridx > GridMax)
                {
                    _gridx = GridMax;
                }
            }
        }
        private int _gridy;
        public int Gridy
        {
            get
            {
                return _gridy;
            }
            private set
            {
                _gridy = value;
                if (_gridy <= 0)
                {
                    _gridy = 1;
                }
                else if (_gridy > GridMax)
                {
                    _gridy = GridMax;
                }
            }
        }

        public bool IsClusteringEnabled { get; private set; }
        public bool IsDebugLinesEnabled { get; private set; }
        public HashSet<int> TypeFilter { get; private set; }

        public JsonGetMarkersReceive(double nelat, double nelon, double swlat, double swlon, int zoomlevel, int gridx, int gridy, int zoomlevelClusterStop, string filter, int sendid)
        {            
            Zoomlevel = zoomlevel;
            ZoomlevelClusterStop = zoomlevelClusterStop;
            Gridx = gridx;
            Gridy = gridy;
            Sendid = sendid;

            Viewport = new Boundary { Minx = swlon, Maxx = nelon, Miny = swlat, Maxy = nelat };

            // Parse filter
            int binarySum = 0;
            int.TryParse(filter, out binarySum);
            string binary = Convert.ToString(binarySum, 2);
            binary = Reverse(binary); // more easy to take index of when reversed

            IsClusteringEnabled = binary.Length >= 1 && binary[0] == '1';
            IsDebugLinesEnabled = binary.Length >= 2 && binary[1] == '1';

            var typeFilter = new HashSet<int>();
            var type1 = binary.Length >= 3 && binary[2] == '1';
            var type2 = binary.Length >= 4 && binary[3] == '1';
            var type3 = binary.Length >= 5 && binary[4] == '1';                       
            if (!type1)
            {
                typeFilter.Add(1);
            }
            if (!type2)
            {
                typeFilter.Add(2);
            }
            if (!type3)
            {
                typeFilter.Add(3);
            }
            TypeFilter = typeFilter;
        }

        static string Reverse(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
    }
}
