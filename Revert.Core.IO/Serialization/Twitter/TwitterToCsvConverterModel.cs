
namespace Revert.Core.IO.Serialization.Twitter
{
    public class TwitterToCsvConverterModel
    {
        public string FilePath { get; set; }
        public int MaxRecordsPerFile { get; set; } = 350000;
    }
}