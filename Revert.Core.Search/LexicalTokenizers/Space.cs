using System.Text.RegularExpressions;

namespace Revert.Core.Search
{
    public class Space : LexicalTokenizer
    {
        // A Space is one or more whitespace characters
        public Regex rx = new Regex(@"^\s+", RegexOptions.None);

        public static readonly Space Parser = new Space();

        private Space() : base() { }

        public Space(string token, int begin, int end) : base(token, begin, end, "spaceToken") { }

        public override LexicalTokenizer TryParse(string inputStr, int position)
        {
            string inputSubStr = inputStr.Substring(position);
            Match match = rx.Match(inputSubStr);

            string matchStr = match.Groups[0].ToString();

            // if inputStr is a space, return an instance of Space(), otherwise null
            return (match.Success) ? new Space(matchStr, position, position + matchStr.Length - 1) : null;
        }
    }
}
