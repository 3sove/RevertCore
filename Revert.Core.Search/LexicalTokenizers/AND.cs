namespace Revert.Core.Search
{
    public class AND : LexicalTokenizer
    {
        public AND(string token, int begin, int end) : base(token, begin, end, "andToken") { }
    }
}
