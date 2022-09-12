using System;
using System.Collections.Generic;
using System.Numerics;

namespace Revert.Core.Indexing
{
    public class ByteArrayKeyComparer : IComparer<byte[]>
    {
        public int Compare(byte[] x, byte[] y)
        {
            if (x == null) return y == null ? 0 : -1;
            if (y == null) return 1;
            if (x.Length != y.Length)
            {
                if (x.Length > y.Length) return 1;
                if (x.Length < y.Length) return -1;
            }

            BigInteger bigX = new BigInteger(x);
            BigInteger bigY = new BigInteger(y);
            return BigInteger.Compare(bigX, bigY);
        }
    }
}
