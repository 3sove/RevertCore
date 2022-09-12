namespace Revert.Core.Search
{
    public class Illegal : LexicalTokenizer
    {
        public static readonly Illegal Parser = new Illegal();

        private Illegal() : base() { }

        public Illegal(string token, int begin, int end) : base(token, begin, end, "otherToken") { }

        public override LexicalTokenizer TryParse(string inputStr, int position)
        {
            return new Illegal(inputStr[position].ToString(), position, position);
        }
    }
}
