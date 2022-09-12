using System;
using System.Collections.Generic;
using System.Linq;

namespace Revert.Core.Search.v2.Tokens
{
    public class AndToken : SearchToken<AndToken>
    {
        protected override IEnumerable<string> MatchStrings => new[] { "and", "." };

        public override IEnumerable<TValue> Evaluate<TKey, TValue>(ISearchable<TKey, TValue> searchable)
        {
            if (!Children.Any()) return null;
            if (Children.Count == 1)
                throw new Exception("And token was found with 1 child when more than 1 was expected.");

            var results = new List<TValue>();
            foreach (var child in Children)
            {
                var matches = searchable.GetMatches(searchable.GetKeyFromString(child.Value));
                if (matches == null)
                {
                    results.Clear();
                    return results;
                }

                if (results.Count == 0) results.AddRange(matches);
                else results = results.Intersect(matches).ToList();
            }
            return results;
        }
    }
}