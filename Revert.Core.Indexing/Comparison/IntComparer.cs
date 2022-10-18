using System.Collections.Generic;

namespace Revert.Core.Indexing.Comparison
{
    public class IntComparer : IComparer<int>
    {
        public static IntComparer Instance { get; } = new IntComparer();
        public int Compare(int x, int y)
        {
            if (x == y) return 0;
            if (x < y) return -1;
            return 1;
        }
    }
}
