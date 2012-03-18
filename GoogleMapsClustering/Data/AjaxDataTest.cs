using System.Runtime.Serialization;

namespace Kunukn.GooglemapsClustering.Data
{
    [DataContract]
    public class AjaxDataTest
    {
        [DataMember]
        public string Value { get; set; }
    }
}
