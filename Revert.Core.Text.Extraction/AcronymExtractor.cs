using System;
using System.Collections.Generic;
using System.Linq;
using Revert.Core.Extensions;
using Revert.Core.Text.Tokenization;

namespace Revert.Core.Text.Extraction
{
    public class Acronym
    {

    }

    public class AcronymExtractor
    {

        //find letters without spaces inside parentheses - especially if they're all caps

        //get variations of previous tokens n = {1+2+...acronym length}


        //get initial characters

        //evaluate cos similarity

        //if 

        public TokenIndex TokenIndex { get; set; }

        public AcronymExtractor(TokenIndex tokenIndex)
        {
            TokenIndex = tokenIndex;
        }

        public List<Acronym> GetAcronyms(string text)
        {
            var acronyms = new List<Acronym>();

            var tokens = text.GetTokens();
            var tokenArray = tokens.Select(pair => pair.Value).ToArray();

            var tokenIndex = 0;
            foreach (KeyValuePair<int, string> token in tokens)
            {
                Console.WriteLine("{0} : {1}", token.Key, token.Value);

                if (token.Value.StartsWith("(") && token.Value.EndsWith(")"))
                {
                    var acronym = token.Value.Substring(1, token.Value.Length - 2);

                    var acronymVector = acronym.ToArray();

                    var definitionVectorCandidates = new Dictionary<string[], float>();

                    for (int i = 0; i < (acronymVector.Length * 2).OrIfSmaller(tokenIndex); i++)
                    {
                        string[] candidateVector;
                        definitionVectorCandidates.Add(candidateVector = tokenArray.Skip(tokenIndex - i).Take(i).ToArray(),
                            GetAcronymMatchRank(acronymVector, candidateVector));
                    }


                }
                tokenIndex++;
            }


            return acronyms;
        }

        private float GetAcronymMatchRank(char[] acronymVector, string[] candidateVector)
        {
            double value = 1;
            if (candidateVector.Length < acronymVector.Length) value *= candidateVector.Length / Convert.ToDouble(acronymVector.Length);

            foreach (var word in candidateVector)
            {
                if (word.Length == 0) continue;

                Token token;
                if (TokenIndex.TryGetToken(word, out token))
                {
                }

            }
            throw new NotImplementedException();
        }
    }
}
