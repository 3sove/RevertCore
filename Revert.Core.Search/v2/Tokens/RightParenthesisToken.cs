using System;
using System.Collections.Generic;

namespace Revert.Core.Search.v2.Tokens
{
    public class RightParenthesisToken : SearchToken<RightParenthesisToken>
    {
        public override List<Type> ChildTokenTypes => null;

        protected override IEnumerable<string> MatchStrings => new[] { ")", "]", "}" };
    }
}