using System;
using System.Collections.Generic;
using Revert.Core.Text.Tokenization;

namespace Revert.Core.Graph.MetaData
{
    public class OrdinalIgnoreCaseTokenComparer : IEqualityComparer<Token>
    {
        public bool Equals(Token x, Token y)
        {
            return string.Equals(x.Value, y.Value, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(Token obj)
        {
            return obj.Value.ToLowerInvariant().GetHashCode();
        }
    }
}