using System;
using System.Diagnostics;

namespace Revert.Core.IO
{
    public class DocumentCrawlerEnumerator : DocumentEnumerator<TextDocument>
    {

        public DocumentCrawlerEnumerator(string folderPath, string fileFilter, Action<string> messageOutputAction, bool includeSubfolders = true, ITextDocumentParser documentParser = null) :
            base(folderPath, fileFilter, messageOutputAction, includeSubfolders, documentParser)
        {
        }

        protected override TextDocument CreateDocument(string text, System.IO.FileInfo fileInfo, string rootSelectedDirectory)
        {
            Debug.Assert(fileInfo.Directory != null, "fileInfo.Directory != null");
            return new TextDocument(fileInfo.Name, fileInfo.Directory.Name, text)
            {
                FilePath = fileInfo.FullName
            };
        }
    }
}