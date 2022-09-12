using System;
using System.Collections.Generic;
using System.Linq;

namespace Revert.Core.Search.v2.Tokens
{
    public class QuoteToken : SearchToken<QuoteToken>
    {
        private List<Type> childTokenTypes;

        public override List<Type> ChildTokenTypes => childTokenTypes ?? (childTokenTypes = new List<Type>
        {
            typeof (WordToken),
            typeof (NotToken),
            typeof (OrToken),
            typeof (AndToken),
            typeof (LeftParenthesisToken),
            typeof (RightParenthesisToken)
        });

        protected override IEnumerable<string> MatchStrings => new[] { "\"", "“", "”", "'" };

        public override IEnumerable<TValue> Evaluate<TKey, TValue>(ISearchable<TKey, TValue> searchable)
        {
            return Children.Any()
                ? searchable.GetMatches(Children.Select(c => searchable.GetKeyFromString(c.Value)).ToArray())
                : null;
        }
    }
}