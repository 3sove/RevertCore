namespace Revert.Core.Search
{
    public class Wildcard : LexicalTokenizer
    {
        public const char ELEMENT = '*';

        public static readonly Wildcard Parser = new Wildcard();

        private Wildcard() : base() { }

        public Wildcard(string token, int begin, int end) : base(token, begin, end, "wildcardToken") { }

        public override LexicalTokenizer TryParse(string inputStr, int position)
        {
            // if inputStr is a wildcard (*), return an instance of Wildcard(), otherwise null
            return inputStr[position] == ELEMENT ? new Wildcard(ELEMENT.ToString(), position, position) : null;
        }
    }
}
