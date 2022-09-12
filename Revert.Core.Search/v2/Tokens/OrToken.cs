using System;
using System.Collections.Generic;
using System.Linq;

namespace Revert.Core.Search.v2.Tokens
{
    public class OrToken : SearchToken<OrToken>
    {
        protected override IEnumerable<string> MatchStrings => new[] { "or" };

        public override IEnumerable<TValue> Evaluate<TKey, TValue>(ISearchable<TKey, TValue> searchable)
        {
            if (!Children.Any()) return null;
            var results = new List<TValue>();
            foreach (var child in Children)
            {
                var childResults = child.Evaluate(searchable);
                if (childResults != null) results.AddRange(childResults);
            }
            return results.Distinct().ToList();
        }
    }
}