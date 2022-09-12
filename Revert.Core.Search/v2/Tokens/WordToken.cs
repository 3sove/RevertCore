using System;
using System.Collections.Generic;
using System.Linq;

namespace Revert.Core.Search.v2.Tokens
{
    public class WordToken : SearchToken<WordToken>
    {
        public override List<Type> ChildTokenTypes => null;

        public override bool IsMatch(string value)
        {
            if (StopList.Contains(value.ToLowerInvariant())) return true;
            return !string.IsNullOrWhiteSpace(value);
        }

        public override IEnumerable<TValue> Evaluate<TKey, TValue>(ISearchable<TKey, TValue> searchable)
        {
            if (StopList.Contains(Value.ToLowerInvariant())) return searchable.AllValues.ToList();
            return searchable.GetMatches(searchable.GetKeyFromString(Value));
        }
    }
}