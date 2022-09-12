using System;
using System.Collections.Generic;
using System.Linq;
using Revert.Core.Common.Modules;
using Revert.Core.Extensions;
using Revert.Core.Text.Tokenization;

namespace Revert.Core.Text.Extraction
{
    public class TagMapper : FunctionalModule<TagMapper, TagMapModel>
    {
        protected override void Execute()
        {
            var model = new TagCountByTokenGeneratorModel
            {
                DirectoryPath = Model.TrainingDocumentAndDirectoryPath,
                TrainingSetItems = Model.TrainingSetItemEnumerable,
                TotalRecordsToParse = Model.TotalRecordsToParse,
                RecordsPerMessage = Model.RecordsPerMessage,
                UpdateMessageAction = Model.UpdateMessageAction,
                Tokenizer = Model.Tokenizer
            };

            Model.TagCountByTokenGeneratorModel = TagCountByTokenGenerator.Instance.Execute(model);
            Model.TagMap = Model.TagCountByTokenGeneratorModel.TagMap;
            GatherStatisticalDetails(Model);
        }

        public void Update(TagMapModel model)
        {
            Model = model;
            GatherStatisticalDetails(Model);
        }

        public TagMapModel UpdateTagMap(TagMapModel model, TrainingSetItem trainingSetItem)
        {
            TagCountByTokenGenerator.Instance.UpdateTagMap(model.TagCountByTokenGeneratorModel, trainingSetItem);
            model.UpdateMessageAction("Updating Statistics");
            GatherStatisticalDetails(model);
            model.UpdateMessageAction(string.Empty);
            return model;
        }

        private void GatherStatisticalDetails(TagMapModel model)
        {
            PublishUpdateMessage("Analyzing Statistical Patterns");
            model.TagMap.TotalCountByTag = GetTokenCountByTag(model);
            model.AverageTokenCount = (model.TagMap.TotalTokenCount + 1) / (model.TagMap.TotalCountByTag.Count + 1);
            model.MedianTokenCountAcrossTags = GetMedianTokenCount(model);

            PublishUpdateMessage("Normalizing Vector Maps");
            model.TagMap.NormalizingFactorByTag = GetNormalizingFactorByTag(model);
            model.TagMap.NormalizedCountByTagByTokenMatrix = GetNormalizedCountByTagByTokenMatrix(model);
            model.TagMap.NormalizedCountByTokenByTagMatrix = GetNormalizedCountByTokenByTagMatrix(model.TagMap.NormalizedCountByTagByTokenMatrix);
            model.TagMap.NormalizedTotalCountByTag = GetNormalizedTokenCountByTag(model);
            model.TagMap.NormalizedTotalCountByToken = GetNormalizedTotalCountByToken(model);

            PublishUpdateMessage("Calculating Final Statistical Distributions");

            model.TagMap.MedianTokenCountByToken = GetMedianTokenCountByToken(model);
            model.TagMap.MeanTokenCountByToken = GetMeanTokenCountByToken(model);
            model.TokenWeightByTokenByTag = GetTokenWeightByTag(model);
        }

        private Dictionary<string, Dictionary<string, float>> GetNormalizedCountByTokenByTagMatrix(Dictionary<string, Dictionary<string, float>> countByTagByTokenMatrix)
        {
            var matrix = new Dictionary<string, Dictionary<string, float>>();

            foreach (var countByTagByToken in countByTagByTokenMatrix)
            {
                foreach (var countByTag in countByTagByToken.Value)
                {
                    Dictionary<string, float> countByToken;
                    var count = 0f;
                    if (!matrix.TryGetValue(countByTag.Key, out countByToken))
                        matrix[countByTag.Key] = countByToken = new Dictionary<string, float>();
                    else
                        countByToken.TryGetValue(countByTagByToken.Key, out count);

                    countByToken[countByTagByToken.Key] = count + countByTag.Value;
                }
            }
            return matrix;
        }

        private Dictionary<string, float> GetNormalizedTotalCountByToken(TagMapModel model)
        {
            var normalizedTotalCountByToken = new Dictionary<string, float>(model.TagMap.TotalCountByToken.Count);

            foreach (var tokenMap in model.TagMap.NormalizedCountByTagByTokenMatrix)
            {
                foreach (var tagMap in tokenMap.Value)
                {
                    float count;
                    normalizedTotalCountByToken.TryGetValue(tokenMap.Key, out count);
                    normalizedTotalCountByToken[tokenMap.Key] = count + tagMap.Value;
                }
            }
            return normalizedTotalCountByToken;
        }

        private Dictionary<string, float> GetMedianTokenCountByToken(TagMapModel model)
        {
            var medianTokenCountByToken = new Dictionary<string, float>();
            foreach (var countByTagByToken in model.TagMap.NormalizedCountByTagByTokenMatrix)
            {
                var countsAcrossTags = countByTagByToken.Value.Values.OrderByDescending(item => item).ToArray();
                var middleCount = (countsAcrossTags.Length - 1) / 2f;
                medianTokenCountByToken[countByTagByToken.Key] = (countsAcrossTags[(int)Math.Floor(middleCount)] + countsAcrossTags[(int)Math.Ceiling(middleCount)]) / 2f;
            }
            return medianTokenCountByToken;
        }

        private Dictionary<string, float> GetMeanTokenCountByToken(TagMapModel model)
        {
            var meanTokenCountByToken = new Dictionary<string, float>();
            foreach (var countByTagByToken in model.TagMap.NormalizedCountByTagByTokenMatrix)
            {
                meanTokenCountByToken[countByTagByToken.Key] = countByTagByToken.Value.Values.Sum() / countByTagByToken.Value.Values.Count;
            }
            return meanTokenCountByToken;
        }

        private Dictionary<string, Dictionary<string, float>> GetNormalizedCountByTagByTokenMatrix(TagMapModel model)
        {
            var normalizedCountByTagByTokenMatrix = new Dictionary<string, Dictionary<string, float>>();
            foreach (var countByTagByToken in model.TagMap.CountByTagByTokenMatrix)
            {
                Dictionary<string, float> normalizedCountByTag;
                if (!normalizedCountByTagByTokenMatrix.TryGetValue(countByTagByToken.Key, out normalizedCountByTag))
                    normalizedCountByTagByTokenMatrix[countByTagByToken.Key] = normalizedCountByTag = new Dictionary<string, float>();

                foreach (var countByTag in countByTagByToken.Value)
                    normalizedCountByTag[countByTag.Key] = countByTag.Value * model.TagMap.NormalizingFactorByTag[countByTag.Key];
            }
            return normalizedCountByTagByTokenMatrix;
        }

        private int GetMedianTokenCount(TagMapModel model)
        {
            var orderedTokenCount = model.TagMap.TotalCountByTag.Select(item => item.Value).OrderBy(item => item).ToList();
            if (orderedTokenCount.Count == 0) return 0;
            return (int)Math.Ceiling(orderedTokenCount[(int)Math.Ceiling((orderedTokenCount.Count - 1) / 2f)] +
                                     orderedTokenCount[(int)Math.Floor((orderedTokenCount.Count - 1) / 2f)] / 2f);
        }

        private Dictionary<string, float> GetNormalizingFactorByTag(TagMapModel model)
        {
            var normalizingFactorByTag = new Dictionary<string, float>();

            foreach (var totalCount in model.TagMap.TotalCountByTag)
            {
                var normalizingFactor = model.MedianTokenCountAcrossTags / (float)totalCount.Value;
                normalizingFactorByTag[totalCount.Key] = normalizingFactor;
            }
            return normalizingFactorByTag;
        }

        private Dictionary<string, int> GetTokenCountByTag(TagMapModel model)
        {
            var tokenCountByTag = new Dictionary<string, int>();
            foreach (var countByTagByToken in model.TagMap.CountByTagByTokenMatrix)
            {
                if (countByTagByToken.Key == null) continue;
                int tokenTotalCount;
                if (!model.TagMap.TotalCountByToken.TryGetValue(countByTagByToken.Key.ToUpper(), out tokenTotalCount)) continue;

                foreach (var countByTag in countByTagByToken.Value)
                {
                    if (countByTag.Key == null) continue;
                    int count;
                    tokenCountByTag.TryGetValue(countByTag.Key, out count);
                    tokenCountByTag[countByTag.Key] = count + countByTag.Value;
                }
            }
            return tokenCountByTag;
        }

        private Dictionary<string, float> GetNormalizedTokenCountByTag(TagMapModel model)
        {
            var tokenCountByTag = new Dictionary<string, float>();
            foreach (var countByTagByToken in model.TagMap.NormalizedCountByTagByTokenMatrix)
            {
                int tokenTotalCount;
                if (!model.TagMap.TotalCountByToken.TryGetValue(countByTagByToken.Key.ToUpper(), out tokenTotalCount)) continue;

                foreach (var countByTag in countByTagByToken.Value)
                {
                    float count;
                    tokenCountByTag.TryGetValue(countByTag.Key, out count);
                    tokenCountByTag[countByTag.Key] = count + countByTag.Value;
                }
            }
            return tokenCountByTag;
        }
        
        public Dictionary<string, List<Tuple<string, float>>> GetTokenWeightByTag(TagMapModel model)
        {
            var tokenFrequenciesByTag = new Dictionary<string, List<Tuple<string, float>>>();
            foreach (var countByTagByToken in model.TagMap.NormalizedCountByTagByTokenMatrix)
            {
                float weightedTokenCount;
                if (!model.TagMap.NormalizedTotalCountByToken.TryGetValue(countByTagByToken.Key.ToUpper(), out weightedTokenCount)) continue;
                var medianTokenCountByTag = model.TagMap.MedianTokenCountByToken[countByTagByToken.Key];
                //var meanTokenCountByTag = model.TagMap.MeanTokenCountByToken[countByTagByToken.KeyOne];
                //var blendedCountByTag = (medianTokenCountByTag + meanTokenCountByTag) / 2;

                foreach (var countByTag in countByTagByToken.Value)
                {
                    var tokenFrequency = countByTag.Value / medianTokenCountByTag; // (blendedCountByTag + countByTag.KeyTwo);

                    List<Tuple<string, float>> tokenFrequencies;
                    if (!tokenFrequenciesByTag.TryGetValue(countByTag.Key, out tokenFrequencies))
                        tokenFrequenciesByTag[countByTag.Key] = tokenFrequencies = new List<Tuple<string, float>>();

                    tokenFrequencies.Add(new Tuple<string, float>(countByTagByToken.Key, tokenFrequency));
                }
            }

            foreach (var key in tokenFrequenciesByTag.Keys.ToArray())
                tokenFrequenciesByTag[key] = tokenFrequenciesByTag[key].OrderByDescending(item => item.Item2).ToList();
            return tokenFrequenciesByTag;
        }

        public List<Tuple<string, float>> GetVectorTags(TagMap tagMap, string textToEvaluate, double probabilityCuttoff,
            ITokenizer tokenizer, out Dictionary<string, Dictionary<string, float>> frequenciesByTokenByTag)
        {
            var tokens = tokenizer.GetTokens(textToEvaluate);
            var evaluationCountByToken = new Dictionary<string, float>();

            foreach (var token in tokens)
            {
                float count;
                float tokenTotalCount;
                double zScore;

                if (!IsTokenValuable(tagMap, token, out tokenTotalCount, out zScore)) continue;

                evaluationCountByToken.TryGetValue(token, out count);
                evaluationCountByToken[token] = ++count;
            }

            var countByTokenByTagVectorMaps = new Dictionary<string, Dictionary<string, float>>();

            foreach (var countByTokenByTag in tagMap.NormalizedCountByTokenByTagMatrix)
            {
                var countByTokenVectorMap = new Dictionary<string, float>();
                countByTokenByTagVectorMaps[countByTokenByTag.Key] = countByTokenVectorMap;

                foreach (var evaluationTokenAndCount in evaluationCountByToken)
                {
                    float mapCount;
                    countByTokenByTag.Value.TryGetValue(evaluationTokenAndCount.Key, out mapCount);
                    countByTokenVectorMap[evaluationTokenAndCount.Key] = mapCount;
                }
            }

            var magnitudeA = Math.Sqrt(evaluationCountByToken.Values.Sum(a => Math.Pow(a, 2)));
            var vectorTags = new List<Tuple<string, float>>();

            frequenciesByTokenByTag = new Dictionary<string, Dictionary<string, float>>();

            foreach (var countByTokenByTag in countByTokenByTagVectorMaps)
            {
                var differenceByToken = evaluationCountByToken.Zip(countByTokenByTag.Value, (a, b) => new Tuple<string, double>(a.Key, Math.Abs(a.Value - b.Value))).ToArray();
                var maxDifferenceByToken = differenceByToken.Max(item => item.Item2) + 1;
                var frequenciesByToken = differenceByToken.ToDictionary(item => item.Item1, item => (float)(1 - (item.Item2 + 1) / maxDifferenceByToken));
                frequenciesByTokenByTag[countByTokenByTag.Key] = frequenciesByToken.OrderByDescending(item => item.Value).ToDictionary(item => item.Key, item => item.Value);

                var vectorProducts = evaluationCountByToken.Values.Zip(countByTokenByTag.Value.Values, (a, b) => (double)a * b);
                var dotProduct = vectorProducts.Sum();
                var magnitudeB = Math.Sqrt(countByTokenByTag.Value.Values.Sum(b => Math.Pow(b, 2)));
                var magnitudeProduct = magnitudeA * magnitudeB;
                var cos = (float)(dotProduct / magnitudeProduct);
                vectorTags.Add(new Tuple<string, float>(countByTokenByTag.Key, cos));
            }
            return vectorTags;
        }

        /// <summary>
        /// Gets tags with their probability as a value between 0.00f and 1.00f
        /// </summary>
        public List<Tuple<string, float>> GetTags(TagMap tagMap, string textToEvaluate, float probabilityCuttoff, ITokenizer tokenizer, out Dictionary<string, Dictionary<string, float>> frequenciesByTokenByTag)
        {
            //var tokenMinimumCount = Math.Log(tagMap.RecordCount); //TODO: Evaluate the logic behind the minimum token count construct

            var frequenciesByTag = new Dictionary<string, List<float>>();
            frequenciesByTokenByTag = new Dictionary<string, Dictionary<string, float>>();
            var tokens = tokenizer.GetTokens(textToEvaluate);

            var localCountByToken = new Dictionary<string, int>();
            foreach (var token in tokens)
            {
                int tokenCount;
                localCountByToken.TryGetValue(token, out tokenCount);
                localCountByToken[token] = ++tokenCount;
            }

            var zScores = new Dictionary<string, double>();

            foreach (var token in tokens)
            {
                //Ensure the token isn't super rare (probably mis-spellings)
                float tokenTotalCount;
                double zScore;
                if (!IsTokenValuable(tagMap, token, out tokenTotalCount, out zScore)) continue;
                zScores[token] = zScore;

                //if (tokenTotalCount < tokenMinimumCount) continue;

                //Get tag counts for this token
                Dictionary<string, float> countByTagInMap;
                if (!tagMap.NormalizedCountByTagByTokenMatrix.TryGetValue(token, out countByTagInMap))
                {
                    continue;
                    //countByTagByTokenInMap[token] = countByTagInMap = new Dictionary<string, int>();
                }

                //loop through tag counts
                double tokenTotalCountAsDouble = tokenTotalCount + countByTagInMap.Count;
                foreach (var item in countByTagInMap)
                {
                    var tokenLocalCount = (float)localCountByToken[token];
                    List<float> frequencies;
                    if (!frequenciesByTag.TryGetValue(item.Key, out frequencies)) frequenciesByTag[item.Key] = frequencies = new List<float>();
                    var frequency = (float)((item.Value + 1) / (tokenTotalCountAsDouble + 1));

                    //var tf = (tokenLocalCount / maxFrequency);

                    //var relativeFrequencyProduct = 1f * frequency * tf; //p p_i
                    //var inverseRelativeFrequencyProduct = 1f * (1 - frequency) * (1 - tf); //p 1-p_i
                    //frequency = relativeFrequencyProduct / (inverseRelativeFrequencyProduct + relativeFrequencyProduct);

                    //frequency = 1 - (1 - frequency) * (1 - tf);

                    //frequency = (float)(1 - Math.Pow(1 - frequency, tokenLocalCount));


                    //var tf = 0.5 + ((0.5 * tokenLocalCount) / maxFrequency);
                    //var idf = Math.Log(tagMap.NormalizedTotalTokenCount / tagMap.NormalizedTotalCountByTag[item.KeyOne]);
                    //var tfidf = tf*idf;
                    //frequency = (float)tfidf;
                    //var medianTokenCount = tagMap.MedianTokenCountByToken[token];
                    //var tagCount = tagMap.TotalCountByTag[item.KeyOne];
                    //var weightedTokenLocalCount = tokenLocalCount*(tagCount/tokens.GetCount);

                    frequencies.Add(frequency);

                    Dictionary<string, float> frequencyByToken;
                    if (!frequenciesByTokenByTag.TryGetValue(item.Key, out frequencyByToken)) frequenciesByTokenByTag[item.Key] = frequencyByToken = new Dictionary<string, float>();
                    float frequencyByTokenByTag;
                    if (!frequencyByToken.TryGetValue(token, out frequencyByTokenByTag)) frequencyByToken[token] = frequency;
                }
            }

            //zScores = zScores.OrderByDescending(d => d.KeyTwo).ToDictionary(pair => pair.KeyOne, pair => pair.KeyTwo);

            var sortedProbabilities = new List<Tuple<string, float>>();

            foreach (var tag in frequenciesByTokenByTag.Keys.ToArray())
            {
                var item = frequenciesByTokenByTag[tag];
                frequenciesByTokenByTag[tag] = item.OrderByDescending(dic => dic.Value).ToDictionary(dic => dic.Key, dic => dic.Value);
            }

            foreach (var item in frequenciesByTag)
            {
                //(int)Math.Ceiling(item.KeyTwo.GetCount * 0.1)
                //var relativeFrequencies = item.KeyTwo.OrderByDescending(f => f).ToList(); //(int)Math.Ceiling(item.KeyTwo.GetCount * 0.1f)).ToList();
                var relativeFrequencies = item.Value.OrderByDescending(f => f).Take(15).ToList(); //(int)Math.Ceiling(item.KeyTwo.GetCount * 0.1f)).ToList();
                var relativeFrequencyProduct = 1f; //p p_i
                relativeFrequencies.ForEach(fq => relativeFrequencyProduct *= fq);
                var inverseRelativeFrequencyProduct = 1f; //p 1-p_i
                relativeFrequencies.ForEach(fq => inverseRelativeFrequencyProduct *= (1 - fq));
                sortedProbabilities.Add(new Tuple<string, float>(item.Key, relativeFrequencyProduct / (inverseRelativeFrequencyProduct + relativeFrequencyProduct)));
            }


            sortedProbabilities = sortedProbabilities.OrderByDescending(item => item.Item2).ToList();
            return sortedProbabilities;
        }

        private bool IsTokenValuable(TagMap tagMap, string token, out float tokenTotalCount, out double zScore)
        {
            if (tagMap.NormalizedTotalCountByToken.TryGetValue(token, out tokenTotalCount))
            {
                zScore = (tokenTotalCount - tagMap.MeanTokenCount) / tagMap.TokenCountStandardDeviation;
                if (zScore > 2) //TODO: Test alternative zScore thresholds
                {
                    PublishUpdateMessage("Ignoring common token {0}", token);
                    return false;
                }

                if (zScore >= -0.035) return true;

                PublishUpdateMessage("Ignoring rare token {0}", token);
                return false;
            }

            zScore = 0;
            return false;
        }

        /// <summary>
        /// Gets tags with their probability as a value between 0.00f and 1.00f
        /// </summary>
        public List<Tuple<string, float>> GetTagsLite(TagMap tagMap, string textToEvaluate, float probabilityCuttoff, ITokenizer tokenizer)
        {
            var countByTagByToken = tagMap.CountByTagByTokenMatrix;
            var tokenMinimumCount = Math.Log(tagMap.RecordCount); //TODO: Evaluate the logic behind the minimum token count construct

            var frequenciesByTag = new Dictionary<string, List<float>>();

            var tokens = tokenizer.GetTokens(textToEvaluate);

            var countByToken = new Dictionary<string, int>();
            foreach (var token in tokens)
                countByToken.IncrementValue(token);

            foreach (var item in countByToken)
            {
                //Ensure the token isn't super rare (probably mis-spellings)
                int tokenTotalCount;
                if (!tagMap.TotalCountByToken.TryGetValue(item.Key, out tokenTotalCount)) continue;
                if (tokenTotalCount < tokenMinimumCount) continue;

                //Get tag counts for this token
                Dictionary<string, int> countByTags;
                if (!countByTagByToken.TryGetValue(item.Key, out countByTags))
                {
                    countByTagByToken[item.Key] = countByTags = new Dictionary<string, int>();
                }

                //loop through tag counts
                double tokenTotalCountAsFloat = tokenTotalCount + (countByTags.Count * item.Value);
                foreach (var countByTag in countByTags)
                {
                    var frequency = (float)((countByTag.Value + item.Value) / tokenTotalCountAsFloat);
                    frequenciesByTag.AddToCollection(countByTag.Key, frequency);
                }
            }

            var sortedProbabilities = new List<Tuple<string, float>>();

            foreach (var item in frequenciesByTag)
            {
                //(int)Math.Ceiling(item.KeyTwo.GetCount * 0.1)
                var relativeFrequencies = item.Value.OrderByDescending(f => f).Take(15).ToArray(); //(int)Math.Ceiling(item.KeyTwo.GetCount * 0.1f)).ToList();
                var relativeFrequencyProduct = 1f; //p p_i
                
                for (int i = 0; i < relativeFrequencies.Length; i++)
                {
                    var fq = relativeFrequencies[i];
                    relativeFrequencyProduct *= fq;
                }

                var inverseRelativeFrequencyProduct = 1f; //p 1-p_i

                for (int i = 0; i < relativeFrequencies.Length; i++)
                {
                    var fq = relativeFrequencies[i];
                    inverseRelativeFrequencyProduct *= (1 - fq);
                }

                var probability = relativeFrequencyProduct / (inverseRelativeFrequencyProduct + relativeFrequencyProduct);
                if (probability < probabilityCuttoff) continue;
                sortedProbabilities.Add(new Tuple<string, float>(item.Key, probability));
            }

            sortedProbabilities = sortedProbabilities.OrderByDescending(item => item.Item2).ToList();
            return sortedProbabilities;
        }
    }
}
