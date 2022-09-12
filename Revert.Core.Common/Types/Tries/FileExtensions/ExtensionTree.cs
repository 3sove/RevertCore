using System.Collections.Generic;

namespace Revert.Core.Common.Types.Tries.FileExtensions
{
    public class ExtensionTree : Trie<byte, string>
    {
        public Dictionary<byte[], string> FileExtensionBySignature { get; }
        public ExtensionTree(Dictionary<byte[], string> fileExtensionBySignature)
        {
            FileExtensionBySignature = fileExtensionBySignature;
        }

        public void Populate()
        {
            foreach (var item in FileExtensionBySignature)
                RootNode.Add(item.Key, item.Value);
        }
    }
}
