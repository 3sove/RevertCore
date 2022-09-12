using System;
using System.Collections.Generic;

namespace Revert.Core.IO
{
    public class DocumentCrawler : DocumentEnumerable<TextDocument>
    {
        public ITextDocumentParser DocumentParser { get; set; }

        public DocumentCrawler(string folderPath, string fileFilter = "*.*", Action<string> messageOutputAction = null, bool includeSubfolders = true, ITextDocumentParser documentParser = null) :
            base(folderPath, fileFilter, messageOutputAction, includeSubfolders)
        {
            DocumentParser = documentParser;
        }

        private DocumentCrawlerEnumerator enumerator;
        public override IEnumerator<TextDocument> GetEnumerator()
        {
            return enumerator ?? (enumerator = new DocumentCrawlerEnumerator(FolderPath, FileFilter, MessageOutputAction, IncludeSubFolders, DocumentParser));
        }
    }
}