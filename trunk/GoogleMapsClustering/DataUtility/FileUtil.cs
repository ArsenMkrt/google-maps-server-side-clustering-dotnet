using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Kunukn.GooglemapsClustering.Data;


namespace Kunukn.GooglemapsClustering.DataUtility
{
    public static class FileUtil
    {
        public const string FolderPath = @"c:\temp\";

        /// <summary>        
        /// folder path is created if not exists
        /// </summary>
        public static bool WriteFile(List<string> data, FileInfo fileInfo)
        {
            var sb = new StringBuilder();
            int i = 0;
            int len = data.Count;
            foreach (var line in data)
            {
                sb.Append(line);
                i++;
                if (i < len)
                    sb.Append(Environment.NewLine);
            }
            return WriteFile(sb.ToString(), fileInfo);
        }
        public static bool WriteFile(string data, FileInfo fileInfo)
        {
            bool success = false;
            try
            {
                if (fileInfo == null)
                    return false;

                //if (fileInfo.Exists)
                //    fileInfo.Delete();

                if (!fileInfo.Directory.Exists)
                    Directory.CreateDirectory(fileInfo.Directory.ToString());

                using (StreamWriter streamWriter = fileInfo.CreateText())
                {
                    streamWriter.Write(data);
                    success = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace + "\nPress a key ... ");
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
                FileInfo fi = new FileInfo(filepath);
                if (!fi.Directory.Exists)
                    Directory.CreateDirectory(fi.Directory.ToString());
                success = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace + "\nPress a key ... ");
                Console.ReadKey();
            }
            return success;
        }

        // save points from mem to file
        public const string DatasetSerializeName = "datasetGridcluster.ser";
        public static void SaveDataSetToFile(List<P> dataset, string filename = null)
        {
            if(filename==null)
                SaveDataSetToFile(dataset, DatasetSerializeName);

            var objectToSerialize = new DatasetToSerialize { Dataset = dataset };
            new Serializer().SerializeObject(FolderPath + filename, objectToSerialize);
        }
       
        // load points from file to mem        
        public static List<P> LoadDataSetFromFile(FileInfo filepath = null)
        {
            if(filepath==null)
                return LoadDataSetFromFile(new FileInfo(FolderPath + DatasetSerializeName));

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
                using (var reader = new StreamReader(path, System.Text.Encoding.UTF8, true))
                {
                    string line = reader.ReadLine();
                    while (line != null)
                    {
                        list.Add(line);
                        line = reader.ReadLine();
                    }
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace);
                throw ex;
            }

            return list;
        }

    }
}
