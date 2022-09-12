namespace Revert.Core.Search
{
    public class Colon : LexicalTokenizer
    {
        public const char ELEMENT = ':';

        public static readonly Colon Parser = new Colon();

        private Colon() : base() { }

        public Colon(string token, int begin, int end) : base(token, begin, end, "colonToken") { }

        public override LexicalTokenizer TryParse(string inputStr, int position)
        {
            // if inputStr is a colon, return an instance of Colon(), otherwise, null
            return inputStr[position] == ELEMENT ? new Colon(ELEMENT.ToString(), position, position) : null;
        }
    }
}
