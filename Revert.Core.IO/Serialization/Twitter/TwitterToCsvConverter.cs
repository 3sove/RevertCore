using System;

namespace Revert.Core.IO.Serialization.Twitter
{
    public class TwitterToCsvConverter
    {
        public TwitterToCsvConverterModel Model { get; set; }

        public TwitterToCsvConverter(TwitterToCsvConverterModel model)
        {
            Model = model;
        }

        public void ParseFile(int recordCount)
        {
            var tweets = new TwitterFileReader(Model.FilePath);
            
            var schemaTweet = new Models.JsonSerializationModels.tweet();
            string csvDefinition = schemaTweet.GetCsvSchemaString(string.Empty);

            int targetRecordCount = recordCount;
            int writtenRecordCount = 0;
            int outputModulusSize = 10000;

            int maxRecordsPerFile = Model.MaxRecordsPerFile;
            int totalFileCount = 1;
            int currentFile = 0;
            if (recordCount > maxRecordsPerFile)
            {
                totalFileCount = (int)Math.Ceiling(recordCount / (double)maxRecordsPerFile);
            }

            var startDateTime = DateTime.Now.ToString("dd MMM yyyy HH-mm");
            System.IO.StreamWriter textWriter = null;
            var outputString = new System.Text.StringBuilder();
            try
            {
                foreach (var tweet in tweets)
                {
                    if (++writtenRecordCount > targetRecordCount) break;

                    if (writtenRecordCount > maxRecordsPerFile * currentFile)
                    {
                        string outputFileName = $"C:\\Development\\Projects\\APOL\\Data\\Pakistani Tweets\\Output of {recordCount.ToString("#,#")} tweets ({startDateTime}) part {++currentFile} of {totalFileCount}.csv";

                        if (textWriter != null)
                        {
                            textWriter.Flush();
                            textWriter.Close();
                            textWriter.Dispose();
                        }

                        textWriter = System.IO.File.CreateText(outputFileName);
                        textWriter.Write(csvDefinition);
                    }

                    if ((writtenRecordCount % outputModulusSize) == 1)
                    {
                        Model.UpdateMessageAction($"Writing converted tweet {writtenRecordCount} to {writtenRecordCount + outputModulusSize - 1} of {targetRecordCount}.");

                        if (textWriter != null)
                        {
                            textWriter.WriteLine(outputString.ToString());
                            outputString.Clear();
                            textWriter.Flush();
                        }
                    }

                    outputString.AppendLine(tweet.ToCsvString());
                }

                textWriter?.WriteLine(outputString.ToString());
            }
            finally
            {
                textWriter?.Flush();
                textWriter?.Close();
                textWriter?.Dispose();
            }
        }


    }
}
