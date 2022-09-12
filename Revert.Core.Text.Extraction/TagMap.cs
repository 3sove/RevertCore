using System;
using System.Collections.Generic;
using System.Linq;
using Revert.Core.Mathematics.Extensions;

namespace Revert.Core.Text.Extraction
{
    //[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class TagMap
    {
        public Dictionary<string, Dictionary<string, int>> CountByTagByTokenMatrix { get; set; }
        public Dictionary<string, Dictionary<string, float>> NormalizedCountByTagByTokenMatrix { get; set; }
        public Dictionary<string, Dictionary<string, float>> NormalizedCountByTokenByTagMatrix { get; set; }
        public Dictionary<string, int> TotalCountByToken { get; set; }
        public Dictionary<string, float> NormalizedTotalCountByToken { get; set; }
        public Dictionary<string, int> TotalCountByTag { get; set; }
        public Dictionary<string, float> NormalizedTotalCountByTag { get; set; }

        public Dictionary<string, float> NormalizingFactorByTag { get; set; }
        public Dictionary<string, float> MedianTokenCountByToken { get; set; }
        public Dictionary<string, float> MeanTokenCountByToken { get; set; }

        private int totalTokenCount;
        public int TotalTokenCount
        {
            get { return totalTokenCount == 0 ? totalTokenCount = TotalCountByTag.Sum(item => item.Value) : totalTokenCount; }
            set { totalTokenCount = value; }
        }

        private float normalizedTotalTokenCount;
        public float NormalizedTotalTokenCount
        {
            get { return Math.Abs(normalizedTotalTokenCount - 0f) < float.Epsilon ? normalizedTotalTokenCount = TotalCountByTag.Sum(item => item.Value) : normalizedTotalTokenCount; }
            set { normalizedTotalTokenCount = value; }
        }

        private double medianTokenCount;
        public double MedianTokenCount
        {
            get
            {
                return Math.Abs(medianTokenCount - default(double)) < double.Epsilon ?
                    medianTokenCount = TotalCountByToken.Select(item => item.Value).Median() :
                    medianTokenCount;
            }
            set { medianTokenCount = value; }
        }

        private double meanTokenCount;
        public double MeanTokenCount
        {
            get
            {
                if (Math.Abs(meanTokenCount - default(double)) < double.Epsilon)
                    tokenCountStandardDeviation = TotalCountByToken.Select(item => item.Value).StandardDeviation(out meanTokenCount);
                return meanTokenCount;
            }
            set { meanTokenCount = value; }
        }

        private double normalizedMeanTokenCount;
        public double NormalizedMeanTokenCount
        {
            get
            {
                if (Math.Abs(normalizedMeanTokenCount - default(double)) < double.Epsilon)
                    tokenCountStandardDeviation = NormalizedTotalCountByToken.Select(item => item.Value).StandardDeviation(out normalizedMeanTokenCount);
                return normalizedMeanTokenCount;
            }
            set { normalizedMeanTokenCount = value; }
        }

        private double tokenCountStandardDeviation;
        public double TokenCountStandardDeviation
        {
            get
            {
                if (Math.Abs(tokenCountStandardDeviation - default(double)) < double.Epsilon)
                    tokenCountStandardDeviation = TotalCountByToken.Select(item => item.Value).StandardDeviation(out meanTokenCount);
                return tokenCountStandardDeviation;
            }
            set { tokenCountStandardDeviation = value; }
        }

        private double normalizedTokenCountStandardDeviation;
        public double NormalizedTokenCountStandardDeviation
        {
            get
            {
                if (Math.Abs(normalizedTokenCountStandardDeviation - default(double)) < double.Epsilon)
                    normalizedTokenCountStandardDeviation = NormalizedTotalCountByToken.Select(item => item.Value).StandardDeviation(out normalizedMeanTokenCount);
                return normalizedTokenCountStandardDeviation;
            }
            set { normalizedTokenCountStandardDeviation = value; }
        }

        public int RecordCount { get; set; }

        public Dictionary<string, int> RecordCountByTag = new Dictionary<string, int>();
    }
}