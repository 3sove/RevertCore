using System;
using System.Linq;

namespace Revert.Core.Extensions
{
    public static class ObjectExtensions
    {
        public static bool IsDefault<T>(this T obj)
        {
            if (Equals(obj, default(T))) return true;
            if (obj is string) return string.IsNullOrWhiteSpace(obj.ToString());
            return false;
        }

        public static bool IsNullable<T>(this T obj)
        {
            if (obj == null) return true; // obvious
            Type type = typeof(T);
            if (!type.IsValueType) return true; // ref-type
            if (Nullable.GetUnderlyingType(type) != null) return true; // Nullable<T>
            return false; // value-type
        }

        public static bool TryGetTabularHorizontalValue<TKey, TValue>(this object[][] data, TKey key, out TValue value) where TKey : class where TValue : class
        {
            foreach (var row in data)
            {
                if (row.All(r => r == null)) continue;
                var rowKey = row[0] as TKey;
                if (rowKey == default(TKey)) continue;

                if (rowKey == key)
                {
                    value = row[1] as TValue;
                    return true;
                }
            }
            value = default;
            return false;
        }

        /// <summary>
        /// Replaces the value returned from a pointer with another if the initial value equals default(T)
        /// </summary>
        public static T IfDefault<T>(this T item, T replacement)
        {
            if (Equals(item, default(T))) return replacement;
            return item;
        }
    }
}