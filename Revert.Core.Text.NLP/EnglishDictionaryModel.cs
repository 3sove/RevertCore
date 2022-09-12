using System.Collections.Generic;
using Revert.Core.Common.Modules;
using Revert.Core.Extensions;

namespace Revert.Core.Text.NLP
{
    public class EnglishDictionaryModel : ModuleModel
    {
        public string WordNetDirectoryPath { get; set; }
        public string MobyPartsOfSpeechFilePath { get; set; }

        public EnglishDictionaryModel()
        {
            WordNetDirectoryPath = NlpResources.WordNetDirectoryPath;
            MobyPartsOfSpeechFilePath = NlpResources.MobyPartsOfSpeechFilePath;
        }

        public List<Word> Words { get; set; }
        public WordIndex WordIndex { get; set; }
    }
}
