using System;
using System.Collections.Generic;

namespace Revert.Core.IO.Files
{
    public class GenericFileEnumerator : IEnumerator<string>
    {
        public GenericFileEnumeratorModel Model { get; set; }

        public GenericFileEnumerator(GenericFileEnumeratorModel model)
        {
            Model = model;
            if (!System.IO.File.Exists(model.FilePath)) throw new System.IO.FileNotFoundException($"Could not find the specified file at {model.FilePath}.");
        }

        private System.IO.StreamReader fileStream;
        public System.IO.StreamReader FileStream => fileStream ?? (fileStream = System.IO.File.OpenText(Model.FilePath));

        int linesRead;
        public bool MoveNext()
        {
            currentLine = FileStream.ReadLine();
            linesRead++;

            int linesPerMessage = 1000;

            if ((linesRead % linesPerMessage) == 1) Console.WriteLine($"Reading line {linesRead} to {linesRead + linesPerMessage - 1}.");

            return currentLine != null;
        }
        
        string currentLine;
        public string Current
        {
            get
            {
                if (currentLine == string.Empty) currentLine = FileStream.ReadLine();
                return currentLine;
            }
        }

        object System.Collections.IEnumerator.Current => Current;

        public void Dispose()
        {
            FileStream.Close();
            FileStream.Dispose();
        }

        public void Reset()
        {
            FileStream.BaseStream.Position = 0;
        }
    }
}