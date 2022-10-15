using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Revert.Core.Common.Types;
using Revert.Core.Common.Types.Tries;

namespace Revert.Core.Extensions
{
    public static class CollectionExtensions
    {
        public static T[] fill<T>(this T[] array, T value)
        {
            for (int x = 0; x < array.Length; x++)
                array[x] = value;
            return array;
        }

        public static T[] fill<T>(this T[] array, Func<int, T> fillFunction)
        {
                for (int x = 0; x < array.Length; x++)
                    array[x] = fillFunction(x);
            return array;
        }


        public static T[][] fill<T>(this T[][] array, T value)
        {
            for (int y = 0; y < array.Length; y++)
                for (int x = 0; x < array[y].Length; x++)
                    array[y][x] = value;
            return array;
        }

        public static T[][] fill<T>(this T[][] array, Func<int, int, T> fillFunction)
        {
            for (int y = 0; y < array.Length; y++)
                for (int x = 0; x < array[y].Length; x++)
                    array[y][x] = fillFunction(y, x);
            return array;
        }

        public static void Shuffle<T>(this IList<T> list, Random rnd)
        {
            for (var i = 0; i < list.Count - 1; i++)
                list.Swap(i, rnd.Next(i, list.Count));
        }

        public static void Swap<T>(this IList<T> list, int i, int j)
        {
            var temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }

        public static T[][] copy<T>(this T[][] array)
        {
            var copyArray = new T[array.Length][];
            for (int i = 0; i < array.Length; i++)
            {
                T[] row = array[i];
                copyArray[i] = row.copy();
            }
            return copyArray;
        }

        public static T[] copy<T>(this T[] array)
        {
            var copyArray = new T[array.Length];
            Array.Copy(array, 0, copyArray, 0, array.Length);
            return copyArray;
        }


        public static int MaxIndex(this int[] array)
        {
            var max = 0;
            var maxIndex = 0;
            for (int i = 0; i < array.Length; i++)
                if (array[i] > max)
                {
                    maxIndex = i;
                    max = array[i];
                }
            return maxIndex;
        }

        public static int MaxIndex(this double[] array)
        {
            var max = 0.0;
            var maxIndex = 0;
            for (int i = 0; i < array.Length; i++)
                if (array[i] > max)
                {
                    maxIndex = i;
                    max = array[i];
                }
            return maxIndex;
        }

        public static int MaxIndex(this float[] array)
        {
            var max = 0f;
            var maxIndex = 0;
            for (int i = 0; i < array.Length; i++)
                if (array[i] > max)
                {
                    maxIndex = i;
                    max = array[i];
                }
            return maxIndex;
        }

        public static int GetIndex<T>(this T[] items, T item)
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].Equals(item)) return i;
            }
            return -1;
        }

        public static int GetIndex<T>(this T[] items, T item, int startIndex)
        {
            for (int i = startIndex; i < items.Length; i++)
            {
                if (items[i].Equals(item)) return i;
            }
            return -1;
        }


        public static bool Contains<T>(this T[] arrayOne, T[] arrayTwo)
        {
            if (arrayOne.Length < arrayTwo.Length) return false;

            if (arrayOne.Length == arrayTwo.Length)
            {
                for (int i = 0; i < arrayOne.Length; i++)
                    if (!Equals(arrayOne[i], arrayTwo[i])) return false;
                return true;
            }

            var trie = new Trie<T>(arrayOne);
            return trie.Contains(arrayTwo);
        }

        public static T[][] ToJaggedArray<T>(this T[,] multiDimensionalArray)
        {
            var firstDimensionSize = multiDimensionalArray.GetLength(0);
            var secondDimensionSize = multiDimensionalArray.GetLength(1);

            var jaggedArray = new T[firstDimensionSize][];

            for (int i = 1; i <= firstDimensionSize; i++)
            {
                var outerArray = jaggedArray[i - 1] = new T[secondDimensionSize];
                for (int j = 1; j <= secondDimensionSize; j++)
                    outerArray[j - 1] = multiDimensionalArray[i, j];
            }
            return jaggedArray;
        }

        public static T[] Flatten<T>(this T[][] items)
        {
            T[] flat = new T[items.Length * items[0].Length];
            int index = 0;
            for (int y = 0; y < items.Length; y++)
                for (int x = 0; x < items[y].Length; x++)
                    flat[index++] = items[y][x];
            return flat;
        }

        public static Dictionary<T, int> DistinctWithCount<T>(this IEnumerable<T> items)
        {
            var distinct = new Dictionary<T, int>();

            foreach (var item in items)
            {
                int count;
                distinct.TryGetValue(item, out count);
                distinct[item] = count + 1;
            }
            return distinct;
        }

        //public static HashSet<T> ToHashSet<T>(this IEnumerable<T> collection)
        //{
        //    return new HashSet<T>(collection);
        //}

        //public static HashSet<T> ToHashSet<T>(this IEnumerable<T> collection, IEqualityComparer<T> comparer)
        //{
        //    return new HashSet<T>(collection, comparer);
        //}

        public static HashSet<T> TakePage<T, TSortKey>(this HashSet<T> items, int pageSize, int pageNumber, Func<T, TSortKey> sortFunction)
        {
            var itemsToSkip = (pageNumber - 1) * pageSize;
            return items.OrderBy(sortFunction).Skip(itemsToSkip).Take(pageSize.OrIfSmaller(items.Count - itemsToSkip)).ToHashSet();
        }
        public static HashSet<T> TakePage<T, TSortKey>(this IEnumerable<T> items, int pageSize, int pageNumber, Func<T, TSortKey> sortFunction)
        {
            var itemsToSkip = (pageNumber - 1) * pageSize;
            return items.OrderBy(sortFunction).Skip(itemsToSkip).Take(pageSize.OrIfSmaller(items.Count() - itemsToSkip)).ToHashSet();
        }



        public static HashSet<T> TakePageDescending<T, TSortKey>(this HashSet<T> items, int pageSize, int pageNumber, Func<T, TSortKey> sortFunction)
        {
            var itemsToSkip = (pageNumber - 1) * pageSize;
            return items.OrderByDescending(sortFunction).Skip(itemsToSkip).Take(pageSize.OrIfSmaller(items.Count - itemsToSkip)).ToHashSet();
        }
        public static HashSet<T> TakePageDescending<T, TSortKey>(this IEnumerable<T> items, int pageSize, int pageNumber, Func<T, TSortKey> sortFunction)
        {
            var itemsToSkip = (pageNumber - 1) * pageSize;
            return items.OrderByDescending(sortFunction).Skip(itemsToSkip).Take(pageSize.OrIfSmaller(items.Count() - itemsToSkip)).ToHashSet();
        }



        public static HashSet<T> TakePage<T>(this HashSet<T> items, int pageSize, int pageNumber)
        {
            var itemsToSkip = (pageNumber - 1) * pageSize;
            return items.Skip(itemsToSkip).Take(pageSize.OrIfSmaller(items.Count - itemsToSkip)).ToHashSet();
        }


        public static HashSet<T> TakePage<T>(this IEnumerable<T> items, int pageSize, int pageNumber)
        {
            var itemsToSkip = (pageNumber - 1) * pageSize;
            return items.Skip(itemsToSkip).Take(pageSize.OrIfSmaller(items.Count() - itemsToSkip)).ToHashSet();
        }

        public static uint IncrementValue<TKey>(this Dictionary<TKey, uint> dictionary, TKey key)
        {
            uint value;
            dictionary.TryGetValue(key, out value);
            dictionary[key] = ++value;
            return value;
        }


        public static int IncrementValue<TKey>(this Dictionary<TKey, int> dictionary, TKey key)
        {
            int value;
            dictionary.TryGetValue(key, out value);
            dictionary[key] = ++value;
            return value;
        }

        public static void SafeInsert<T>(this List<T> list, int posi, T value)
        {
            if (posi > list.Count)
                if (posi > list.Capacity)
                    list.Capacity = posi;
            while (posi > list.Count)
                list.Add(default);

            list.Insert(posi, value);
        }

        public static IEnumerable<KeyPair<TKeyOne, TKeyTwo>> ToKeyPairs<TKeyOne, TKeyTwo, TItem>(
            this IEnumerable<TItem> items, Func<TItem, KeyPair<TKeyOne, TKeyTwo>> pairSelector)
        {
            return items.Select(item => pairSelector(item));
        }

        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<TValue> values, Func<TValue, TKey> keyFunc, IEqualityComparer<TKey> comparer)
        {
            var dictionary = new Dictionary<TKey, TValue>(comparer);
            foreach (TValue item in values)
                dictionary[keyFunc(item)] = item;
            return dictionary;
        }

        public static Dictionary<TValue, List<TValue>> ToDictionaryByValue<TValue>(this IEnumerable<TValue> list, Func<TValue, List<TValue>> valuesFunc, IEqualityComparer<TValue> valueComparer)
        {
            var dictionary = new Dictionary<TValue, List<TValue>>(valueComparer);

            foreach (var item in list)
            {
                List<TValue> values;
                if (dictionary.TryGetValue(item, out values) == false || values == null)
                {
                    values = new List<TValue>();
                    dictionary[item] = values;
                }
                values.Add(item);
            }
            return dictionary;
        }

        public static Dictionary<TKey, List<TValue>> ToMultiDictionary<TKey, TValue>(this IEnumerable<TValue> list, Func<TValue, TKey> keySelector)
        {
            var dictionary = new Dictionary<TKey, List<TValue>>();

            foreach (var item in list)
            {
                var currentKey = keySelector(item);
                if (currentKey == null) continue;
                List<TValue> values;
                if (dictionary.TryGetValue(currentKey, out values) == false)
                {
                    values = new List<TValue>();
                    dictionary[currentKey] = values;
                }
                values.Add(item);
            }
            return dictionary;
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue item;
            if (dictionary.TryGetValue(key, out item)) return item;
            return default;
        }

        public static bool TryGetValue<T>(this List<T> list, int position, out T value)
        {
            if (list.Count <= position)
            {
                value = default;
                return false;
            }
            value = list[position];
            return true;
        }

        public static void SetValue<T>(this List<T> list, int posi, T value)
        {
            while (list.Count < posi)
                list.Add(default);
            list.Insert(posi, value);
        }

        public static void SafeInsert<TYpeOfValue>(this TYpeOfValue valueToAdd, ref TYpeOfValue[] array, int growthBeyondTenPercent = 1)
        {
            var positionWithinArray = 0;
            var positionFound = false;
            if (array != null)
            {
                for (var i = 0; i < array.Length; i++)
                {
                    if (Equals(default(TYpeOfValue), array[i]))
                    {
                        positionWithinArray = i;
                        positionFound = true;
                        break;
                    }
                }
                if (positionFound == false) positionWithinArray = array.Length;
            }
            else
            {
                positionWithinArray = 0;
            }

            //checking to see if the new position falls within the current bounds of the array
            var growthPercent = 10 + growthBeyondTenPercent;
            if (array == null)
                array = new TYpeOfValue[positionWithinArray + 1 + ((positionWithinArray / 100) * growthPercent)];

            if (array.Length <= positionWithinArray)
            {
                var tempArray = new TYpeOfValue[positionWithinArray + 1 + ((positionWithinArray / 100) * growthPercent)];
                Array.Copy(array, tempArray, array.Length);
                array = tempArray;
            }

            array[positionWithinArray] = valueToAdd;
        }

        public static void SafeInsert<T>(this T valueToAdd, ref T[] array, long positionWithinArray, int growthBeyondTenPercent = 1)
        {
            //checking to see if the new position falls within the current bounds of the array
            var growthPercent = 10 + growthBeyondTenPercent;
            if (array == null)
                array = new T[positionWithinArray + 1 + ((positionWithinArray / 100) * growthPercent)];

            if (array.Length <= positionWithinArray)
            {
                var tempArray = new T[positionWithinArray + 1 + ((positionWithinArray / 100) * growthPercent)];
                Array.Copy(array, tempArray, array.Length);
                array = tempArray;
            }

            array[positionWithinArray] = valueToAdd;
        }

        public static bool TryGetValue(this System.Collections.Specialized.NameValueCollection nameValueCollection, string keyName, out string returnValue)
        {
            if (nameValueCollection.Keys.Count != 0)
            {
                foreach (var key in nameValueCollection.AllKeys)
                {
                    if (string.Equals(key, keyName))
                    {
                        returnValue = nameValueCollection[key];
                        return true;
                    }
                }
            }

            returnValue = string.Empty;
            return false;
        }

        public static bool TryGetValueAsInt(this System.Collections.Specialized.NameValueCollection nameValueCollection, string keyName, out int returnValue)
        {
            if (nameValueCollection.Keys.Count != 0)
            {
                foreach (var key in nameValueCollection.AllKeys)
                {
                    if (!string.Equals(key, keyName)) continue;
                    var value = nameValueCollection[key];
                    return int.TryParse(value, out returnValue);
                }
            }

            returnValue = default;
            return false;
        }

        public static bool TryGetValueAsGuid(this System.Collections.Specialized.NameValueCollection nameValueCollection, string keyName, out Guid returnValue)
        {
            if (nameValueCollection != null && nameValueCollection.Keys.Count != 0)
            {
                foreach (var key in nameValueCollection.AllKeys)
                {
                    if (string.Equals(key, keyName))
                    {
                        var value = nameValueCollection[key];
                        return Guid.TryParse(value, out returnValue);
                    }
                }
            }

            returnValue = Guid.Empty;
            return false;
        }

        public static bool AddToCollection<TKey, TValue>(this Dictionary<TKey, HashSet<TValue>> dictionary, TKey key, TValue value, IEqualityComparer<TValue> comparer = null)
        {
            HashSet<TValue> collection;
            if (!dictionary.TryGetValue(key, out collection))
            {
                collection = comparer != null ? new HashSet<TValue>(comparer) : new HashSet<TValue>();
                dictionary[key] = collection;
            }
            return collection.Add(value);
        }

        public static bool AddToCollection<TKey, TValue>(this Dictionary<TKey, List<TValue>> dictionary, TKey key, TValue value)
        {
            List<TValue> collection;
            if (!dictionary.TryGetValue(key, out collection))
                dictionary[key] = collection = new List<TValue>();
            if (collection.Contains(value)) return false;
            collection.Add(value);
            return true;
        }

        public static bool AddToCollection<TKey, TValue>(this Dictionary<TKey, Queue<TValue>> dictionary, TKey key, TValue value)
        {
            Queue<TValue> collection;
            if (!dictionary.TryGetValue(key, out collection))
                dictionary[key] = collection = new Queue<TValue>();
            if (collection.Contains(value)) return false;
            collection.Enqueue(value);
            return true;
        }

        public static bool TryGetFromCollection<TKey, TValue>(this Dictionary<TKey, Queue<TValue>> dictionary, TKey key, out TValue value)
        {
            Queue<TValue> collection;
            if (!dictionary.TryGetValue(key, out collection) || collection.Count == 0)
            {
                value = default;
                return false;
            }

            value = collection.Dequeue();
            return true;
        }

        public static bool AddToCollection<TKey, TValue>(this Dictionary<TKey, List<TValue>> dictionary, TKey key, TValue value, out int position)
        {
            List<TValue> collection;
            if (!dictionary.TryGetValue(key, out collection))
            {
                collection = new List<TValue>();
                dictionary[key] = collection;
            }

            for (position = 0; position < collection.Count; position++)
                if (Equals(collection[position], value))
                    return false;

            collection.Add(value);
            return true;
        }

        public static List<List<T>> SplitCollection<T>(this List<T> items)
        {
            return items.SplitCollection(Environment.ProcessorCount);
        }

        public static TValue TryReturnOrSetValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TValue> defaultValueDelegate)
        {
            TValue value;
            if (!dictionary.TryGetValue(key, out value))
            {
                value = defaultValueDelegate();
                dictionary[key] = value;
            }

            return value;
        }

        public static TValue TryReturnValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue value;
            dictionary.TryGetValue(key, out value);
            return value;
        }


        /// <summary>
        /// Splits a List<T> into multiple Lists.
        /// </summary>
        /// <param name="chunkCount">Total number of new lists to create</param>
        /// <returns></returns>
        public static List<List<T>> SplitCollection<T>(this List<T> items, int chunkCount)
        {
            var chunks = new List<List<T>>();

            int chunkSize;
            if (items.Count < chunkCount) chunkSize = items.Count;
            else chunkSize = (int)Math.Ceiling((double)items.Count / chunkCount);

            for (var i = 0; i < chunkCount; i++)
            {
                var currentPosition = i * chunkSize;
                var currentChunk = items.Skip(currentPosition).Take(chunkSize.OrIfSmaller(items.Count - currentPosition)).ToList();
                if (!currentChunk.Any()) break;
                chunks.Add(currentChunk);
                if (currentChunk.Count < chunkSize) break;
            }

            return chunks;
        }

        /// <summary>
        /// Generates a destination path for recursive directory moves given a new root directory
        /// </summary>
        /// <param name="stack">First member must be your root directory which after completion RootDestination will directly mirror</param>
        /// <param name="rootDestination">Directory you will add the new structure to.</param>
        /// <returns></returns>
        public static string ToDestinationPath(this Stack<DirectoryInfo> stack, string rootDestination)
        {
            if (stack == null || stack.Count <= 1) return rootDestination;
            if (!rootDestination.EndsWith("\\")) rootDestination = rootDestination + "\\";
            var destination = rootDestination;

            var i = 1;
            foreach (var di in stack)
            {
                if (i == stack.Count) continue;
                i++;
                destination += di.Name + "\\";
            }

            return destination;
        }

        public static void TryAdd(this ICollection<short> collection, string value)
        {
            short convertedValue;
            if (short.TryParse(value, out convertedValue))
                collection.Add(convertedValue);
        }

        public static void TryAdd(this ICollection<int> collection, string value)
        {
            int convertedValue;
            if (int.TryParse(value, out convertedValue))
                collection.Add(convertedValue);
        }

        public static void TryAdd(this ICollection<long> collection, string value)
        {
            long convertedValue;
            if (long.TryParse(value, out convertedValue))
                collection.Add(convertedValue);
        }

        public static void TryAdd(this ICollection<float> collection, string value)
        {
            float convertedValue;
            if (float.TryParse(value, out convertedValue))
                collection.Add(convertedValue);
        }

        public static void TryAdd(this ICollection<decimal> collection, string value)
        {
            decimal convertedValue;
            if (decimal.TryParse(value, out convertedValue))
                collection.Add(convertedValue);
        }

        public static List<T> ToTypedList<T>(this ICollection<string> initialCollection, TryParseHandler<T> converter)
        {
            var typedList = new List<T>(initialCollection.Count);

            foreach (var item in initialCollection)
            {
                T result;
                if (converter(item, out result)) typedList.Add(result);
            }
            return typedList;
        }

        public delegate bool TryParseHandler<T>(string value, out T result);

        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
        {
            if (enumeration == null) return;
            foreach (var item in enumeration) action(item);
        }
    }
}
