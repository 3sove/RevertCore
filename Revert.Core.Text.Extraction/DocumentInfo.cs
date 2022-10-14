using System.Collections.Generic;
using Revert.Core.IO;

namespace Revert.Core.Text.Extraction
{
    public class DocumentInfo : TrainingSetItem, ITextDocument
    {
        public DocumentInfo(string rootSelectedDirectory)
        {
            RootSelectedDirectory = rootSelectedDirectory;
        }

        public string RootSelectedDirectory { get; set; }

        public string DocumentName { get; set; }

        public override string Source => DocumentName;

        public override List<string> Tags => new List<string> { DirectoryName.Replace(RootSelectedDirectory ?? "", "").TrimStart('\\') };

        public string Name => DocumentName;

        public string DirectoryName { get; set; }
    }
}