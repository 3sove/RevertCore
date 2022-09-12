namespace Revert.Core.Search
{
    public class Comma : LexicalTokenizer
    {
        public const char ELEMENT = ',';

        public static readonly Comma Parser = new Comma();

        private Comma() : base() { }

        public Comma(string token, int begin, int end) : base(token, begin, end, "commaToken") { }

        public override LexicalTokenizer TryParse(string inputStr, int position)
        {
            // if inputStr is a comma, return an instance of Comma(), otherwise, null
            return inputStr[position] == ELEMENT ? new Comma(ELEMENT.ToString(), position, position) : null;
        }
    }
}
