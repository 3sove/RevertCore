namespace Revert.Core.Text.NLP
{
    public class MatchedWord
    {
        public SentenceToken Token { get; set; }
        public WordMatchType MatchType { get; set; }
        //public EntityExtractionTreeNode MatchedTreeNode { get; set; }

        public MatchedWord(SentenceToken token, WordMatchType matchType)//, EntityExtractionTreeNode matchedTreeNode)
        {
            Token = token;
            MatchType = matchType;
            //MatchedTreeNode = matchedTreeNode;
        }
    }

}
