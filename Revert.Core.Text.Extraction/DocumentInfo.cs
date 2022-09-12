using System.Collections.Generic;
using Revert.Core.IO;
using ProtoBuf;

namespace Revert.Core.Text.Extraction
{
    //[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class DocumentInfo : TrainingSetItem, ITextDocument
    {
        public DocumentInfo(string rootSelectedDirectory)
        {
            RootSelectedDirectory = rootSelectedDirectory;
        }

        public string RootSelectedDirectory { get; set; }

        public string DocumentName { get; set; }

        [ProtoIgnore]
        public override string Source => DocumentName;

        [ProtoIgnore]
        public override List<string> Tags => new List<string> { DirectoryName.Replace(RootSelectedDirectory ?? "", "").TrimStart('\\') };

        [ProtoIgnore]
        public string Name => DocumentName;

        [ProtoIgnore]
        public string DirectoryName { get; set; }
    }
}