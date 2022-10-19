using System;
using System.Collections.Generic;
using Revert.Core.Extensions;

namespace Revert.Core.Text.NLP
{
    public class EnglishDictionary
    {
        public List<Word> Words { get; set; }
        public Dictionary<string, Word> WordByString { get; set; }
        public WordIndex WordIndex { get; set; }

        public bool IncludeWordDetails { get; }

        public EnglishDictionary(EnglishDictionaryModel model)
        {
            WordByString = new Dictionary<string, Word>(StringComparer.InvariantCultureIgnoreCase);
            Words = new List<Word>();
            Model = model;
            Load();
        }

        public EnglishDictionaryModel Model { get; set; }

        public EnglishDictionary(EnglishDictionaryModel model, bool includeWordDetails = false) : this(model)
        {
            IncludeWordDetails = includeWordDetails;
        }

        protected void Load()
        {
            WordIndex = Model.WordIndex ?? (Model.WordIndex = new WordIndex());
            Words = Model.WordIndex.GetWords(Model.WordNetDirectoryPath);

            if (IncludeWordDetails) LoadWordDetails();

            foreach (var word in Words)
                WordByString[word.Value] = word;
        }

        private int recordsPerMessage = 1000;

        private void LoadWordDetails()
        {
            Console.WriteLine("Loading parts of speech tagger.");

            var partsOfSpeechTagger = new PartsOfSpeechTagger();
            partsOfSpeechTagger.CreateDictionary(Model.MobyPartsOfSpeechFilePath);

            Console.WriteLine("Enhancing parts of speech.");

            var i = 0;

            foreach (var partOfSpeechWord in partsOfSpeechTagger.PartsOfSpeechByWord)
            {
                if ((i++%recordsPerMessage) == 0)
                    Console.WriteLine("Enhancing words {0:#,#} through {1:#,#} of {2:#,#}.",
                        i, (i + recordsPerMessage - 1).OrIfSmaller(partsOfSpeechTagger.PartsOfSpeechByWord.Count), partsOfSpeechTagger.PartsOfSpeechByWord.Count);

                foreach (var partOfSpeech in partOfSpeechWord.Value)
                {
                    bool isNewWord;
                    var word = Model.WordIndex.AddPartOfSpeech(partOfSpeechWord.Key, partOfSpeech, Model.WordIndex, out isNewWord);
                    if (isNewWord) Model.Words.Add(word);
                }
            }
        }
    }
}
