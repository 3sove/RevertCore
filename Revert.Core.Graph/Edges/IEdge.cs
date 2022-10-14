using Revert.Core.IO;

namespace Revert.Core.Graph.Edges
{
    public interface IEdge : IMongoRecord
    {
        float Weight { get; set; }
    }
}