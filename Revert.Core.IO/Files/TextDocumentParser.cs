using System.Diagnostics;
using System.IO;

namespace Revert.Core.IO
{
    public class TextDocumentParser : ITextDocumentParser
    {
        public string GetDocumentText(FileInfo file)
        {
            using (var stream = file.OpenText())
            {
                Debug.Assert(file.Directory != null, "currentFile.Directory is null");
                return stream.ReadToEnd();
            }
        }
    }
}
