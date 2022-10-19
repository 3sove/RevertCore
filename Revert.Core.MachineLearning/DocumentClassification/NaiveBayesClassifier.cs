using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Revert.Core.IO;
using Revert.Core.Text.Extraction;

namespace Revert.Core.MachineLearning.DocumentClassification.Araculus
{
    public class NaiveBayesClassifier : ClassifierBase
    {
        public TagMapper TagMapper { get; set; } = new TagMapper();

        public DocumentClassificationResult Classify<TDocument>(TagMapModel model, TDocument document) where TDocument : ITextDocument
        {
            var messageTags = TagMapper.GetTagsLite(model.TagMap, document.Text, 0.0f, model.Tokenizer);
            return new DocumentClassificationResult { Document = document, CategoryWeights = messageTags };
        }

        public List<Tuple<string, float>> Classify(TagMapModel model, string text, float cutoff = 0.0f)
        {
            return TagMapper.GetTagsLite(model.TagMap, text, cutoff, model.Tokenizer);
        }

        public List<DocumentClassificationResult> Classify<TDocument>(TagMapModel model, IEnumerable<TDocument> documentEnumerable) where TDocument : ITextDocument
        {
            var documentClassificationResults = new List<DocumentClassificationResult>();
            int i = 0;
            foreach (var message in documentEnumerable)
            {
                if (i++ % 10 == 0) Console.WriteLine("Tagging document {0} through {1}.", i, i + 10);

                Dictionary<string, Dictionary<string, float>> frequenciesByTokenByTag;
                var messageTags = TagMapper.GetTags(model.TagMap, message.Text, 0.0f, model.Tokenizer, out frequenciesByTokenByTag);
                documentClassificationResults.Add(new DocumentClassificationResult { Document = message, CategoryWeights = messageTags, CategoryTokenWeights = frequenciesByTokenByTag });
            }

            return documentClassificationResults;
        }

        /// <summary>
        /// Doesn't include the CategoryTokenWeights
        /// </summary>
        public List<DocumentClassificationResult> ClassifyLite<TDocument>(TagMapModel model, IEnumerable<TDocument> documentEnumerable) where TDocument : ITextDocument
        {
            var documentClassificationResults = new List<DocumentClassificationResult>();
            int i = 0;

            var sw = new Stopwatch();
            sw.Start();
            Parallel.ForEach(documentEnumerable, message =>
            {
                if (i++ % 100 == 0) Console.WriteLine("Tagging document {0} through {1} - {2} documents per second.", i, i + 100, (i / (sw.ElapsedMilliseconds * 0.001f)));
                var messageTags = TagMapper.GetTagsLite(model.TagMap, message.Text, 0.0f, model.Tokenizer);
                documentClassificationResults.Add(new DocumentClassificationResult { Document = message, CategoryWeights = messageTags });
            });
            
            return documentClassificationResults;
        }

        public void ClassifyMultipleDirectories<TDocument>(TagMapModel model, Dictionary<string, IEnumerable<TDocument>> documentEnumerables, bool includeCategoryTokenWeights = false) where TDocument : ITextDocument
        {
            int i = 0;

            var sw = new Stopwatch();
            sw.Start();

            foreach (var item in documentEnumerables)
            {
                var documentClassificationResults = new List<DocumentClassificationResult>();
                Parallel.ForEach(item.Value, message =>
                {
                    if (i++ % 100 == 0) Console.WriteLine("Tagging document {0} through {1} - {2} documents per second.",
                        i, i + 100, (i / (sw.ElapsedMilliseconds * 0.001f)));

                    Dictionary<string, Dictionary<string, float>> categoryTokenWeights = null;

                    var messageTags = includeCategoryTokenWeights ? TagMapper.GetTags(model.TagMap, message.Text, 0.0f, model.Tokenizer, out categoryTokenWeights) : TagMapper.GetTagsLite(model.TagMap, message.Text, 0.0f, model.Tokenizer);

                    documentClassificationResults.Add(new DocumentClassificationResult
                    {
                        Document = message,
                        CategoryWeights = messageTags,
                        CategoryTokenWeights = categoryTokenWeights
                    });
                });
                OutputDocumentClassificationResults(documentClassificationResults,
                    string.Format("C:\\Development\\Machine_Learning_Algorithm\\Evaluation Result Araculus {0} - {1}.csv",
                    item.Key, DateTime.Now.ToString("dd MMMM yyyy hh_mm")));
            }
        }
    }
}