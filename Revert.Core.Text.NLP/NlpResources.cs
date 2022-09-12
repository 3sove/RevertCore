using System;
using System.Collections.Generic;
using System.Text;
using Revert.Core.Extensions;

namespace Revert.Core.Text.NLP
{
    public static class NlpResources
    {
        public static string NlpPath { get; set; } = @"A:\Woodhouse\NLP\";

        public static string WordNetDirectoryPath = NlpPath.AddFilePath("WordNet\\");
        public static string MobyPartsOfSpeechFilePath = NlpPath.AddFilePath("Parts of speech.data");






    }
}
