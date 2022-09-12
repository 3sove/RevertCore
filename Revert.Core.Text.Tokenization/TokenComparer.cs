using System.Collections.Generic;

namespace Revert.Core.Text.Tokenization
{
    public class TokenComparer : IEqualityComparer<Token>
    {
        public static TokenComparer Instance { get; } = new TokenComparer();
        
        public bool Equals(Token x, Token y)
        {
            if (x == null || y == null) return false;
            return x.Id == y.Id && x.Value == y.Value;
        }

        public int GetHashCode(Token obj)
        {
            return obj.Id.GetHashCode() ^ obj.Value.GetHashCode();
        }
    }
}