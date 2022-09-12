using System.Collections.Concurrent;
using Revert.Core.Common.Modules;

namespace Revert.Core.IO.Files
{
    public class LineCounterModel : ModuleModel
    {
        public string FilePath { get; set; }
        public int LineCount { get; set; }
        public ConcurrentDictionary<string, int> CountByTerm { get; set; }
    }
}