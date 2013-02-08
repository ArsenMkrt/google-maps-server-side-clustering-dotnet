using System;
using System.Collections.Generic;

namespace Kunukn.GooglemapsClustering.Clustering.Contract
{
    public interface IPoints
    {
        int Count { get ; } 
        List<IP> Data { get; set; }
        IP this[int i] { get; set; }
        void Add(IP p);
    }
}
