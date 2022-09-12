using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Revert.Core.IO.Files
{
    public class DirectoryFileTextEnumerable : IEnumerable<Tuple<string, string>>
    {
        public string DirectoryPath { get; private set; }
        private readonly DirectoryFileTextEnumerator enumerator;

        public DirectoryFileTextEnumerable(string directoryPath, bool includeSubFolders)
        {
            DirectoryPath = directoryPath;
            enumerator = new DirectoryFileTextEnumerator(directoryPath, includeSubFolders);
        }

        public DirectoryFileTextEnumerable(DirectoryInfo directoryInfo, bool includeSubFolders)
        {
            DirectoryPath = directoryInfo.FullName;
            enumerator = new DirectoryFileTextEnumerator(directoryInfo.FullName, includeSubFolders);
        }

        public IEnumerator<Tuple<string, string>> GetEnumerator()
        {
            return enumerator;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
