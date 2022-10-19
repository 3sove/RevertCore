using System;
using System.Collections.Generic;
using System.Linq;
using Revert.Core.Text.Extraction.TextDocumentAndDirectory;
using Revert.Core.Text.Tokenization;
using ProtoBuf;

namespace Revert.Core.Text.Extraction
{
    public class TagMapModel
    {
        public TagMapModel()
        { 
        }

        public string TrainingDocumentAndDirectoryPath
        {
            get { return trainingDocumentAndDirectoryPath; }
            set { trainingDocumentAndDirectoryPath = value; }
        }

        private TextDocumentAndDirectoryEnumerable trainingSetItemEnumerable;
        public TextDocumentAndDirectoryEnumerable TrainingSetItemEnumerable
        {
            get { return trainingSetItemEnumerable ?? (trainingSetItemEnumerable = new TextDocumentAndDirectoryEnumerable(TrainingDocumentAndDirectoryPath)); }
            set
            {
                TrainingDocumentAndDirectoryPath = value.FolderPath;
                trainingSetItemEnumerable = value;
            }
        }

        private int totalRecordsToParse;
        public int TotalRecordsToParse
        {
            get { return totalRecordsToParse != 0 ? totalRecordsToParse : totalRecordsToParse = TrainingSetItemEnumerable.Count(); }
            set { totalRecordsToParse = value; }
        }

        public Dictionary<string, List<Tuple<string, float>>> TokenWeightByTokenByTag { get; set; }
        public TagMap TagMap { get; set; }

        public int AverageTokenCount { get; set; }
        public int MedianTokenCountAcrossTags { get; set; }

        private ITokenizer tokenizer;
        private string trainingDocumentAndDirectoryPath;

        public ITokenizer Tokenizer
        {
            get { return tokenizer ?? (tokenizer = new SimpleTokenizer()); }
            set { tokenizer = value; }
        }

        public TagCountByTokenGeneratorModel TagCountByTokenGeneratorModel { get; set; }

        public void UpdateFeature(string tag, string term, float weight)
        {
            if (string.IsNullOrEmpty(tag) || string.IsNullOrEmpty(tag)) throw new Exception();
            var topCountByToken = TagMap.CountByTagByTokenMatrix[term].OrderByDescending(t => t.Value).First().Value;
            TagMap.CountByTagByTokenMatrix[term][tag] = (int)(topCountByToken * (weight * 1.5));

            List<Tuple<string, float>> tokensByTag;
            if (!TokenWeightByTokenByTag.TryGetValue(tag, out tokensByTag))
                TokenWeightByTokenByTag[tag] = tokensByTag = new List<Tuple<string, float>>();

            tokensByTag.Remove(tokensByTag.First(t => t.Item1 == term));
            tokensByTag.Add(new Tuple<string, float>(term, weight));
        }
    }
}