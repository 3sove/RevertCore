using System.Collections.Generic;
using MongoDB.Bson;
using Revert.Core.IO;

namespace Revert.Core.Graph.Edges
{
    public interface IAdjacencyList : IMongoRecord
    {
        List<ObjectId> VertexIds { get; set; }
    }
}
