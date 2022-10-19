using System;
using System.Collections.Generic;
using Revert.Core.Text.Tokenization;

namespace Revert.Core.Text.Extraction
{
    public class TokenInstanceCountByTagGeneratorModel
    {
        public Dictionary<string, Dictionary<string, int>> CountByTagByToken
        {
            get;
            set;
        }

        public IEnumerable<Tuple<string, IEnumerable<string>>> RecordTextAndTagEnumerable { get; set; }
        private int totalRecordsToParse = 10000;
        public int TotalRecordsToParse
        {
            get { return totalRecordsToParse; }
            set { totalRecordsToParse = value; }
        }

        public HashSet<string> HashTags { get; set; }
        public Dictionary<string, int> FullCountByTag { get; set; }

        public int RecordCount { get; set; }
    }

    public class TokenInstanceCountByTagGenerator
    {
        public void Execute(TokenInstanceCountByTagGeneratorModel model)
        {
            //for each word in the tweet, increase the word's hashtag dictionary values for the hashtags
            var countByTagByToken = new Dictionary<string, Dictionary<string, int>>();

            int recordCount = 0;

            var hashTags = new HashSet<string>();

            var fullCountByTag = new Dictionary<string, int>();
            var tokenizer = new SimpleTokenizer();

            foreach (var textAndTags in model.RecordTextAndTagEnumerable)
            {
                if ((++recordCount % 1000) == 1) Console.WriteLine("Reading line {0} through {1}.", recordCount, recordCount + 1000 - 1);

                foreach (var token in tokenizer.GetTokens(textAndTags.Item1))
                {
                    Dictionary<string, int> countByTag;
                    if (!countByTagByToken.TryGetValue(token, out countByTag))
                    {
                        countByTag = new Dictionary<string, int>();
                        countByTagByToken[token] = countByTag;
                    }

                    //foreach tag in the tweet
                    foreach (var tag in textAndTags.Item2)
                    {
                        hashTags.Add(tag);
                        int tagCount;
                        countByTag.TryGetValue(tag, out tagCount);
                        countByTag[tag] = ++tagCount;

                        int fullCount;
                        fullCountByTag.TryGetValue(tag, out fullCount);
                        fullCountByTag[tag] = ++fullCount;
                    }
                }

                if (recordCount == model.TotalRecordsToParse) break;
            }

            model.CountByTagByToken = countByTagByToken;
            model.HashTags = hashTags;
            model.FullCountByTag = fullCountByTag;
            model.RecordCount = recordCount;
        }
    }
}

