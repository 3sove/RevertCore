using System;
using System.Collections.Generic;

namespace Revert.Core.IO.Files
{
    public class LineReader : IEnumerable<string>, IDisposable
    {
        public string FilePath { get; set; }
        public bool OutputToConsole { get; set; }
        public int LinesPerMessage { get; set; }
        public LineReader(string filePath, bool outputToConsole = false, int linesPerMessage = 1000)
        {
            FilePath = filePath;
            OutputToConsole = outputToConsole;
            LinesPerMessage = linesPerMessage;
            if (!System.IO.File.Exists(filePath)) throw new System.IO.FileNotFoundException($"Could not find the specified file at {filePath}.");
        }

        private IEnumerator<string> enumerator;
        public IEnumerator<string> GetEnumerator()
        {
            return enumerator ?? (enumerator = new GenericFileEnumerator(new GenericFileEnumeratorModel { FilePath = FilePath, RecordsPerMessage = LinesPerMessage }));
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return enumerator ?? (enumerator = new GenericFileEnumerator(new GenericFileEnumeratorModel { FilePath = FilePath, RecordsPerMessage = LinesPerMessage }));
        }

        public void Dispose()
        {
            enumerator.Dispose();
        }
    }
}
