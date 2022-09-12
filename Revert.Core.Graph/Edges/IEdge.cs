using MongoDB.Bson;
using Revert.Core.Common;
using ProtoBuf;
using Revert.Core.IO;

namespace Revert.Core.Graph.Edges
{
    //[ProtoContract(ImplicitFields = ImplicitFields.None)]
    [ProtoInclude((int) ProtobufIds.IncludeEdge, typeof(Edge))]
    public interface IEdge : IMongoRecord
    {
        float Weight { get; set; }
    }
}