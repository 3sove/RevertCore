using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Revert.Core.Common.Modules;
using Revert.Core.IO.Files;

namespace Revert.Core.Text.Extraction.RecordExtraction
{
    public class TagMatcherModel : ModuleModel
    {
        public string[] Tags { get; set; }

        public string[] SearchTerms { get; set; }

        public LineReader LineReader { get; set; }

    }

    public class TagMatcher //: FunctionalModule<TagMatcher, TagMatcherModel>
    {
        public void Execute(TagMatcherModel model)
        {
            var tweetString = new StringBuilder();

            var currentTweet = 0;

            var matchesByTerm = new Dictionary<string, int>();

            foreach (var line in model.LineReader)
            {
                var upperLine = line.ToUpper();
                if ((currentTweet++%10000) == 1)
                    Console.WriteLine(string.Format("Checking tweet {0} to {1}", currentTweet, currentTweet + 10000 - 1));
                
                foreach (var term in model.SearchTerms.Where(upperLine.Contains))
                {
                    int matchByTermCount;
                    matchesByTerm.TryGetValue(term, out matchByTermCount);
                    matchesByTerm[term] = ++matchByTermCount;
                    tweetString.AppendLine(line);
                }
            }

            Console.WriteLine("Matches by Item");

            foreach (var kvp in matchesByTerm)
                Console.WriteLine(string.Format("{0} ({1})", kvp.Key, kvp.Value.ToString("#,#")));

            Console.WriteLine("Tag Matching has completed.");
            Console.ReadKey();
        }
    }
}
