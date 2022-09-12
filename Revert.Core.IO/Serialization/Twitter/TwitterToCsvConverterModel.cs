using Revert.Core.Common.Modules;

namespace Revert.Core.IO.Serialization.Twitter
{
    public class TwitterToCsvConverterModel : ModuleModel
    {
        public string FilePath { get; set; }
        public int MaxRecordsPerFile { get; set; } = 350000;
    }
}