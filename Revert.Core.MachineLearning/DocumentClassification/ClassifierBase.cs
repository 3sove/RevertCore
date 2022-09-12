using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Revert.Core.Extensions;

namespace Revert.Core.MachineLearning.DocumentClassification
{
    public class ClassifierBase
    {
        public void OutputDocumentClassificationResults(List<DocumentClassificationResult> documentClassificationResults, string outputFilePathAndName)
        {
            int categoryIndex = 0;
            var categoryNamesAndIndex = new Dictionary<string, int>();

            foreach (var result in documentClassificationResults)
                foreach (var categoryWeight in result.CategoryWeights)
                    if (!categoryNamesAndIndex.ContainsKey(categoryWeight.Item1))
                        categoryNamesAndIndex[categoryWeight.Item1] = categoryIndex++;

            const string csvHeaderData = "\"Directory\",\"File Name\"";

            var fileToWrite = new FileInfo(outputFilePathAndName);

            if (fileToWrite.Exists == false)
            {
                using (var writer = fileToWrite.CreateText())
                {
                    writer.WriteLine(csvHeaderData + ",\"" + categoryNamesAndIndex.Keys.ToList().Combine("\",\"") + "\"");

                    foreach (var document in documentClassificationResults)
                    {
                        var categoryWeights = new List<string>();
                        var categoryWeightsAsDictionary = document.CategoryWeights.ToDictionary(item => item.Item1,
                                                                                                item => item.Item2);

                        foreach (var categoryNameAndIndex in categoryNamesAndIndex)
                        {
                            float documentPropertyValue;
                            if (categoryWeightsAsDictionary.TryGetValue(categoryNameAndIndex.Key, out documentPropertyValue))
                                categoryWeights.Add(documentPropertyValue.ToString(CultureInfo.InvariantCulture));
                            else categoryWeights.Add(string.Empty);
                        }

                        writer.WriteLine("\"" + document.Document.DirectoryName + "\",\"" + document.Document.Name +
                                         "\",\"" + categoryWeights.Combine("\",\"") + "\"");
                    }
                }
            }
        }
    }
}