namespace Revert.Core.Search
{
    public class LeftParenthesis : LexicalTokenizer
    {
        public const char ELEMENT = '(';

        public static readonly LeftParenthesis Parser = new LeftParenthesis();

        private LeftParenthesis() : base() { }

        public LeftParenthesis(string token, int begin, int end) : base(token, begin, end, "parenToken") { }

        public override LexicalTokenizer TryParse(string inputStr, int position)
        {
            // if inputStr is a left paren, return an instance of LeftParen(), otherwise, null
            return inputStr[position] == ELEMENT ? new LeftParenthesis(ELEMENT.ToString(), position, position) : null;
        }
    }
}
