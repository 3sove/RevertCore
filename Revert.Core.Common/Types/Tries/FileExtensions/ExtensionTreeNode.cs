using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Revert.Core.Common.Types.Tries.FileExtensions
{
    public class ExtensionTreeNode : TrieNode<byte, string>
    {
        public bool Evaluate(string filePath, out string extension)
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException(filePath + " not found");

            var fileBytes = new byte[256];
            using (var fs = new FileStream(filePath, FileMode.Open))
            {
                if (fs.Length >= 256) fs.Read(fileBytes, 0, 256);
                else fs.Read(fileBytes, 0, (int)fs.Length);
            }


            List<string> possibleExtensions;
            if (!TryEvaluate(fileBytes, 0, out possibleExtensions))
            {
                extension = string.Empty;
                return false;
            }
            //TODO: Order by probability or frequency in training data
            extension = possibleExtensions.First();
            return true;
        }
    }
}
