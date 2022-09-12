using Revert.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Revert.Core.Mathematics.Extensions
{
    public static class StatsExtensions
    {
        public static Dictionary<string, Dictionary<string, int>> ValuesBySetName { get; set; } = new Dictionary<string, Dictionary<string, int>>(StringComparer.OrdinalIgnoreCase);

        public static Dictionary<string, int> MaxValuesBySetName { get; set; } = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        public static Dictionary<TKey, int> ToFrequencyDistribution<TItem, TKey>(this IEnumerable<TItem> items, Func<TItem, TKey> keySelector)
        {
            var dic = new Dictionary<TKey, int>();

            foreach (var item in items)
            {
                if (item == null) continue;
                var key = keySelector(item);
                if (key == null) continue;
                int count;
                dic.TryGetValue(key, out count);
                dic[key] = ++count;
            }
            return dic.OrderBy(item => item.Value).ToDictionary(item => item.Key, item => item.Value);
        }

        public static Dictionary<T, int> ToDiscreteVector<T>(this IEnumerable<T> items, IEqualityComparer<T> equalityComparer)
        {
            var currentId = 0;
            var values = new Dictionary<T, int>(equalityComparer);

            foreach (var item in items)
            {
                int id;
                if (!values.TryGetValue(item, out id))
                    values[item] = currentId++;
            }
            return values;
        }

        public static int GetVectorIndex<T>(this T value, Dictionary<T, int> discreteVector)
        {
            int index;
            if (!discreteVector.TryGetValue(value, out index))
                throw new Exception("Discrete vector not properly formed: value not found in vector");
            return index;
        }

        public static double Median(this IEnumerable<int> source)
        {
            return source.ToArray().Median();
        }

        public static double Median(this int[] source)
        {
            if (source == null) throw new NullReferenceException("Array passed in SelectKth was null.");

            int from = 0, to = source.Length - 1;
            var even = (source.Length % 2) == 0;
            var k = source.Length / 2;

            while (from < to)
            {
                int r = from, w = to;
                var mid = source[(r + w) / 2];

                while (r < w)
                {
                    if (source[r] >= mid)
                    {
                        var tmp = source[w];
                        source[w] = source[r];
                        source[r] = tmp;
                        w--;
                    }
                    else
                    {
                        r++;
                    }
                }

                if (source[r] > mid) r--;
                if (k <= r) to = r;
                else from = r + 1;
            }

            return (even) ? (source[k] + source[k - 1]) * 0.5 : source[k];
        }

        public static double StandardDeviation(this int[] source)
        {
            double mean;
            return source.StandardDeviation(out mean);
        }

        public static double StandardDeviation(this int[] source, out double mean)
        {
            double sum = source.Aggregate<int, double>(0, (current, t) => current + t);
            var average = mean = sum / source.Length;
            var dividend = source.Sum(item => Math.Pow((item - average), 2));
            var divisor = source.Length - 1;
            var variance = dividend / divisor;
            return Math.Sqrt(variance);
        }

        public static double StandardDeviation(this float[] source, out double mean)
        {
            double sum = source.Aggregate<float, double>(0, (current, t) => current + t);
            var average = mean = sum / source.Length;
            var dividend = source.Sum(item => Math.Pow((item - average), 2));
            var divisor = source.Length - 1;
            var variance = dividend / divisor;
            return Math.Sqrt(variance);
        }

        public static double StandardDeviation(this double[] source, out double mean)
        {
            double sum = source.Aggregate<double, double>(0, (current, t) => current + t);
            var average = mean = sum / source.Length;
            var dividend = source.Sum(item => Math.Pow((item - average), 2));
            var divisor = source.Length - 1;
            var variance = dividend / divisor;
            return Math.Sqrt(variance);
        }

        public static double StandardDeviation(this IEnumerable<int> source)
        {
            double mean;
            return source.ToArray().StandardDeviation(out mean);
        }

        public static double StandardDeviation(this IEnumerable<int> source, out double mean)
        {
            return source.ToArray().StandardDeviation(out mean);
        }

        public static double StandardDeviation(this IEnumerable<float> source, out double mean)
        {
            return source.ToArray().StandardDeviation(out mean);
        }

        public static double StandardDeviation(this IEnumerable<double> source, out double mean)
        {
            return source.ToArray().StandardDeviation(out mean);
        }
    }
}