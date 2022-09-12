using System.Collections.Generic;
using Revert.Core.Common.Modules;
using Revert.Core.Common.Performance;
using Revert.Core.Extensions;
using Revert.Core.Text.Tokenization;
using ProtoBuf;
using Revert.Core.Mathematics;

namespace Revert.Core.Text.Extraction
{
    //[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class TagCountByTokenGeneratorModel : ModuleModel
    {
        public TagMap TagMap { get; set; }
        public IEnumerable<TrainingSetItem> TrainingSetItems { get; set; }
        public int TotalRecordsToParse { get; set; } = int.MaxValue;

        private ITokenizer tokenizer;
        public ITokenizer Tokenizer
        {
            get { return tokenizer ?? (tokenizer = new SimpleTokenizer()); }
            set { tokenizer = value; }
        }

        public string DirectoryPath { get; set; }
    }

    public class TagCountByTokenGenerator : FunctionalModule<TagCountByTokenGenerator, TagCountByTokenGeneratorModel>
    {
        protected override void Execute()
        {
            var countByToken = new Dictionary<string, int>();
            var countByTagByToken = new Dictionary<string, Dictionary<string, int>>();

            var recordCountByTag = new Dictionary<string, int>();

            var performanceMonitor = new PerformanceMonitor("Training Set Population", Model.RecordsPerMessage, Model.TotalRecordsToParse, Model.UpdateMessageAction);

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
                if (performanceMonitor.Tick() == Model.TotalRecordsToParse) break;
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

            model.UpdateMessageAction($"Updating model with new training set item: {trainingSetItem.Source.OrIfEmpty("User Input Text")}");
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