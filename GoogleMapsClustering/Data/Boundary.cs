using System;


namespace Kunukn.GooglemapsClustering.Data
{
    public class Boundary : Point2
    {        
        public Boundary()
        {
        }

        public Boundary(Boundary b)
        {
            Minx = b.Minx;
            Miny = b.Miny;
            Maxx = b.Maxx;
            Maxy = b.Maxy;
        }


        /// <summary>
        /// Normalize lat and lon values to their boundary values
        /// </summary>
        public void Normalize()
        {
            Minx = Minx.NormalizeLongitude();
            Maxx = Maxx.NormalizeLongitude();
            Miny = Miny.NormalizeLatitude();
            Maxy = Maxy.NormalizeLatitude();
        }


        public void ValidateLatLon()
        {
            if ((Minx > LatLonInfo.MaxLonValue || Minx < LatLonInfo.MinLonValue)
                || (Maxx > LatLonInfo.MaxLonValue || Maxx < LatLonInfo.MinLonValue)
                || (Miny > LatLonInfo.MaxLatValue || Miny < LatLonInfo.MinLatValue)
                || (Maxy > LatLonInfo.MaxLatValue || Maxy < LatLonInfo.MinLatValue)
                )
                throw new ApplicationException("input Boundary.ValidateLatLon() error " + this);
        }

        // distance lon
        public double AbsX
        {
            get
            {
                return DataExtensions.AbsLon(Minx, Maxx);
            }
        }
        // distance lat
        public double AbsY
        {
            get
            {
                return DataExtensions.AbsLat(Miny, Maxy);
            }
        }

        public override string ToString()
        {
            return base.ToString();
        }

    }
}
