using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using MongoDB.Bson;
using Revert.Core.Graph.Vertices;
using Revert.Core.Graph.MetaData;

namespace Revert.Core.Graph.Edges
{
    [DataContract]
    public class Clique : IAdjacencyList
    {
        [DataMember]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

        [DataMember]
        public List<ObjectId> VertexIds { get; set; } = new List<ObjectId>();

        private Features features = null;
        [DataMember]
        public Features Features
        {
            get
            {
                if (features == null) features = new Features();
                return features;
            }
            set
            {
                features = value;
            }
        }

        public Clique()
        {
        }

        public Clique(IEnumerable<IVertex> entities)
        {
            VertexIds = entities?.Select(e => e.Id).ToList();
        }
    }
}