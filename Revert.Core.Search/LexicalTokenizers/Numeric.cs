using System.Text.RegularExpressions;

namespace Revert.Core.Search
{
    public class Numeric : WildCardToken
    {
        // A Numeric is a string of digits (0-9)
        public Regex rx = new Regex("^[0-9]+", RegexOptions.IgnoreCase);

        public static readonly Numeric Parser = new Numeric();

        private Numeric()
            : base()
        {
        }

        public Numeric(string token, int begin, int end, int wildcard)
            : base(token, begin, end, "numericToken", wildcard)
        {
        }
    }
}
