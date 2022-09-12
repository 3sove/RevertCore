using System.IO;

namespace Revert.Core.Common
{
    public class ProgressLog
    {
        private readonly string filePath;

        public ProgressLog(string filePath, int defaultStartingValue = 0)
        {
            this.filePath = filePath;
            VerifyPath(defaultStartingValue);
        }

        private void VerifyPath(int defaultStartingValue)
        {
            var file = new FileInfo(filePath);
            if (file.Directory != null && !file.Directory.Exists)
                file.Directory.Create();

            if (!file.Exists)
                using (var fs = file.Create())
                using (var sw = new StreamWriter(fs))
                    sw.Write(defaultStartingValue);
        }

        public void WriteUpdate(int recordId)
        {
            using (var sw = new StreamWriter(filePath, false))
                sw.Write(recordId);
        }

        public int ReadLastUpdate()
        {
            using (var sr = new StreamReader(filePath))
            {
                int value;
                return int.TryParse(sr.ReadToEnd().Trim(), out value) ? value : 0;
            }
        }

    }
}
