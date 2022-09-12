namespace Revert.Core.Search
{
    public class OR : LexicalTokenizer
    {
        public OR(string token, int begin, int end) : base(token, begin, end, "orToken") { }
    }
}
