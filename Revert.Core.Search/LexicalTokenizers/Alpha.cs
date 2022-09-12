using System.Text.RegularExpressions;

namespace Revert.Core.Search
{
    public class Alpha : WildCardToken
    {
        // An Alpha is a string of letters (ONLY), A-Z, case-INsensitive
        public Regex rx = new Regex("^[A-Z]+", RegexOptions.IgnoreCase);

        public static readonly Alpha Parser = new Alpha();

        private Alpha() : base()
        {
        }

        public Alpha(string token, int begin, int end, int wildcard) : base(token, begin, end, "alphaToken", wildcard)
        {
        }
    }
}
