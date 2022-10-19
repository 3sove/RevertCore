using System;
using System.Collections.Concurrent;

namespace Revert.Core.IO.Files
{
    public class LineCounter 
    {
        private int lineCount;
        public const int OutputDelayLineCount = 10000;
        public LineCounterModel Model { get; private set; } = new LineCounterModel();

        public LineCounter(string filePath)
        {
            Model.FilePath = filePath;
        }

        protected int Execute()
        {
            using (var textReader = System.IO.File.OpenText(Model.FilePath))
            {
                while (textReader.ReadLine() != null)
                {
                    lineCount++;
                    if (lineCount % OutputDelayLineCount == 1) Console.WriteLine("Read {0} lines so far.", lineCount);
                }
            }

            Model.LineCount = lineCount;
            return lineCount;
        }
    }
}
