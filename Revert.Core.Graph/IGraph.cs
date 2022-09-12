using MongoDB.Bson;
using Revert.Core.Graph.Edges;
using Revert.Core.Graph.Vertices;
using Revert.Core.IO.Stores;

namespace Revert.Core.Graph
{
    public interface IGraph<TVertex> where TVertex : IVertex
    {
        IKeyValueStore<ObjectId, TVertex> GetVertices();
        IKeyValueStore<ObjectId, Clique> GetCliques();
        TVertex GetVertex(ObjectId id);

    }
}