using System.Collections.Generic;
using Revert.Core.Common.Text;
using Revert.Core.Extensions;

namespace Revert.Core.Text.NLP
{
    public class Tokenizer
    {
        public static List<Word> GetTokens(string value, EnglishDictionary dictionary)
        {
            var tokens = new List<Word>();

            foreach (var token in value.GetTokens())
            {
                Word tokenWord;
                var upperToken = token.Value.ToUpper();
                if (!dictionary.WordByString.TryGetValue(upperToken, out tokenWord))
                {
                    tokenWord = new Word { Value = token.Value, PartOfSpeech = PartsOfSpeech.Unknown };
                    dictionary.WordByString[upperToken] = tokenWord;
                }
                tokens.Add(tokenWord);
            }
            return tokens;
        }

        public static List<SentenceToken> GetSentenceTokens(string value, EnglishDictionary dictionary)
        {
            var tokens = new List<SentenceToken>();
            foreach (var token in value.GetTokens())
            {
                Word tokenWord;
                if (!dictionary.WordByString.TryGetValue(token.Value, out tokenWord))
                {
                    tokenWord = new Word { Value = token.Value, PartOfSpeech = PartsOfSpeech.Unknown };
                    dictionary.WordByString[token.Value] = tokenWord;
                }
                tokens.Add(new SentenceToken(tokenWord, token.Key));
            }
            return tokens;
        }

    }
}
