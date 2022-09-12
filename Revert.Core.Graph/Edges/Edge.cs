using System.Runtime.Serialization;
using MongoDB.Bson;
using Revert.Core.Common;
using ProtoBuf;

namespace Revert.Core.Graph.Edges
{
    [DataContract]
    //[ProtoContract(ImplicitFields = ImplicitFields.None)]
    public class Edge : IEdge
    {
        public Edge()
        {
        }

        public Edge(ObjectId vertexId)
        {
            VertexId = vertexId;
        }

        public Edge(ObjectId vertexId, float weight)
        {
            VertexId = vertexId;
            Weight = weight;
        }

        public void UpdatePointers(ObjectId originalId, ObjectId newId)
        {
            if (id == originalId) id = newId;
            if (Id == originalId) Id = newId;
        }

        private ObjectId id = ObjectId.Empty;
        [DataMember]
        //[ProtoMember((int) ProtobufIds.Id)]
        public ObjectId Id
        {
            get => id == ObjectId.Empty ? id = ObjectId.GenerateNewId() : id;
            set => id  = value;
        }

        [DataMember]
        public ObjectId VertexId
        {
            get;
            set;
        }


        [DataMember]
        //[ProtoMember((int)ProtobufIds.EdgeWeight)]
        public float Weight { get; set; }

        [DataMember]
        //[ProtoMember((int)ProtobufIds.EdgeDetails)]
        public string Details { get; set; }

        public override int GetHashCode()
        {
            return (Id.GetHashCode() ^ Id.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Edge objEdge)) return false;
            return objEdge.Id == Id && objEdge.Id == Id;
        }
    }
}
