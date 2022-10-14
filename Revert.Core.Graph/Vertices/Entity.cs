using MongoDB.Bson.Serialization.Attributes;
using Revert.Core.Extensions;
using Revert.Core.Graph.MetaData;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Revert.Core.Graph.Vertices
{
    [DataContract]
    [DebuggerDisplay("{GetEntityDebuggerDisplay(),nq}")]
    public class Entity : Vertex
    {
        public Entity()
        {
        }

        public virtual string GetEntityDebuggerDisplay()
        {
            return $"{Name} -- {Features.SummarizeDataPoints()}";
        }

        [DataMember]
        public float Weight { get; set; } = 1f;

        private float manualNeighborhoodStrength = 0f;
        [DataMember]
        public float NeighborhoodStrength
        {
            get
            {
                if (manualNeighborhoodStrength > float.Epsilon) return manualNeighborhoodStrength;

                float neighborhoodStrength = (float)Math.Pow(CliqueIds.Count.OrIfSmaller(5), 2) / (float)Math.Pow(5, 2);
                neighborhoodStrength += (float)Math.Pow(Edges.Count.OrIfSmaller(2), 2) / (float)Math.Pow(2, 2);
                return neighborhoodStrength;
            }
            set { manualNeighborhoodStrength = value; }
        }
        
        [IgnoreDataMember]
        public string EntityType
        {
            get { return Features.TextData.FirstOrDefault(item => item.Key == "Type")?.Value ?? string.Empty; }
            set { Features.TextData.Add("Type", value); }
        }
        
        public Entity(string name, string type, bool resolvable = true)
        {
            Name = name;
            Features.TextData.AddIgnoreEmpty("Name", name, resolvable);
            Features.EntityType = type;
            Features.TextData.AddIgnoreEmpty("Type", Features.EntityType);
        }

        public Entity(string name, string type, Features features)
        {
            Name = name;
            Features = features;
            Features.EntityType = type;
            Features.TextData.AddIgnoreEmpty("Type", type);
        }

        public void CreateEdge(Entity secondEntity, string details)
        {
            lock (Edges)
            {
                Edges.Add(new Edges.Edge(secondEntity.Id) { Details = details });
                secondEntity.Edges.Add(new Edges.Edge(Id) { Details = details });
            }
        }

        public void CreateEdge(Entity secondEntity)
        {
            lock (Edges)
            {
                Edges.Add(new Edges.Edge(secondEntity.Id) { Details = secondEntity.EntityType });
                secondEntity.Edges.Add(new Edges.Edge(Id) { Details = secondEntity.EntityType });
            }
        }

        [IgnoreDataMember]
        [BsonIgnore]
        public bool IncludeInSearchResults => Features.IncludeInRollup;

        private string toolTip = string.Empty;

        [IgnoreDataMember]
        [BsonIgnore]
        public string ToolTip
        {
            get
            {
                if (toolTip != string.Empty) return toolTip;

                var toolTipBuilder = new StringBuilder();
                if (!string.IsNullOrWhiteSpace(Name)) toolTipBuilder.AppendLine(Name);
                toolTipBuilder.AppendLine(Features.SummarizeDataPoints());
                return toolTipBuilder.ToString();
            }
            set { toolTip = value; }
        }
    }
}
