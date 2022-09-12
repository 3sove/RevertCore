using System;
using System.Collections.Generic;
using System.Linq;
using Revert.Core.Common.Modules;
using Revert.Core.IO.Files;

namespace Revert.Core.Text.Extraction.Gazetteer
{
    public class GazetteerMatcherModule // : ModuleModel
    {
        public GazetteerGeneratorModel GazetteerGeneratorModel { get; set; }
        public Dictionary<string, int> HitsByTerm { get; set; }
        public string FilePathToMatch { get; set; }
    }

    public class GazetteerMatcher //: FunctionalModule<GazetteerMatcher, GazetteerMatcherModule>
    {
        protected void Execute(GazetteerMatcherModule model)
        {
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            var lineReader = new LineReader(model.FilePathToMatch, true);
            var hitsByKeyword = new Dictionary<string, int>();

            var tree = new TermTree(new TermTreeModel
                {
                    GazetteerGeneratorModel = model.GazetteerGeneratorModel
                });

            sw.Restart();

            var tweetCount = 0;

            foreach (var line in lineReader)
            {
                List<string> matchingTerms;
                tree.Evaluate(line.ToUpper(), out matchingTerms);
                tweetCount++;

                foreach (var match in matchingTerms)
                {
                    int currentKeywordHitCount;
                    hitsByKeyword.TryGetValue(match, out currentKeywordHitCount);
                    currentKeywordHitCount++;
                    hitsByKeyword[match] = currentKeywordHitCount;
                }

                if ((tweetCount % 1000) == 1)
                {
                    Console.WriteLine(string.Format("Evaluated 1000 tweets in {0}.", sw.Elapsed));
                    sw.Restart();
                }
            }

            model.HitsByTerm = hitsByKeyword;
        }

        //public override GazetteerMatcherModule PopulateTree(GazetteerMatcherModule model)
        //{
        //    var lineReader = new Modules.IO.Files.FileLineReader(model.FilePathToMatch, true);
        //    var hitsByKeyword = new Dictionary<string, int>();
        //    var keywords = model.GazetteerGeneratorModel.GetKeywords();


        //    foreach (var line in lineReader)
        //    {
        //        var lineTokens = GetTokens(line);

        //        foreach (var token in lineTokens)
        //        {
        //            if (keywords.Contains(token))
        //            {
        //                int currentKeywordHitCount = 0;
        //                hitsByKeyword.TryGetValue(token, out currentKeywordHitCount);
        //                currentKeywordHitCount++;
        //                hitsByKeyword[token] = currentKeywordHitCount;
        //            }
        //        }
        //    }
        //    model.HitsByTerm = hitsByKeyword;
        //    return model;
        //}

        public List<string> GetTokens(string inputString)
        {
            var index = 0;
            var currentTokenCharacters = new List<char>();
            var tokens = new List<string>();
            var allowedCharacterArray = new[] { '$', '#', '@' };
            var allowedSeparatorCharacterArray = new[] { '-', '_' };
            while (index < inputString.Length)
            {
                var currentCharacter = inputString[index];
                index++;

                if (char.IsLetterOrDigit(currentCharacter) ||
                    allowedCharacterArray.Contains(currentCharacter) ||
                    (currentTokenCharacters.Count > 0 && allowedSeparatorCharacterArray.Contains(currentCharacter)))
                {
                    currentTokenCharacters.Add(currentCharacter);
                }
                else
                {
                    if (currentTokenCharacters.Count != 0)
                    {
                        tokens.Add(new string(currentTokenCharacters.ToArray()));
                        currentTokenCharacters.Clear();
                    }
                }
            }
            return tokens;
        }
    }
}
