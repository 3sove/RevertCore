using System.Collections.Generic;

namespace Revert.Core.IO.Serialization.Twitter
{
    public class TwitterFileReader : IEnumerable<Models.JsonSerializationModels.tweet>
    {
        public string FilePath { get; set; }
        public TwitterFileReader(string filePath)
        {
            FilePath = filePath;
            if (!System.IO.File.Exists(filePath)) throw new System.IO.FileNotFoundException($"Could not find the specified file at {filePath}.");
        }

        private IEnumerator<Models.JsonSerializationModels.tweet> enumerator;
        public IEnumerator<Models.JsonSerializationModels.tweet> GetEnumerator()
        {
            return enumerator ?? (enumerator = new TwitterEnumerator(FilePath));
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return enumerator ?? (enumerator = new TwitterEnumerator(FilePath));
        }
    }
}
