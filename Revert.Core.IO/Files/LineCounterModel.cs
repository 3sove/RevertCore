using System.Collections.Concurrent;

namespace Revert.Core.IO.Files
{
    public class LineCounterModel
    {
        public string FilePath { get; set; }
        public int LineCount { get; set; }
        public ConcurrentDictionary<string, int> CountByTerm { get; set; }
    }
}