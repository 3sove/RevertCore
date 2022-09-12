using System.Collections.Generic;

namespace Revert.Core.Text.NLP.SentenceMaps
{
    public class AnnotatedSentenceMap
    {
        public string OriginalString { get; set; }
        public List<SentenceToken> Tokens { get; set; }
        public List<TokenCategorizationMap> TokenCategorizationMaps { get; set; }
        public Dictionary<int, List<TokenCategorizationMap>> TokenCategorizationMapsByStartingPosition { get; set; }
    }
}
