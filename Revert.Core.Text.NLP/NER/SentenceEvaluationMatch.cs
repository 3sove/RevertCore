using Revert.Core.Extensions;
using System.Collections.Generic;

namespace Revert.Core.Text.NLP
{
    public class SentenceEvaluationMatch
    {
        public string Category { get; set; }
        public TokenCategorizationMap CategorizationMap { get; set; }
        private List<MatchedWord> matchedWords;
        public List<MatchedWord> MatchedWords
        {
            get { return matchedWords ?? (matchedWords = new List<MatchedWord>()); }
            set { matchedWords = value; }
        }

        public override string ToString()
        {
            return $"{Category}: {MatchedWords.Combine(" ")}";
        }
    }

}
