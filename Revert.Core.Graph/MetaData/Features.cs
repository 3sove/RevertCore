using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using MongoDB.Bson;
using Revert.Core.Extensions;
using Revert.Core.Graph.Vertices;
using Revert.Core.MachineLearning;
using Revert.Core.Text.Tokenization;
using Revert.Core.Mathematics.Extensions;
using Revert.Core.Graph.MetaData.DataPoints;
using Revert.Core.IO;

namespace Revert.Core.Graph.MetaData
{

    [DataContract(IsReference = true)]
    [DebuggerDisplay("{GetFeaturesDebuggerDisplay(),nq}")]
    public class Features<TVertex> : Features where TVertex : class, IVertex, IMongoRecord, new()
    {

        private string resolvableString = string.Empty;
        public string GetResolvableString(Graph<TVertex> graph)
        {
            if (!string.IsNullOrWhiteSpace(resolvableString)) return resolvableString;
            return GetResolvableTokens(graph).Select(t => t.Value).Combine(" ");
        }

        public IEnumerable<Token> GetResolvableTokens(Graph<TVertex> graph)
        {
            return GetResolvableTokens(graph.TokenIndex, graph.StopList);
        }
    }

    public class Features
    {
        [DataMember]
        public string EntityType { get; set; }

        private static readonly HashSet<string> blackListedEntityTypes = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

        [IgnoreDataMember]
        public bool IncludeInRollup => !blackListedEntityTypes.Contains(EntityType);


        [DataMember]
        [DebuggerDisplay("{GetDateDataDebuggerDisplay(),nq}")]
        public HashSet<DateDataPoint> DateData { get; set; } = new HashSet<DateDataPoint>();

        [DataMember]
        [DebuggerDisplay("{GetBooleanDataDebuggerDisplay(),nq}")]
        public HashSet<BooleanDataPoint> BooleanData { get; set; } = new HashSet<BooleanDataPoint>();

        [DataMember]
        [DebuggerDisplay("{GetTextDataDebuggerDisplay(),nq}")]
        public HashSet<TextDataPoint> TextData { get; set; } = new HashSet<TextDataPoint>();

        [DataMember]
        [DebuggerDisplay("{GetBinaryDebuggerDisplay(),nq}")]
        public HashSet<BinaryDataPoint> BinaryData { get; set; } = new HashSet<BinaryDataPoint>();

        [DataMember]
        public HashSet<DiscreteDataPoint> DiscreteData { get; set; } = new HashSet<DiscreteDataPoint>();

        [DataMember]
        public HashSet<ContinuousDataPoint> ContinuousData { get; set; } = new HashSet<ContinuousDataPoint>();

        [DataMember]
        public HashSet<ContinuousTimeSeriesDataPoint> ContinuousTimeSeriesData { get; set; } = new HashSet<ContinuousTimeSeriesDataPoint>();

        [DataMember]
        public HashSet<DiscreteTimeSeriesDataPoint> DiscreteTimeSeriesData { get; set; } = new HashSet<DiscreteTimeSeriesDataPoint>();

        [DataMember]
        public HashSet<TextTimeSeriesDataPoint> TextTimeSeriesData { get; set; } = new HashSet<TextTimeSeriesDataPoint>();

        [DataMember]
        public string Name
        {
            get { return TextData.FirstOrDefault(t => t.Key == "Name")?.Value ?? string.Empty; }
            set => AddText("Name", value, true, true);
        }

        [DataMember]
        public DateTime LastUpdated
        {
            get
            {
                return DateData.FirstOrDefault(data => data.Key == "Last Updated")?.Value ?? (LastUpdated = DateTime.Now);
                //return TextTimeSeriesData.Where(t => t.KeyOne == "Updates").Select(t => t.KeyTwo.Item1).OrderByDescending(t => t).FirstOrDefault();
            }
            set
            {
                DateData.Add(new DateDataPoint("Last Updated", value));
                //TextTimeSeriesData.Upsert(new TextTimeSeriesDataPoint("Updates", new Tuple<DateTime, string>(value, "Last Updated")));
            }
        }

        private string GetResolvableData()
        {
            var sbCombiner = new StringBuilder();
            DiscreteData.Where(d => d.IsResolvable).Select(item => item.Value).ForEach(item => sbCombiner.AppendLine(item.ToString()));
            ContinuousData.Where(d => d.IsResolvable).Select(item => item.Value).ForEach(item => sbCombiner.AppendLine(item.ToString(CultureInfo.InvariantCulture)));
            TextData.Where(d => d.IsResolvable).Select(item => item.Value).ForEach(item => sbCombiner.AppendLine(item));

            return sbCombiner.ToString();
        }

        //TODO: Fix this name - it's more like searchable tokens by id with count, but that's too long
        private Dictionary<ObjectId, int> searchableTokensByCount = null;
        public Dictionary<ObjectId, int> GetSearchableTokensWithCount(TokenIndex tokenIndex, HashSet<string> stopList)
        {
            var searchableText = GetSearchableText();
            return searchableTokensByCount ?? (searchableTokensByCount = tokenIndex.GetTokenIdsWithCount(searchableText, true, stopList));
        }

        public ObjectId[] GetOrderedSearchableTokens(TokenIndex tokenIndex, HashSet<string> stopList)
        {
            var searchableText = GetSearchableText();
            return tokenIndex.GetTokenIds(searchableText, stopList);
        }


        /// <summary>
        /// Returns true if similarity is > 80%
        /// </summary>
        public virtual float CalculateFeatureSimilarity(IVertex entity1, IVertex entity2, TokenIndex tokenIndex, HashSet<string> stopList)
        {
            if (entity1.Id == entity2.Id)
            {
                return 1f;
            }

            if (entity1.Features == null)
            {
                return 0f;
            }

            var entity1Tokens = entity1.Features.GetResolvableTokens(tokenIndex, stopList);
            var entity2Tokens = entity2.Features.GetResolvableTokens(tokenIndex, stopList);

            if (entity1Tokens == null || entity2Tokens == null || !entity1Tokens.Any() || !entity2Tokens.Any())
            {
                return 0f;
            }

            var similarity = CalculateFeatureSimilarity(entity1Tokens, entity2Tokens);

            //TODO: Fix similarity comparison in CalculateFeatureSimilarity
            return similarity;
        }

        public float CalculateFeatureSimilarity(IEnumerable<Token> f1Tokens, IEnumerable<Token> f2Tokens)
        {
            Dictionary<ObjectId, Token> f1TokensById = new Dictionary<ObjectId, Token>();
            foreach (var token in f1Tokens)
                f1TokensById[token.Id] = token;

            Dictionary<ObjectId, Token> f2TokensById = new Dictionary<ObjectId, Token>();
            HashSet<Token> set = new HashSet<Token>();

            foreach (var token in f2Tokens)
                if (set.Add(token)) f2TokensById[token.Id] = token;

            // TODO: Get similar tokens

            var tokenLookup = f1TokensById.Values.Union(f2TokensById.Values).ToDiscreteVector(TokenComparers.OrdinalIgnoreCaseTokenComparer);

            int position = 0;
            int[] f1Vector = new int[tokenLookup.Count];
            int[] f2Vector = new int[tokenLookup.Count];

            foreach (var item in tokenLookup)
            {
                f1Vector[position] = f1TokensById.ContainsKey(item.Key.Id) ? 1 : 0;
                f2Vector[position] = f2TokensById.ContainsKey(item.Key.Id) ? 1 : 0;
                position++;
            }

            return CosineSimilarity.GetVectorCosSimilarity(f1Vector, f2Vector);
        }

        public int GetTextDataLength()
        {
            return TextData.Sum(item => item.Value.Length);
        }

        public float ContentLength => GetTextDataLength();
        public string GetFeaturesDebuggerDisplay()
        {
            return SummarizeDataPoints();
        }

        public string GetBinaryDebuggerDisplay()
        {
            return BinaryData.Select(item => $"Binary Data: {(item.IsSearchable ? "" : "* un-searchable * ")}" +
                $"{item.Key}: {item.Value.Length.ToString("#,#")} bytes").Combine(" | ");
        }

        public string GetBooleanDataDebuggerDisplay()
        {
            return BooleanData.Select(item => $"Boolean Data: {(item.IsSearchable ? "" : "* un-searchable *")}" +
                $"{item.Key}: {item.Value}").Combine(" | ");
        }

        public string GetDateDataDebuggerDisplay()
        {
            return DateData.Select(item => $"Date Data: {(item.IsSearchable ? "" : "* un-searchable *")}" +
                $"{item.Key}: {item.Value}").Combine(" | ");
        }

        public string GetTextDataDebuggerDisplay()
        {
            return TextData.Select(item => $"{(item.IsSearchable ? "" : "* un-searchable *")}" +
                                           $"{item.Key}: {item.Value.Truncate(30, true, true, true)}").Combine(" | ");
        }

        public string GetHtmlFormattedDataPoints()
        {
            var value = (DiscreteData.Any() ? DiscreteData.Select(item => "<b>" + item.Key + "</b>" + ": " + item.Value + "<br/>").Combine("<br/>") : "") +
                   (ContinuousData.Any() ? ContinuousData.Select(item => "<b>" + item.Key + "</b>" + ": " + item.Value + "<br/>").Combine("<br/>") : "") +
                   (TextData.Any() ? "<br/><h2>Text Data</h2>" + TextData.Where(t => !string.IsNullOrWhiteSpace(t.Value)).Select(item => "<b>" + item.Key + "</b>" + ": " + item.Value.RemoveDoubleCarriageReturns() + "<br/>").Combine("<br/>") : "");

            return value.Replace("\r\n", "<br />").Replace("\n", "<br />");
        }

        public TextDataPoint GetText(string key)
        {
            return TextData.FirstOrDefault(item => string.Equals(key, item.Key, StringComparison.InvariantCultureIgnoreCase));
        }

        public string GetSearchableText()
        {
            var text = new StringBuilder();
            text.AppendLine(EntityType);
            TextData.Where(item => item.IsSearchable).ForEach(item =>
            {
                text.AppendLine(item.Key);
                text.AppendLine(item.Value);
            });

            ContinuousData.Where(item => item.IsSearchable).ForEach(item =>
            {
                text.AppendLine(item.Key);
                text.AppendLine(item.Value.ToString(CultureInfo.InvariantCulture));
            });

            DiscreteData.Where(item => item.IsSearchable).ForEach(item =>
            {
                text.AppendLine(item.Key.ToString());
                text.AppendLine(item.Value.ToString());
            });

            TextTimeSeriesData.Where(item => item.IsSearchable).ForEach(item =>
            {
                text.AppendLine(item.Value.Item1.ToString("dd M yyyy"));
                text.AppendLine(item.Value.Item2);
            });

            DiscreteTimeSeriesData.Where(item => item.IsSearchable).ForEach(item =>
            {
                text.AppendLine(item.Key);
                text.AppendLine(item.Value.ToString());
            });

            ContinuousTimeSeriesData.Where(item => item.IsSearchable).ForEach(item =>
            {
                text.AppendLine(item.Key);
                text.AppendLine(item.Value.ToString());
            });


            return text.ToString();
        }

        public string SummarizeDataPoints(bool includeName = false)
        {
            if (TextData == null || !TextData.Any()) return string.Empty;
            var dataPoints = TextData.Where(item => !string.IsNullOrWhiteSpace(item.Value));
            if (!includeName) dataPoints = dataPoints.Where(item => !string.Equals(item.Key, "Name", StringComparison.OrdinalIgnoreCase));
            return dataPoints.Select(item => $"{item.Key}: {item.Value.RemoveDoubleCarriageReturns().TrimCarriageReturns().RemoveDoubleSpaces().Trim()}").Combine(Environment.NewLine);
        }

        public float CalculateEntropy(float longestDataPointSummaryValue)
        {
            return TextData.Sum(item => item.Value.Length) / longestDataPointSummaryValue;
        }

        private void SetUpdated(DateTime dateTime, string message)
        {
            //TextTimeSeriesData.Upsert(new TextTimeSeriesDataPoint("Updates", new Tuple<DateTime, string>(dateTime, message)));
            DateData.AddIgnoreEmpty("Update", dateTime);
        }

        public void SetUpdated(string message)
        {
            SetUpdated(DateTime.Now, message);
        }

        public void SetUpdated(string message, string userName)
        {
            var updatedDateTime = DateTime.Now;
            SetUpdated(updatedDateTime, message);
            TextTimeSeriesData.Add(new TextTimeSeriesDataPoint("Updated By", new Tuple<DateTime, string>(updatedDateTime, userName)));
        }



        public DiscreteDataPoint AddDiscrete(string key, int value, bool resolvable = false, bool searchable = true)
        {
            var dataPoint = new DiscreteDataPoint(key, value)
            {
                IsResolvable = resolvable,
                IsSearchable = searchable
            };
            DiscreteData.Add(dataPoint);
            return dataPoint;
        }

        public ContinuousDataPoint AddContinuous(string key, float value, bool resolvable = false, bool searchable = true)
        {
            var dataPoint = new ContinuousDataPoint(key, value)
            {
                IsResolvable = resolvable,
                IsSearchable = searchable
            };
            ContinuousData.Add(dataPoint);
            return dataPoint;
        }

        public TextDataPoint AddText(string key, string value, bool resolvable = false, bool searchable = true)
        {
            var dataPoint = new TextDataPoint(key, value)
            {
                IsResolvable = resolvable,
                IsSearchable = searchable
            };
            TextData.Add(dataPoint);
            return dataPoint;
        }

        public DateDataPoint AddDate(string key, DateTime value, bool resolvable = false, bool searchable = true)
        {
            //var dataPoint = new TextTimeSeriesDataPoint("Dates", new Tuple<DateTime, string>(value, key))
            //{
            //    IsResolvable = resolvable,
            //    IsSearchable = searchable
            //};

            var dataPoint = new DateDataPoint(key, value) { IsResolvable = resolvable, IsSearchable = searchable };

            DateData.Add(dataPoint);

            //TextTimeSeriesData.Upsert(dataPoint);
            return dataPoint;
        }

        protected IEnumerable<Token> tokens;
        public IEnumerable<Token> GetResolvableTokens(TokenIndex tokenIndex, HashSet<string> stopList)
        {
            return tokens ?? (tokens = tokenIndex.GetTokens(GetResolvableData(), stopList)).Where(t => t.IsMeaningful).Distinct(TokenComparer.Instance);
        }

        public void Merge(Features features)
        {
            foreach (var item in features.TextData)
                if (!TextData.Contains(item))
                    TextData.Add(item);

            foreach (var item in features.DiscreteData)
                if (!DiscreteData.Contains(item))
                    DiscreteData.Add(item);

            foreach (var item in features.ContinuousData)
                if (!ContinuousData.Contains(item))
                    ContinuousData.Add(item);

            foreach (var item in features.BooleanData)
                if (!BooleanData.Contains(item))
                    BooleanData.Add(item);

            foreach (var item in features.TextTimeSeriesData)
                if (!TextTimeSeriesData.Contains(item))
                    TextTimeSeriesData.Add(item);

            foreach (var item in features.DiscreteTimeSeriesData)
                if (!DiscreteTimeSeriesData.Contains(item))
                    DiscreteTimeSeriesData.Add(item);

            foreach (var item in features.ContinuousTimeSeriesData)
                if (!ContinuousTimeSeriesData.Contains(item))
                    ContinuousTimeSeriesData.Add(item);
        }

    }
}
