using System.Collections.Generic;
using System.Linq;
using Revert.Core.Extensions;

namespace Revert.Core.Text.NLP
{
    public class TokenCategorizationMap
    {
        public static HashSet<string> AllCategories = new HashSet<string>();
        public int StartTokenIndex { get; set; }
        public string Category
        {
            get { return category; }
            set { AllCategories.Add(category = value); }
        }

        public List<SentenceToken> SpanTokens { get; set; }

        public int Id { get; set; }

        private static int globalIds;
        private string category;

        public TokenCategorizationMap()
        {
            Id = globalIds++;
        }

        public override string ToString()
        {
            return $"{Category}: {SpanTokens.Select(t => t.Word.Value).Combine(" ")}";
        }
    }
}