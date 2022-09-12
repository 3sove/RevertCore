using System.Collections.Generic;
using MongoDB.Bson;
using Revert.Core.Common;
using ProtoBuf;
using Revert.Core.IO;

namespace Revert.Core.Graph.Edges
{
    //[ProtoContract(ImplicitFields = ImplicitFields.None)]
    [ProtoInclude((int)ProtobufIds.IncludeAdjacencyList, typeof(Clique))]
    public interface IAdjacencyList : IMongoRecord
    {
        List<ObjectId> VertexIds { get; set; }
    }
}
