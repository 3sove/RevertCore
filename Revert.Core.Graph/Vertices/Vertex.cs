using System.Collections.Generic;
using System.Runtime.Serialization;
using MongoDB.Bson;
using Revert.Core.Graph.Edges;
using Revert.Core.Graph.MetaData;

namespace Revert.Core.Graph.Vertices
{
    [DataContract]
    public class Vertex : IVertex
    {
        [DataMember]
        public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

        [DataMember]
        public Features Features { get; set; } = new Features();

        public float BetweennessCentrality { get; set; } = 1f;

        [DataMember]
        public string Name
        {
            get => !string.IsNullOrWhiteSpace(_name) ? _name : Features.Name;
            set => _name = value;
        }

        private HashSet<Edge> edges;
        [DataMember]
        public HashSet<Edge> Edges
        {
            get => edges ?? (edges = new HashSet<Edge>());
            set => edges = value;
        }

        private HashSet<ObjectId> cliqueIds;
        private string _name;

        [DataMember]
        public HashSet<ObjectId> CliqueIds
        {
            get => cliqueIds ?? (cliqueIds = new HashSet<ObjectId>());
            set => cliqueIds = value;
        }
       
        
        public void Merge(IVertex vertex)
        {
            if (vertex.Id == Id) return;
            Features.Merge(vertex.Features);

            foreach (var edge in vertex.Edges)
                Edges.Add(edge);
            
            foreach (var cliqueId in vertex.CliqueIds)
                CliqueIds.Add(cliqueId);
        }

        public void UpdatePointers(ObjectId originalId, ObjectId newId)
        {
            if (CliqueIds.Contains(originalId))
            {
                CliqueIds.Remove(originalId);
                CliqueIds.Add(newId);
            }

            foreach (var edge in Edges)
                edge.UpdatePointers(originalId, newId);
        }

        public void CreateEdge(IVertex targetVertex)
        {
            CreateEdge(targetVertex, string.Empty);
        }

        public void CreateEdge(IVertex targetVertex, object edgeDetails)
        {
            Edges.Add(new Edge(targetVertex.Id) { Details = edgeDetails.ToString() });
            targetVertex.Edges.Add(new Edge(Id) { Details = edgeDetails.ToString() });
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var entityObj = obj as Entity;
            return Id == entityObj?.Id;
        }
    }
}