using System.Collections.Generic;
using MongoDB.Bson;
using Revert.Core.Graph.Edges;
using Revert.Core.Graph.MetaData;
using Revert.Core.IO;

namespace Revert.Core.Graph.Vertices
{
    public interface IVertex : IMongoRecord
    {
        //ulong Id { get; set; }
        Features Features { get; set; }
        string Name { get; set; }
        HashSet<Edge> Edges { get; set; }
        HashSet<ObjectId> CliqueIds { get; set; }

        float BetweennessCentrality { get; set; }

        void Merge(IVertex vertex);

        void UpdatePointers(ObjectId originalId, ObjectId newId);
        void CreateEdge(IVertex targetVertex);
        void CreateEdge(IVertex targetVertex, object edgeDetails);
    }
}
