using System.Collections.Generic;

namespace Revert.Core.Search.v2.Tokens
{
    public class SpaceToken : SearchToken<SpaceToken>
    {
        protected override IEnumerable<string> MatchStrings => new[] { " " };

        public override bool IsMatch(string value)
        {
            return !string.IsNullOrEmpty(value) && string.IsNullOrWhiteSpace(value);
        }
    }
}