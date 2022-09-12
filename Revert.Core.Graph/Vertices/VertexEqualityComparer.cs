using System.Collections.Generic;

namespace Revert.Core.Graph.Vertices
{
    public class VertexEqualityComparer : IEqualityComparer<IVertex>
    {
        public bool Equals(IVertex x, IVertex y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;
            return x.Id == y.Id;
        }

        public int GetHashCode(IVertex obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
