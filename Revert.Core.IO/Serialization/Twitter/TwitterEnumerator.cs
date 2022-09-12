using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Revert.Core.IO.Serialization.Twitter
{
    public class TwitterEnumerator : IEnumerator<Models.JsonSerializationModels.tweet>
    {
        public string FilePath { get; set; }
        private readonly Func<Models.JsonSerializationModels.tweet, bool> tweetEvaluationFunction;
        public TwitterEnumerator(string filePath, Func<Models.JsonSerializationModels.tweet, bool> tweetEvaluationFunction = null)
        {
            FilePath = filePath;
            this.tweetEvaluationFunction = tweetEvaluationFunction;
            if (!System.IO.File.Exists(filePath)) throw new System.IO.FileNotFoundException($"Could not find the specified file at {filePath}.");
        }

        private System.IO.StreamReader fileStream;
        public System.IO.StreamReader FileStream => fileStream ?? (fileStream = System.IO.File.OpenText(FilePath));

        string currentLine;
        public string CurrentLine
        {
            get
            {
                if (currentLine == string.Empty)
                {
                    currentLine = FileStream.ReadLine();
                    if (linesRead == 1) currentLine = FileStream.ReadLine(); //Ensure we get past file definitions.
                }

                return currentLine;
            }
        }

        public System.IO.Stream CurrentLineAsStream => new System.IO.MemoryStream(Encoding.UTF8.GetBytes(CurrentLine));

        int linesRead;

        public bool MoveNext()
        {
            while (true)
            {
                currentLine = FileStream.ReadLine();
                linesRead++;

                if (tweetEvaluationFunction == null || tweetEvaluationFunction(Current)) break;
            }

            return currentLine != null;
        }

        private DataContractJsonSerializer jSonSerializer;
        private DataContractJsonSerializer JSonSerializer => jSonSerializer ?? (jSonSerializer = new DataContractJsonSerializer(typeof(Models.JsonSerializationModels.tweet)));

        public Models.JsonSerializationModels.tweet Current
        {
            get 
            {
                using (var stream = CurrentLineAsStream)
                    return JSonSerializer.ReadObject(stream) as Models.JsonSerializationModels.tweet;
            }
        }

        object System.Collections.IEnumerator.Current => Current;

        public void Dispose()
        {
            FileStream.Close();
            FileStream.Dispose();
        }

        public void Reset()
        {
            FileStream.BaseStream.Position = 0;
        }
    }
}