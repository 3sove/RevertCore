using System;
using System.IO;
using Revert.Core.Extensions;

namespace Revert.Core.IO
{
    public class TextDocument : ITextDocument
    {
        public TextDocument()
        {
        }

        public TextDocument(string name, string text)
        {
            Name = name;
            Text = text;
        }

        public TextDocument(string name, string directoryName, string text)
            : this(name, text)
        {
            DirectoryName = directoryName;
        }

        public string Name { get; private set; }
        public string DirectoryName { get; private set; }
        public string Text { get; private set; }

        private int? hash;
        private byte[] fileHash;
        public int Hash => (int)(hash ?? (hash = Convert.ToInt32(FileHash)));

        public string FilePath { get; set; }

        public byte[] FileHash

        {
            get
            {
                if (fileHash != null) return fileHash;
                var file = new FileInfo(FilePath);
                if (file.Exists) fileHash = FileHash = File.ReadAllBytes(file.FullName).ComputeHash();
                return fileHash;
            }
            set { fileHash = value; }
        }
    }
}

