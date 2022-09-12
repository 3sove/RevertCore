using System;
using System.Diagnostics;
using System.IO;
using Revert.Core.Extensions;
using Revert.Core.IO;

namespace Revert.Core.Text.Extraction
{
    public class DocumentCrawlerEnumerator : DocumentEnumerator<DocumentInfo>
    {
        public DocumentCrawlerEnumerator(string folderPath, string fileFilter, Action<string> messageOutputAction, bool includeSubfolders = true) :
            base(folderPath, fileFilter, messageOutputAction, includeSubfolders)
        {
        }

        protected override DocumentInfo CreateDocument(string documentText, FileInfo fileInfo, string rootSelectedDirectory)
        {
            Debug.Assert(fileInfo.Directory != null, "fileInfo.Directory != null");
            return new DocumentInfo(rootSelectedDirectory)
            {
                DirectoryName = fileInfo.DirectoryName,
                DocumentName = fileInfo.Name,
                Text = fileInfo.ReadText()
            };
        }
    }
}