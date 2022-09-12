namespace Revert.Core.Search
{
    public class RightParenthesis : LexicalTokenizer
    {
        public const char ELEMENT = ')';

        public static readonly RightParenthesis Parser = new RightParenthesis();

        private RightParenthesis() : base() { }

        public RightParenthesis(string token, int begin, int end) : base(token, begin, end, "parenToken") { }

        public override LexicalTokenizer TryParse(string inputStr, int position)
        {
            // if inputStr is a right paren, return an instance of RightParen(), otherwise, null
            return inputStr[position] == ELEMENT ? new RightParenthesis(ELEMENT.ToString(), position, position) : null;
        }
    }
}
