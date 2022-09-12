using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using Revert.Core.Common.Text;
using Revert.Core.Text.NLP;

namespace Revert.Core.Text.Tokenization
{
    public class DictionaryTokenizer : SimpleTokenizer
    {
        public EnglishDictionary EnglishDictionary { get; set; }

        public bool IncludeSynonyms { get; set; }

        public PartsOfSpeech ValidPartsOfSpeech { get; set; }

        public DictionaryTokenizer(EnglishDictionary dictionary)
        {
            EnglishDictionary = dictionary;
        }

        protected override bool IsTokenValid(string token)
        {
            Word word;
            if (!EnglishDictionary.WordByString.TryGetValue(token, out word)) return false;
            return ((word.PartOfSpeech & ValidPartsOfSpeech) != PartsOfSpeech.None);
        }

        public HashSet<ObjectId> GetSynonymIds(string token)
        {
            Word word;
            return EnglishDictionary.WordByString.TryGetValue(token, out word) ? word.Synonyms : null;
        }

        public List<string> GetTokens(HashSet<ObjectId> tokenIds)
        {
            return tokenIds.Select(tokenId => EnglishDictionary.WordIndex.Words[tokenId].Value.ToUpper()).ToList();
        }

        public List<string> GetSynonymTokens(string token)
        {
            if (!IncludeSynonyms) return null;

            var relatedTokens = new List<string>();
            Word word;
            if (EnglishDictionary.WordByString.TryGetValue(token, out word))
            {
                if (word.PartOfSpeech.HasFlag(PartsOfSpeech.Noun) || word.PartOfSpeech.HasFlag(PartsOfSpeech.Verb))
                {
                    var synonyms = word.GetSynonyms(EnglishDictionary);

                    relatedTokens.AddRange(synonyms.Select(synonym => synonym.Value.ToUpper()));
                }
            }
            return relatedTokens;
        }

        public List<string> GetRelatedTokens(string token)
        {
            var relatedTokens = new List<string>();
            Word word;
            if (EnglishDictionary.WordByString.TryGetValue(token, out word))
            {
                var relatedWords = word.GetRelatedWords(EnglishDictionary.WordIndex);
                foreach (var relatedWord in relatedWords)
                    relatedTokens.AddRange(relatedWord.Value.Select(relatedToken => relatedToken.Value.ToUpper()));
            }
            return relatedTokens;
        }

    }
}


