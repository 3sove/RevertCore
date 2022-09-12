using System;
using System.Collections;
using System.Collections.Generic;

namespace Revert.Core.IO
{
    public abstract class DocumentEnumerable<TDocument> : IEnumerable<TDocument> where TDocument : ITextDocument
    {
        public string FolderPath { get; set; }
        public bool IncludeSubFolders { get; set; }
        public Action<string> MessageOutputAction { get; private set; }
        public string FileFilter { get; set; }

        protected DocumentEnumerable(string folderPath, string fileFilter = "*.*", Action<string> messageOutputAction = null, bool includeSubfolders = true)
        {
            FolderPath = folderPath;
            FileFilter = fileFilter;
            MessageOutputAction = messageOutputAction;
            IncludeSubFolders = includeSubfolders;
        }

        public abstract IEnumerator<TDocument> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}


