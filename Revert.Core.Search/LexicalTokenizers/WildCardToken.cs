namespace Revert.Core.Search
{
    public class WildCardToken : LexicalTokenizer
    {
        public int WildCard;
        public WildCardToken() : base()
        {
            WildCard = 0;
        }

        public WildCardToken(string token, int begin, int end, string label, int wildcard) : base(token, begin, end, label)
        {
            WildCard = wildcard;
        }

        public static bool ValidWild(string token, int begin, int end, int wildcard)
        {
            return !(wildcard > 2 || (wildcard == 2 && (token[begin] != '*' || token[end] != '*')));
        }
    }
}
