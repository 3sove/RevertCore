using System;
using MongoDB.Bson;
using Revert.Core.Common.Text;
using Revert.Core.IO;
using Revert.Core.Text.NLP;

namespace Revert.Core.Text.Tokenization
{
    public class Token : IMongoRecord
    {
        public Token(bool getNewId = false)
        {
            //Synonyms = new HashSet<int>();
            //RelatedWords = new Dictionary<WordNetEngine.SynSetRelation, List<int>>();
        }

        public ObjectId Id { get; set; }

        public string Value { get; set; }

        public PartsOfSpeech PartOfSpeech { get; set; }


        //public string Definition { get; set; }

        //public HashSet<int> Synonyms { get; set; }

        //public Dictionary<WordNetEngine.SynSetRelation, List<int>> RelatedWords { get; set; }

        public bool IsMeaningful
        {
            get
            {
                return true;

                //TODO: Fix checks to see if meaningful tokens are preventing searches on unmeaningful tokens
                switch (PartOfSpeech)
                {
                    case PartsOfSpeech.Conjunction:
                    case PartsOfSpeech.Preposition:
                    case PartsOfSpeech.Interjection:
                    case PartsOfSpeech.Pronoun:
                    case PartsOfSpeech.DefiniteArticle:
                    case PartsOfSpeech.IndefiniteArticle:
                    case PartsOfSpeech.Nominative:
                    case PartsOfSpeech.SubordinateConjunction:
                        return false;
                    default:
                        return true;
                }
            }
        }

        //public IEnumerable<Token> GetSynonyms(TokenIndex index)
        //{
        //    if (Synonyms == null || Synonyms.Count == 0) return null;

        //    return Synonyms.Select(id =>
        //    {
        //        Token token;
        //        index.TryGetToken(id, out token);
        //        return token;
        //    });
        //}

        //public void AddRelatedWord(WordNetEngine.SynSetRelation key, int index)
        //{
        //    RelatedWords.AddToCollection(key, index);
        //}

        //public Dictionary<WordNetEngine.SynSetRelation, List<Token>> GetRelatedWords(TokenIndex index)
        //{
        //    var dictionary = new Dictionary<WordNetEngine.SynSetRelation, List<Token>>();

        //    foreach (var relatedWord in RelatedWords)
        //        foreach (var value in relatedWord.KeyTwo)
        //        {
        //            Token token;
        //            if (index.TryGetToken(value, out token))
        //                dictionary.AddToCollection(relatedWord.KeyOne, token);
        //        }

        //    return dictionary;
        //}

        public bool IsMatch(Token otherToken, out WordMatchType matchType)
        {
            matchType = WordMatchType.NoMatch;
            if (Equals(this, otherToken))
                matchType = WordMatchType.Exact;
            //else if (Synonyms.Contains(otherToken.Id))
            //    matchType = WordMatchType.Synonym;
            //else if ((PartOfSpeech & otherToken.PartOfSpeech) != PartsOfSpeech.None)
            //    matchType = WordMatchType.PartOfSpeech;

            if (string.Equals(Value, otherToken.Value, StringComparison.OrdinalIgnoreCase))
                matchType = WordMatchType.Exact;
            return matchType != WordMatchType.NoMatch;
        }

        public override string ToString()
        {
            return $"{Value} : {PartOfSpeech}";
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode() ^ Value.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var otherToken = obj as Token;
            return otherToken?.Id == Id;
        }
    }
}
