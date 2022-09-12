using System;
using System.Collections.Generic;
using System.Linq;

namespace Revert.Core.Search.v2.Tokens
{
    public class LeftParenthesisToken : SearchToken<LeftParenthesisToken>
    {
        protected override IEnumerable<string> MatchStrings => new[] { "(", "[", "{" };

        public override IEnumerable<TValue> Evaluate<TKey, TValue>(ISearchable<TKey, TValue> searchable)
        {
            if (!Children.Any()) return null;
            if (Children.Count > 1)
                throw new Exception("Token was found with more than 1 child when only 1 was expected.");
            return searchable.GetMatches(searchable.GetKeyFromString(Children[0].Value));
        }
    }
}