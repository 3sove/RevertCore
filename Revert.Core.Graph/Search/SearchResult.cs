using System.Runtime.Serialization;
using Revert.Core.Graph.MetaData;

namespace Revert.Core.Graph.Search
{
    [DataContract]
    public class SearchResult
    {
        [DataMember]
        public ulong Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public Features Features { get; set; }

        public override string ToString()
        {
            return Name + Features?.SummarizeDataPoints();
        }
    }
}