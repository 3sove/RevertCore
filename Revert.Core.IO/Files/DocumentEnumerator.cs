using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Revert.Core.Extensions;

namespace Revert.Core.IO
{
    public abstract class DocumentEnumerator<TDocument> : IEnumerator<TDocument> where TDocument : ITextDocument
    {
        public string FolderPath { get; set; }
        public bool IncludeSubFolders { get; set; }
        public Action<string> MessageOutputAction { get; private set; }
        public string FileFilter { get; set; }

        public ITextDocumentParser DocumentParser { get; set; }

        protected DocumentEnumerator(string folderPath, string fileFilter, Action<string> messageOutputAction, bool includeSubfolders = true, ITextDocumentParser documentParser = null)
        {
            FolderPath = folderPath;
            FileFilter = fileFilter;
            MessageOutputAction = messageOutputAction ?? Console.WriteLine;
            IncludeSubFolders = includeSubfolders;
            PopulateDirectoryStack(new DirectoryInfo(folderPath));
            DocumentParser = documentParser ?? new TextDocumentParser();
        }

        Stack<DirectoryInfo> directories = new Stack<DirectoryInfo>();
        Stack<FileInfo> files = new Stack<FileInfo>();

        FileInfo currentFile;

        public void PopulateDirectoryStack(DirectoryInfo directory)
        {
            if (IncludeSubFolders) directory.GetDirectories().ForEach(PopulateDirectoryStack);

            directories.Push(directory);

            MessageOutputAction(directory.Parent == null 
                ? $"Populating file list from directory: {directory.Name}"
                : $"Populating file list from directory: {directory.Parent.Name}\\{directory.Name}");

            PopulateFileStack(directory);
        }

        private void PopulateFileStack(DirectoryInfo directory)
        {
            var directoryFiles = directory.GetFiles(FileFilter, SearchOption.TopDirectoryOnly);
            foreach (var file in directoryFiles)
                files.Push(file);
        }

        protected abstract TDocument CreateDocument(string documentText, FileInfo fileInfo, string rootSelectedDirectory);

        public TDocument Current
        {
            get
            {
                Debug.Assert(currentFile.Directory != null, "currentFile.Directory is null");
                var text = DocumentParser.GetDocumentText(currentFile);
                return CreateDocument(text, currentFile, FolderPath);
            }
        }

        public void Dispose()
        {
        }

        object IEnumerator.Current => Current;

        private Queue<FileInfo> poppedFiles = new Queue<FileInfo>();

        public bool MoveNext()
        {
            if (files == null || files.Count == 0 || files.Peek() == null)
            {
                Reset();
                return false;
            }

            poppedFiles.Enqueue(currentFile = files.Pop());
            return true;
        }

        public void Reset()
        {
            if (poppedFiles?.Any() ?? false)
            {
                while (poppedFiles.Any()) files.Push(poppedFiles.Dequeue());
            }
            else
            {
                PopulateDirectoryStack(new DirectoryInfo(FolderPath));
            }
        }
    }
}