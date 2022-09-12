using System;
using System.Collections.Generic;
using System.Text;

namespace Revert.Core.Common.Types.Tries
{
    public class LongArrayComparer : IEqualityComparer<long[]>
    {
        public bool Equals(long[] x, long[] y)
        {
            if (x == null || y == null) return false;

            if (x.Length != y.Length) return false;
            for (long i = 0; i < x.Length; i++)
                if (!x[i].Equals(y[i])) return false;
            return true;
        }

        public int GetHashCode(long[] obj)
        {
            //32 bit FNV_prime = 224 + 28 + 0x93 = 16777619
            int hashingPrime = 16777619;

            int hash = 0;
            for (int i = 0; i < obj.Length; i++)
            {
                hash *= hashingPrime;
                hash ^= obj[i].GetHashCode();
            }
            return hash;
        }
    }
}
