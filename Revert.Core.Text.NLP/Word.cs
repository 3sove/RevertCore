using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using Revert.Core.Common.Text;
using Revert.Core.Common.Types.Collections;
using Revert.Core.Extensions;
using Revert.Core.Text.NLP.WordNet;

namespace Revert.Core.Text.NLP
{
    public class SentenceToken
    {
        public int RawStartingPosition { get; set; }
        public Word Word { get; set; }

        public SentenceToken(Word word, int rawStartingPosition)
        {
            Word = word;
            RawStartingPosition = rawStartingPosition;
        }

        public override string ToString()
        {
            return Word.Value;
        }
    }

    public class Word
    {
        public ObjectId Id { get; set; }
        //public int Id { get; set; }
        public string Value { get; set; }

        public PartsOfSpeech PartOfSpeech { get; set; }
        public string Definition { get; set; }

        private HashSet<ObjectId> synonyms = new HashSet<ObjectId>();
        public HashSet<ObjectId> Synonyms
        {
            get { return synonyms; }
            set { synonyms = value; }
        }

        public List<Word> GetSynonyms(EnglishDictionary dictionary)
        {
            return synonyms.Select(dictionary.WordIndex.GetOrCreateWord).ToList();
        }

        public void AddSynonym(ObjectId index)
        {
            synonyms.Add(index);
        }

        private SerializableDictionary<WordNetEngine.SynSetRelation, List<ObjectId>> relatedWords;
        public SerializableDictionary<WordNetEngine.SynSetRelation, List<ObjectId>> RelatedWords
        {
            get { return relatedWords ?? (relatedWords = new SerializableDictionary<WordNetEngine.SynSetRelation, List<ObjectId>>()); }
            set { relatedWords = value; }
        }

        public void AddRelatedWord(WordNetEngine.SynSetRelation key, ObjectId index)
        {
            RelatedWords.AddToCollection(key, index);
        }

        public Dictionary<WordNetEngine.SynSetRelation, List<Word>> GetRelatedWords(WordIndex index)
        {
            var dictionary = new Dictionary<WordNetEngine.SynSetRelation, List<Word>>();

            foreach (var synonym in RelatedWords)
            {
                foreach (var synonymWord in synonym.Value)
                {
                    dictionary.AddToCollection(synonym.Key, index.GetOrCreateWord(synonymWord));
                }
            }

            return dictionary;
        }

        public bool IsMatch(Word otherWord, out WordMatchType matchType)
        {
            matchType = WordMatchType.NoMatch;
            if (this == otherWord)
            {
                matchType = WordMatchType.Exact;
            }
            else if (Synonyms.Contains(otherWord.Id))
            {
                matchType = WordMatchType.Synonym;
            }
            else if ((PartOfSpeech & otherWord.PartOfSpeech) != PartsOfSpeech.None)
            {
                matchType = WordMatchType.PartOfSpeech;
            }
            return matchType != WordMatchType.NoMatch;
        }

        public override string ToString()
        {
            return string.Format("{0} : {1}", Value, PartOfSpeech);
        }

        public void Merge(Word word)
        {
            if (word == this) return;

            word.Synonyms.ForEach(w => Synonyms.Add(w));
            PartOfSpeech |= word.PartOfSpeech;
            if (Definition != word.Definition)
            {
                if (string.IsNullOrWhiteSpace(Definition) || 
                    (!string.IsNullOrWhiteSpace(word.Definition) && 
                    word.Definition.Length > Definition.Length)) Definition = word.Definition;

            }

            word.RelatedWords.ForEach(relatedWord =>
            {
                if (!RelatedWords.TryGetValue(relatedWord.Key, out List<ObjectId> relatedWordIds) || relatedWordIds == null)
                    relatedWordIds = new List<ObjectId>();

                RelatedWords[relatedWord.Key] = relatedWordIds.Union(relatedWord.Value).ToList();
            });
        }
    }

    public enum WordMatchType
    {
        NoMatch = 0,
        Exact = 1,
        Synonym = 2,
        PartOfSpeech = 3
    }
}
