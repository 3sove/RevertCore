using System.Collections.Generic;
using Revert.Core.Graph.MetaData;
using Revert.Core.Graph.MetaData.DataPoints;

namespace Revert.Core.Graph
{
    public static class Extensions
    {

        /// <summary>
        /// Ignores data points with empty keys
        /// </summary>
        public static void Add<TDataPoint, TKey, TValue>(this List<TDataPoint> collection, TKey key, TValue value, bool isResolvable = false, bool isSearchable = true) where TDataPoint : IDataPoint<TKey, TValue>, new()
        {
            if (key == null || Equals(key, default(TKey))) return;
            collection.Add(new TDataPoint { Key = key, Value = value, IsResolvable = isResolvable, IsSearchable = isSearchable });
        }

        public static void Add<TDataPoint, TKey, TValue>(this HashSet<TDataPoint> collection, TKey key, TValue value, bool isResolvable = false, bool isSearchable = true) where TDataPoint : IDataPoint<TKey, TValue>, new()
        {
            if (key == null || Equals(key, default(TKey))) return;
            collection.Add(new TDataPoint { Key = key, Value = value, IsResolvable = isResolvable, IsSearchable = isSearchable });
        }

        /// <summary>
        /// Ignores data points with empty keys or empty values
        /// </summary>
        public static void AddIgnoreEmpty<TDataPoint, TKey, TValue>(this HashSet<TDataPoint> collection, TKey key, TValue value, bool isResolvable = false, bool isSearchable = true) where TDataPoint : IDataPoint<TKey, TValue>, new()
        {
            if (key == null || Equals(key, default(TKey))) return;
            if (value == null || Equals(value, default(TValue))) return;
            collection.Add(new TDataPoint { Key = key, Value = value, IsResolvable = isResolvable, IsSearchable = isSearchable });
        }
    }
}
