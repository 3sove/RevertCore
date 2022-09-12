using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using Revert.Core.Common.Text;
using Revert.Core.Common.Types.Collections;
using Revert.Core.Extensions;
using Revert.Core.IO.Serialization;
using Revert.Core.Text.NLP.WordNet;

namespace Revert.Core.Text.NLP
{
    [Serializable]
    public class WordIndex : SerializableBase<WordIndex>
    {
        private readonly SerializableDictionary<string, Word> wordByString = new SerializableDictionary<string, Word>();

        public void PopulateWordsByString()
        {
            wordByString.Clear();
            foreach (var word in Words)
                wordByString[word.Value.Value] = word.Value;
        }

        private SerializableDictionary<ObjectId, Word> words = new SerializableDictionary<ObjectId, Word>();
        public SerializableDictionary<ObjectId, Word> Words
        {
            get => words;
            set => words = value;
        }

        public Word GetOrCreateWord(ObjectId index)
        {
            lock (Words)
            {
                return Words.TryReturnOrSetValue(index, () => new Word());
            }
        }

        public override WordIndex DeSerialize(string path)
        {
            var index = base.DeSerialize(path);
            index.PopulateWordsByString();
            return index;
        }

        public bool AddWord(Word word)
        {
            lock (Words)
            {
                word.Id = ObjectId.GenerateNewId();
                Words[word.Id] = word;
                return true;
            }
        }

        private WordNetEngine wordNetEngine;

        public List<Word> GetWords(string wordNetDirectoryPath, bool includeRelatedWords = false, bool includeSynonyms = false)
        {
            Console.WriteLine("Initializing WordNet.");
            Console.WriteLine("Getting WordNet Words.");
            var wordNetWords = GetWordNetWords(wordNetEngine ?? (wordNetEngine = new WordNetEngine(wordNetDirectoryPath, true)));

            if (includeRelatedWords || includeSynonyms)
            {
                var i = 0;
                foreach (var word in wordNetWords)
                {
                    const int wordsPerMessage = 10000;
                    if ((i++ % wordsPerMessage) == 0)
                    {
                        Console.WriteLine("Populating related words and synonyms for words {0} - {1} of {2}",
                            i.ToString("#,#"),
                            (i + wordsPerMessage - 1).OrIfSmaller(wordNetWords.Count).ToString("#,#"),
                            wordNetWords.Count.ToString("#,#"));
                    }

                    var synSets = wordNetEngine.GetSynSets(word.Value).ToList();
                    foreach (var synSet in synSets)
                    {
                        if (includeRelatedWords) PopulateRelatedWords(word, synSet);
                        if (includeSynonyms) PopulateSynonyms(word, synSet);
                    }
                }
            }

            return wordNetWords;
        }

        private void PopulateSynonyms(Word word, SynSet synSet)
        {
            foreach (var synonym in synSet.Words)
            {
                bool isNewWord;
                var synonymWord = GetOrCreateWord(synonym, word.PartOfSpeech, out isNewWord);
                synonymWord.Definition = synSet.Gloss;
            }

            foreach (var semanticRelation in synSet.SemanticRelations)
            {
                var relatedSynSets = synSet.GetRelatedSynSets(semanticRelation, true).ToList();
                foreach (var relatedSynSet in relatedSynSets)
                {
                    foreach (var relatedSynSetWord in relatedSynSet.Words)
                    {
                        bool isNewWord;
                        word.AddSynonym(GetOrCreateWord(relatedSynSetWord, out isNewWord).Id);
                    }
                }
            }
        }

        private void PopulateRelatedWords(Word word, SynSet synSet)
        {
            foreach (var item in synSet.GetLexicallyRelatedWords())
            {
                bool isNewWord;
                foreach (var relatedWord in item.Value)
                    foreach (var relatedWordComponent in relatedWord.Value)
                        word.AddRelatedWord(item.Key, GetOrCreateWord(relatedWordComponent, out isNewWord).Id);
            }
        }

        private List<Word> GetWordNetWords(WordNetEngine wordNetEngine)
        {
            var wordNetWords = new List<Word>();
            foreach (var item in wordNetEngine.AllWords)
            {
                var partOfSpeech = PartsOfSpeech.Unknown;
                switch (item.Key)
                {
                    case WordNetEngine.Pos.Noun:
                        partOfSpeech = PartsOfSpeech.Noun;
                        break;
                    case WordNetEngine.Pos.Verb:
                        partOfSpeech = PartsOfSpeech.Verb;
                        break;
                    case WordNetEngine.Pos.Adjective:
                        partOfSpeech = PartsOfSpeech.Adjective;
                        break;
                    case WordNetEngine.Pos.Adverb:
                        partOfSpeech = PartsOfSpeech.Adverb;
                        break;
                }

                foreach (var word in item.Value)
                {
                    bool isNewWord;
                    wordNetWords.Add(GetOrCreateWord(word, partOfSpeech, out isNewWord));
                }
            }
            return wordNetWords;
        }

        public Word AddPartOfSpeech(string text, PartsOfSpeech partOfSpeech, WordIndex wordIndex, out bool isNewWord)
        {
            var word = GetOrCreateWord(text, partOfSpeech, out isNewWord);

            foreach (var synonymId in word.Synonyms)
            {
                var synonym = wordIndex.Words[synonymId];
                synonym.PartOfSpeech |= partOfSpeech;
            }
            return word;
        }


        private Word GetOrCreateWord(string text, PartsOfSpeech partOfSpeech, out bool isNewWord)
        {
            var word = GetOrCreateWord(text, out isNewWord);
            if (word.PartOfSpeech == PartsOfSpeech.Unknown) word.PartOfSpeech = partOfSpeech;
            else word.PartOfSpeech |= partOfSpeech;

            return word;
        }

        private Word GetOrCreateWord(string word, out bool isNewWord)
        {
            Word result;
            if (!wordByString.TryGetValue(word, out result))
            {
                result = new Word { Value = word };
                AddWord(result);
                wordByString[word] = result;
                isNewWord = true;
            }
            else isNewWord = false;

            return result;
        }
    }
}
