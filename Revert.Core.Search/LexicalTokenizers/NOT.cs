namespace Revert.Core.Search
{
    public class NOT : LexicalTokenizer
    {
        public NOT(string token, int begin, int end) : base(token, begin, end, "notToken") { }
    }
}
