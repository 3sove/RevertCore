using System.Collections.Generic;

namespace Revert.Core.Indexing
{
    public class LongComparer : IComparer<long>
    {
        public static LongComparer Instance { get; } = new LongComparer();
        public int Compare(long x, long y)
        {
            if (x == y) return 0;
            if (x < y) return -1;
            return 1;
        }
    }
}
