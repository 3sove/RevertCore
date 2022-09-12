using System;
using System.Collections.Generic;
using System.Linq;

namespace Revert.Core.Search.v2.Tokens
{
    public class NotToken : SearchToken<NotToken>
    {
        protected override IEnumerable<string> MatchStrings => new[] { "not" };

        public override IEnumerable<TValue> Evaluate<TKey, TValue>(ISearchable<TKey, TValue> searchable)
        {
            if (!Children.Any()) return null;
            if (Children.Count > 1)
                throw new Exception("Not token was found with more than 1 child.  This should be impossible.");
            return searchable.AllValues.Except(searchable.GetMatches(searchable.GetKeyFromString(Children[0].Value))).ToList();
        }
    }
}