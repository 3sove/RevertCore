using System.Text.RegularExpressions;

namespace Revert.Core.Search
{
    public class AlphaNumeric : WildCardToken
    {
        // An AlphaNumeric is a string of letters(A-Z) (case-INsensitive), and digits(0-9)
        public Regex rx = new Regex("^[A-Z0-9]+", RegexOptions.IgnoreCase);

        public static readonly AlphaNumeric Parser = new AlphaNumeric();

        private AlphaNumeric() : base()
        {
        }

        public AlphaNumeric(string token, int begin, int end, int wildcard) : base(token, begin, end, "alphaNumericToken", wildcard)
        {
        }
    }
}
