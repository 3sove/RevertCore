namespace Revert.Core.Common.Types.Tries
{
    public class TrieMatch<TItem>
    {
        public TItem Item { get; set; }
        public int Position { get; set; }

        public TrieMatch(TItem item, int position)
        {
            Item = item;
            Position = position;
        }
    }
}