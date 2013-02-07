using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Kunukn.GooglemapsClustering.Clustering.Data;

namespace Kunukn.GooglemapsClustering.Clustering.Utility
{   
    /// <summary>
    /// Author: Kunuk Nykjaer
    /// </summary>
    public class Dataset
    {        
        // Database simulation
        public static List<P> LoadDatasetFromDatabase(string websitepath, LoadType loadType)
        {            
            switch (loadType)
            {
                case LoadType.Serialized: 
                    throw new NotSupportedException(MethodBase.GetCurrentMethod().ToString());
                case LoadType.Csv: 
                    return LoadDatasetFromDatabaseCsv(websitepath);                    
                default:
                    throw new ApplicationException("LoadDatasetFromDatabase unknown loadtype");
            }            
        }         

         private  static List<P> LoadDatasetFromDatabaseCsv(string websitepath)
         {
             var filepath = websitepath;
             var fi = new FileInfo(websitepath);
             if (!fi.Exists)
             {
                 throw new ApplicationException("File does not exists: " + fi.FullName);
             }                 
       
             var list = FileUtil.ReadFile(filepath);
             var dataset = new List<P>();

             foreach (var s in list)
             {
                 var arr = s.Split(new []{";"}, StringSplitOptions.RemoveEmptyEntries);
                 if (arr.Length == 4)
                 {
                     double x = PBase.ToValue(arr[0]).Value;
                     double y = PBase.ToValue(arr[1]).Value;
                     var i = arr[2];
                     var t = arr[3];

                     dataset.Add(new P(x, y) { I = i, T = t });
                 }
             }

             foreach (var p in dataset)
             {
                 p.Lon = p.Lon.NormalizeLongitude();
                 p.Lat = p.Lat.NormalizeLatitude();
             }

             return dataset;
         }
    }
}