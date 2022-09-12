using System.Collections.Generic;
using System.Linq;
using Revert.Core.Common.Text;
using Revert.Core.Extensions;
using Revert.Core.IO.Files;

namespace Revert.Core.Text.NLP
{
    public class PartsOfSpeechTagger
    {
        public Dictionary<PartsOfSpeech, HashSet<string>> WordsByPartsOfSpeech { get; set; } = new Dictionary<PartsOfSpeech, HashSet<string>>();

        public Dictionary<string, HashSet<PartsOfSpeech>> PartsOfSpeechByWord { get; set; } = new Dictionary<string, HashSet<PartsOfSpeech>>();

        public void CreateDictionary(string path)
        {
            var reader = new LineReader(path, true, 1);

            foreach (var line in reader)
            {
                var segments = line.Split((char)65533);
                var word = segments[0];

                foreach (var item in segments[1].ToArray())
                {
                    var partOfSpeech = GetPartOfSpeech(item);
                    WordsByPartsOfSpeech.AddToCollection(partOfSpeech, word);
                    PartsOfSpeechByWord.AddToCollection(word, partOfSpeech);
                }
            }
        }

        private PartsOfSpeech GetPartOfSpeech(char character)
        {
            switch (character)
            {
                case 'N':
                    return PartsOfSpeech.Noun;
                case 'p':
                    return PartsOfSpeech.Plural;
                case 'h':
                    return PartsOfSpeech.NounPhrase;
                case 'V':
                    return PartsOfSpeech.Verb;
                case 't':
                    return PartsOfSpeech.TransitiveVerb;
                case 'i':
                    return PartsOfSpeech.IntransitiveVerb;
                case 'A':
                    return PartsOfSpeech.Adjective;
                case 'v':
                    return PartsOfSpeech.Adverb;
                case 'C':
                    return PartsOfSpeech.Conjunction;
                case 'P':
                    return PartsOfSpeech.Preposition;
                case '!':
                    return PartsOfSpeech.Interjection;
                case 'r':
                    return PartsOfSpeech.Pronoun;
                case 'D':
                    return PartsOfSpeech.DefiniteArticle;
                case 'I':
                    return PartsOfSpeech.IndefiniteArticle;
                case 'o':
                    return PartsOfSpeech.Nominative;
                default:
                    return PartsOfSpeech.Unknown;
            }
        }

        public PartsOfSpeechResult Evaluate(string textToEvaluate)
        {
            return new PartsOfSpeechResult(textToEvaluate);
        }
    }

    public class PartsOfSpeechResult
    {
        public string OriginalText { get; private set; }

        public PartsOfSpeechResult(string originalText)
        {
            OriginalText = originalText;
        }
    }
}
