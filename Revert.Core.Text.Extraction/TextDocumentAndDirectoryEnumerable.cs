using System;
using System.Collections.Generic;
using System.IO;
using Revert.Core.IO;
using ProtoBuf;

namespace Revert.Core.Text.Extraction.TextDocumentAndDirectory
{
    //[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class TextDocumentAndDirectoryEnumerable : DocumentEnumerable<DocumentInfo>
    {
        public int Count
        {
            get { return count != 0 ? count : (count = GetCount(true)); }
            set { count = value; }
        }

        public void Add(DocumentInfo docInfo)
        {
            throw new NotImplementedException();
        }

        //public TextDocumentAndDirectoryEnumerator Enumerator
        //{
        //    get { return enumerator ?? (enumerator = new TextDocumentAndDirectoryEnumerator(RootDirectoryPath)); }
        //    set { enumerator = value; }
        //}

        //private TextDocumentAndDirectoryEnumerator enumerator;

        private int count;

        public int GetCount(bool forceRefresh = false)
        {
            if (forceRefresh || Count == 0) Count = Directory.GetDirectories(FolderPath, "**", SearchOption.AllDirectories).Length;
            return Count;
        }

        private DocumentCrawlerEnumerator enumerator;

        public TextDocumentAndDirectoryEnumerable(string folderPath, string fileFilter, Action<string> messageOutputAction, bool includeSubfolders = true)
            : base(folderPath, fileFilter, messageOutputAction, includeSubfolders)
        {
        }

        public TextDocumentAndDirectoryEnumerable(string folderPath)
            : base(folderPath)
        {
        }

        public override IEnumerator<DocumentInfo> GetEnumerator()
        {
            return enumerator ?? (enumerator = new DocumentCrawlerEnumerator(FolderPath, FileFilter, MessageOutputAction, IncludeSubFolders));
        }
    }
}
