
namespace Revert.Core.IO.Files
{
    public class LargeFileSplitterModel
    {
        private FileSplitOptions splitOptions = FileSplitOptions.SplitByByteCount;
        public FileSplitOptions SplitOptions
        {
            get { return splitOptions; }
            set { splitOptions = value; }
        }

        private int fileSplitSize = 1024 * 100;
        public int FileSplitSize
        {
            get { return fileSplitSize; }
            set { fileSplitSize = value; }
        }

        public string[] FileNames { get; set; }
    }
}