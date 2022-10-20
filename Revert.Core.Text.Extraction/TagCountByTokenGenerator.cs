using System;
using System.Collections.Generic;
using Revert.Core.Common.Performance;
using Revert.Core.Extensions;
using Revert.Core.Text.Tokenization;

namespace Revert.Core.Text.Extraction
{
    public class TagCountByTokenGeneratorModel// : ModuleModel
    {
        public TagMap TagMap { get; set; }
        public IEnumerable<TrainingSetItem> TrainingSetItems { get; set; }
        public int MaximumRecordsToParse { get; set; } = int.MaxValue;

        private ITokenizer tokenizer;
        public ITokenizer Tokenizer
        {
            get { return tokenizer ?? (tokenizer = new SimpleTokenizer()); }
            set { tokenizer = value; }
        }

        public string DirectoryPath { get; set; }
    }

    public class TagCountByTokenGenerator
    {
        public TagCountByTokenGeneratorModel Model { get; set; } 

        public void Execute(TagCountByTokenGeneratorModel model)
        {
            Model = model;
            var countByToken = new Dictionary<string, int>();
            var countByTagByToken = new Dictionary<string, Dictionary<string, int>>();

            var recordCountByTag = new Dictionary<string, int>();

            var performanceMonitor = new PerformanceMonitor("Training Set Population", 1000, Model.MaximumRecordsToParse, Console.WriteLine);

            foreach (var trainingSetItem in Model.TrainingSetItems)
            {
                var tokens = Model.Tokenizer.GetTokens(trainingSetItem.Text);

                foreach (var token in tokens)
                {
                    countByToken.IncrementValue(token);

                    Dictionary<string, int> countByTag;
                    if (!countByTagByToken.TryGetValue(token, out countByTag))
                        countByTagByToken[token] = countByTag = new Dictionary<string, int>();

                    trainingSetItem.Tags.ForEach(tag => countByTag.IncrementValue(tag));
                }

                trainingSetItem.Tags.ForEach(tag => recordCountByTag.IncrementValue(tag));
                if (performanceMonitor.Tick() == Model.MaximumRecordsToParse) break;
            }

            Model.TagMap = new TagMap
            {
                CountByTagByTokenMatrix = countByTagByToken,
                TotalCountByToken = countByToken,
                RecordCount = (int)performanceMonitor.Position,
                RecordCountByTag = recordCountByTag
            };
        }

        public TagCountByTokenGeneratorModel UpdateTagMap(TagCountByTokenGeneratorModel model, TrainingSetItem trainingSetItem)
        {
            var countByToken = model.TagMap.TotalCountByToken;
            var countByTagByToken = model.TagMap.CountByTagByTokenMatrix;

            Console.WriteLine($"Updating model with new training set item: {trainingSetItem.Source.OrIfEmpty("User Input Text")}");
            var tokens = model.Tokenizer.GetTokens(trainingSetItem.Text);
            foreach (var token in tokens)
            {
                Dictionary<string, int> countByTag;
                if (!countByTagByToken.TryGetValue(token, out countByTag)) countByTagByToken[token] = countByTag = new Dictionary<string, int>();

                countByToken.IncrementValue(token);
                trainingSetItem.Tags.ForEach(tag => countByTag.IncrementValue(tag));
            }

            trainingSetItem.Tags.ForEach(tag => model.TagMap.RecordCountByTag.IncrementValue(tag));

            model.TagMap.RecordCount++;
            return model;
        }
    }
}