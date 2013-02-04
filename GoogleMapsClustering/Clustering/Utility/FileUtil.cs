﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Kunukn.GooglemapsClustering.Clustering.Data;

namespace Kunukn.GooglemapsClustering.Clustering.Utility
{
    /// <summary>
    /// Author: Kunuk Nykjaer
    /// </summary>
    public static class FileUtil
    {
        private static Action<object> WL = Console.WriteLine;

        public const string FolderPath = @"c:\temp\";
        private static readonly Encoding _encodingRead = Encoding.Default; // Encoding.Default  Encoding.UTF8  Encoding.Unicode      
        private static readonly Encoding _encodingWrite = Encoding.Unicode; //Encoding.Default  Encoding.UTF8  Encoding.Unicode

        /// <summary>        
        /// folder path is created if not exists
        /// </summary>
        public static bool WriteFile(List<string> data, FileInfo fileInfo)
        {
            var sb = new StringBuilder();
            var i = 0;
            var len = data.Count;
            foreach (var line in data)
            {
                sb.Append(line);
                i++;
                if (i < len) sb.Append(Environment.NewLine);                    
            }
            return WriteFile(sb.ToString(), fileInfo);
        }
        public static bool WriteFile(string data, FileInfo fileInfo)
        {
            bool success = false;
            try
            {
                if (fileInfo == null) return false;
                                                  
                if (!fileInfo.Directory.Exists)
                {
                    Directory.CreateDirectory(fileInfo.Directory.ToString());
                }
                                    
                using (var streamWriter = fileInfo.CreateText() )
                {
                    streamWriter.Write(data, _encodingWrite);
                    success = true;
                }
            }
            catch (Exception ex)
            {
                WL(ExceptionUtil.GetException(ex));
                WL("\nPress a key ... ");
                Console.ReadKey();
            }

            return success;
        }

        /// <summary>
        /// Creates the folders if not exists for file path
        /// </summary>
        public static bool CreateFilePath(string filepath)
        {
            bool success = false;
            try
            {
                var fi = new FileInfo(filepath);
                if (!fi.Directory.Exists)
                {
                    Directory.CreateDirectory(fi.Directory.ToString());
                }                    
                success = true;
            }
            catch (Exception ex)
            {
                WL(ExceptionUtil.GetException(ex));
                WL("\nPress a key ... ");
                Console.ReadKey();
            }
            return success;
        }

        // Save points from mem to file
        public const string DatasetSerializeName = "datasetGridcluster.ser";
        public static void SaveDataSetToFile(List<P> dataset, string filename = null)
        {
            if(filename==null)
            {
                SaveDataSetToFile(dataset, DatasetSerializeName);
            }
                
            var objectToSerialize = new DatasetToSerialize { Dataset = dataset };
            new Serializer().SerializeObject(FolderPath + filename, objectToSerialize);
        }
       
        // Load points from file to mem        
        public static List<P> LoadDataSetFromFile(FileInfo filepath = null)
        {
            if(filepath==null)
            {
                return LoadDataSetFromFile(new FileInfo(FolderPath + DatasetSerializeName));   
            }
                
            var objectToSerialize = (DatasetToSerialize)
                (new Serializer().DeSerializeObject(filepath.FullName));
            return objectToSerialize.Dataset;
        }
      
        public static List<string> ReadFile(FileInfo fi)
        {
            return ReadFile(fi.FullName);
        }

        public static List<string> ReadFile(string path)
        {
            var list = new List<string>();
            try
            {                
                using (var reader = new StreamReader(path, _encodingRead, true))
                {
                    var line = reader.ReadLine();
                    while (line != null)
                    {
                        list.Add(line);
                        line = reader.ReadLine();
                    }
                }
            }
            catch (Exception ex)
            {
                WL(ExceptionUtil.GetException(ex));
                WL("\nPress a key ... ");
                throw ex;
            }

            return list;
        }       
    }
}
