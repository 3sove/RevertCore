namespace Revert.Core.Search
{
    public class Dot : LexicalTokenizer
    {
        public const char ELEMENT = '.';

        public static readonly Dot Parser = new Dot();

        private Dot() : base() { }

        public Dot(string token, int begin, int end) : base(token, begin, end, "dotToken") { }

        public override LexicalTokenizer TryParse(string inputStr, int position)
        {
            // if inputStr is a period, return an instance of Period(), otherwise null
            return inputStr[position] == ELEMENT ? new Dot(ELEMENT.ToString(), position, position) : null;
        }
    }
}
