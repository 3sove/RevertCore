using System.Collections.Generic;

namespace Revert.Core.Text.NLP
{
    public class EvaluationMatches
    {
        /// <summary>
        /// Partial EvaluationMatches by current position
        /// </summary>
        public Dictionary<int, List<SentenceEvaluationMatch>> PartialMatches = new Dictionary<int, List<SentenceEvaluationMatch>>();
        public List<SentenceEvaluationMatch> CompleteMatches = new List<SentenceEvaluationMatch>();
    }

}
