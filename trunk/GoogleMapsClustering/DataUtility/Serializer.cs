using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Kunukn.GooglemapsClustering.DataUtility
{
    /// <summary>
    /// Author: Kunuk Nykjaer
    /// </summary>
    public class Serializer
    {
        public void SerializeObject(string filepath, object objectToSerialize)
        {
            try
            {
                FileUtil.CreateFilePath(filepath); // create folder if not exists
                using (Stream stream = File.Open(filepath, FileMode.Create))
                {
                    BinaryFormatter bFormatter = new BinaryFormatter();
                    bFormatter.Serialize(stream, objectToSerialize);
                }
            }
            catch (Exception ex)
            {
                throw ex;
                Console.WriteLine(ex.StackTrace + "\nPress a key ... ");
                Console.ReadKey();
            }
        }
        public object DeSerializeObject(string filepath)
        {
            try
            {
                using (Stream stream = File.Open(filepath, FileMode.Open, FileAccess.Read))
                {
                    BinaryFormatter bFormatter = new BinaryFormatter();
                    var objectToSerialize = bFormatter.Deserialize(stream);
                    return objectToSerialize;
                }
            }
            catch (Exception ex)
            {
                throw ex;
                Console.WriteLine(ex.StackTrace + "\nPress a key ... ");
                Console.ReadKey();
            }
            return null;
        }
    }
}
