using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using MongoDB.Bson;
using Revert.Core.Common;
using Revert.Core.Graph.Vertices;
using ProtoBuf;
using Revert.Core.Graph.MetaData;

namespace Revert.Core.Graph.Edges
{
    [DataContract]
    //[ProtoContract(ImplicitFields = ImplicitFields.None)]
    public class Clique : IAdjacencyList
    {
        [DataMember]
        //[ProtoMember((int)ProtobufIds.AdjacencyListId)]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

        [DataMember]
        //[ProtoMember((int)ProtobufIds.AdjacencyListVertexIds)]
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