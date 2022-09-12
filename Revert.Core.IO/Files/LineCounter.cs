using System.Collections.Concurrent;
using Revert.Core.Common.Modules;

namespace Revert.Core.IO.Files
{
    public class LineCounter : FunctionalModule<LineCounter, LineCounterModel>
    {
        public int LineCount;
        public ConcurrentDictionary<string, int> CountByTerm = new ConcurrentDictionary<string, int>();

        public const int OutputDelayLineCount = 10000;

        protected override void Execute()
        {
            using (var textReader = System.IO.File.OpenText(Model.FilePath))
            {
                while (textReader.ReadLine() != null)
                {
                    LineCount++;
                    if (LineCount % OutputDelayLineCount == 1) PublishUpdateMessage(Model, "Read {0} lines so far.", LineCount);
                }
            }

            Model.LineCount = LineCount;
        }
    }
}
