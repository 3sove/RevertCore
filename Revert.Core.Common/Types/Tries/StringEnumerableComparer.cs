using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Revert.Core.Common.Types.Tries
{
    public class StringEnumerableComparer : IEqualityComparer<IEnumerable<string>>
    {
        public bool Equals(IEnumerable<string> x, IEnumerable<string> y)
        {
            if (x == null || y == null) return false;

            if (x.Count() != y.Count()) return false;
            for (int i = 0; i < x.Count(); i++)
                if (!x.ElementAt(i).Equals(y.ElementAt(i))) return false;
            return true;
        }

        public int GetHashCode(IEnumerable<string> obj)
        {
            //32 bit FNV_prime = 224 + 28 + 0x93 = 16777619
            int hashingPrime = 16777619;

            int hash = 0;
            for (int i = 0; i < obj.Count(); i++)
            {
                hash *= hashingPrime;
                hash ^= obj.ElementAt(i).GetHashCode();
            }
            return hash;
        }
    }
}
