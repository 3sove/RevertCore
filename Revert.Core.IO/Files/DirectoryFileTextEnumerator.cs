using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Revert.Core.Extensions;

namespace Revert.Core.IO.Files
{
    public class DirectoryFileTextEnumerator : IEnumerator<Tuple<string, string>>
    {
        public string DirectoryPath { get; private set; }
        private DirectoryInfo directoryInfo;
        private FileInfo[] files;
        private int filesPosition = -1;
        private bool includeSubFolders;

        public DirectoryFileTextEnumerator(string directoryPath, bool includeSubFolders) : this(new DirectoryInfo(directoryPath), includeSubFolders)
        {
        }

        public DirectoryFileTextEnumerator(DirectoryInfo directoryInfo, bool includeSubFolders)
        {
            DirectoryPath = directoryInfo.FullName;
            this.directoryInfo = directoryInfo;
            this.includeSubFolders = includeSubFolders;
            Reset();
        }

        public void Dispose()
        {
            files = null;
            directoryInfo = null;
        }

        public bool MoveNext()
        {
            if (++filesPosition == files.Length) return false;
            var file = files[filesPosition];
            using (var sr = file.OpenText())
                Current = new Tuple<string, string>(file.GetFileNameWithoutExtension(), sr.ReadToEnd());
            return true;
        }

        public void Reset()
        {
            filesPosition = -1;
            files = directoryInfo.GetFiles("*.*", includeSubFolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        }

        public Tuple<string, string> Current { get; private set; }

        object IEnumerator.Current
        {
            get { return Current; }
        }
    }
}