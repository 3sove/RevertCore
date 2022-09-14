using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Revert.Core.Common;
using Revert.Core.Common.Types;
using Revert.Core.Extensions;
using Revert.Core.IO.Serialization;
using Revert.Core.IO.Stores;

namespace Revert.Core.Indexing
{
    public class KeyPairKeyGenerator<TKey> : KeyGenerator<KeyPair<TKey, uint>>
    {
        public KeyPairKeyGenerator(IKeyGenerator<TKey> keyGenerator) : base(new KeyPair<TKey, uint>(keyGenerator.GetNext(), 0), pair => new KeyPair<TKey, uint>(keyGenerator.GetNext(), 0))
        {
        }
    }

    public abstract class FasterKeyedIndex<TKey, TValue> : IKeyValueStore<TKey, TValue>
    {
        private readonly IKeyGenerator<TKey> keyGenerator;

        private IKeyValueStore<KeyPair<TKey, uint>, TValue> valuesByKeyAndIndex;
        public IKeyValueStore<KeyPair<TKey, uint>, TValue> ValuesByKeyAndIndex
        {
            get => valuesByKeyAndIndex ?? (valuesByKeyAndIndex = GetValuesByKeyAndIndexTree());
            set => valuesByKeyAndIndex = value;
        }
        public IEnumerable<TKey> Keys => CountByKey.GetKeys().ToList();
        public IEnumerable<TValue> Values => ValuesByKeyAndIndex.GetValues();

        private IKeyValueStore<TKey, uint> countByKey;
        public IKeyValueStore<TKey, uint> CountByKey
        {
            get
            {
                lock (DirectoryPath)
                {
                    return countByKey ?? (countByKey = GetCountByKeyTree());
                }
            }
            set => countByKey = value;
        }

        private IKeyValueStore<TKey, uint> GetCountByKeyTree()
        {
            return new FasterKeyValueStore<TKey, uint>(DirectoryPath.AddFilePath("VertexCountByKey\\"), keyGenerator);
        }

        private IKeyValueStore<KeyPair<TKey, uint>, TValue> GetValuesByKeyAndIndexTree()
        {
            return new FasterKeyValueStore<KeyPair<TKey, uint>, TValue>(DirectoryPath.AddFilePath("ValuesByKeyAndIndex\\"), new KeyPairKeyGenerator<TKey>(keyGenerator));
        }

        public string DirectoryPath { get; set; }
        public string IndexName { get; set; }

        public ISerializer<TKey> KeySerializer { get; set; } = new ProtobufSerializer<TKey>();
        public ISerializer<TValue> ValueSerializer { get; set; }

        public long GetCount()
        {
            return ValuesByKeyAndIndex.GetCount();
        }

        public void Upsert(TKey key, TValue value)
        {
            throw new NotImplementedException();
        }

        public void UpsertRange(IEnumerable<IKeyPair<TKey, TValue>> itemsToAdd)
        {
            throw new NotImplementedException();
        }

        public bool Delete(TKey key)
        {
            throw new NotImplementedException();
        }

        public bool Delete(TKey key, TValue value)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IKeyPair<TKey, TValue>> GetItems()
        {
            var items = new List<KeyPair<TKey, TValue>>();
            foreach (var item in CountByKey.GetItems())
            {
                var values = new List<TValue>();
                for (uint i = 0; i < item.KeyTwo; i++)
                {
                    var key = new KeyPair<TKey, uint>(item.KeyOne, i);
                    if (ValuesByKeyAndIndex.TryGetValue(key, out var value)) values.Add(value);
                }

                foreach (var value in values)
                {
                    items.Add(new KeyPair<TKey, TValue>(item.KeyOne, value));
                }
            }
            return items;
        }

        public IEnumerable<TKey> GetKeys()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TValue> GetValues()
        {
            throw new NotImplementedException();
        }

        public TValue Get(TKey key)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            throw new NotImplementedException();
        }

        public FasterKeyedIndex(string directoryPath, string indexName, ISerializer<TValue> valueSerializer, IKeyGenerator<TKey> keyGenerator)
        {
            this.keyGenerator = keyGenerator;
            DirectoryPath = directoryPath;
            IndexName = indexName;
            ValueSerializer = valueSerializer;

            if (!Directory.Exists(directoryPath)) directoryPath.CreateDirectory();
        }

        public void Add(TKey key, TValue value)
        {
            lock (CountByKey)
            {
                try
                {
                    uint count;
                    if (!CountByKey.TryGetValue(key, out count)) count = 0;
                    count++;
                    var keyPair = new KeyPair<TKey, uint>(key, count);

                    ValuesByKeyAndIndex.Upsert(keyPair, value);
                    CountByKey.Upsert(key, count);
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }


        //private byte[] GetKeyBytes(TKey key, uint index)
        //{
        //    var keyBytes = GetKeyBytes(key);
        //    var paddedKeyBytes = new byte[keyBytes.Length + 4];
        //    Array.Copy(keyBytes, 0, paddedKeyBytes, 4, keyBytes.Length);
        //    var countBytes = BitConverter.GetBytes(index);
        //    Array.Copy(countBytes, 0, paddedKeyBytes, 0, countBytes.Length);
        //    return keyBytes;
        //}

        ///// <summary>
        ///// Return an unpadded array of bytes representing the key
        ///// </summary>
        //protected abstract byte[] GetKeyBytes(TKey key);


        public void Add(Dictionary<TKey, TValue> items)
        {
            lock (CountByKey)
            {
                try
                {
                    var newItems = new List<KeyPair<KeyPair<TKey, uint>, TValue>>();

                    foreach (var item in items)
                    {
                        uint count;
                        if (!CountByKey.TryGetValue(item.Key, out count)) count = 0;
                        newItems.Add(
                            new KeyPair<KeyPair<TKey, uint>, TValue>(
                                new KeyPair<TKey, uint>(item.Key, count), 
                                item.Value));
                        CountByKey.Upsert(item.Key, ++count);
                    }

                    ValuesByKeyAndIndex.UpsertRange(newItems);

                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public void Add(Dictionary<TKey, HashSet<TValue>> items)
        {
            lock (CountByKey)
            {
                try
                {
                    foreach (var item in items)
                    {
                        uint count;
                        if (!CountByKey.TryGetValue(item.Key, out count)) count = 0;

                        var newItems = new List<KeyPair<KeyPair<TKey, uint>, TValue>>();

                        var values = item.Value.ToArray();

                        for (uint i = 0; i < values.Length; i++)
                        {
                            var value = values[i];
                            newItems.Add(
                                new KeyPair<KeyPair<TKey, uint>, TValue>(
                                    new KeyPair<TKey, uint>(item.Key, count + i), value));
                        }

                        ValuesByKeyAndIndex.UpsertRange(newItems);
                        CountByKey.Upsert(item.Key, count);
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public void Remove(TKey key)
        {
            lock (CountByKey)
            {
                try
                {
                    uint count;
                    if (!CountByKey.TryGetValue(key, out count)) return;

                    for (uint i = 0; i < count; i++)
                    {
                        ValuesByKeyAndIndex.Delete(new KeyPair<TKey, uint>(key, i));
                    }

                    CountByKey.Delete(key);
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public void Remove(TKey key, TValue value)
        {
            lock (CountByKey)
            {
                try
                {
                    uint count;
                    if (!CountByKey.TryGetValue(key, out count)) return;

                    uint removed = 0;
                    for (uint i = 0; i < count; i++)
                    {
                        var keyPair = new KeyPair<TKey, uint>(key, i);

                        TValue indexedValue;
                        if (ValuesByKeyAndIndex.TryGetValue(keyPair, out indexedValue))
                        {
                            if (Equals(value, indexedValue))
                            {
                                ValuesByKeyAndIndex.Delete(keyPair);
                                countByKey.Upsert(key, count - ++removed);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public void Remove(TKey key, HashSet<TValue> values)
        {
            foreach (var value in values)
                Remove(key, value);
        }

        public void Update(TKey key, TValue oldValue, TValue value)
        {
            uint count;
            if (!CountByKey.TryGetValue(key, out count)) return;

            for (uint i = 0; i < count; i++)
            {
                var keyPair = new KeyPair<TKey, uint>(key, i);

                if (ValuesByKeyAndIndex.TryGetValue(keyPair, out var indexedValue))
                {
                    if (Equals(oldValue, indexedValue))
                    {
                        ValuesByKeyAndIndex.Upsert(keyPair, value);
                    }
                }
            }
        }

        public void Update(TKey key, HashSet<TValue> values)
        {
            try
            {
                Remove(key);
                var dictionary = new Dictionary<TKey, HashSet<TValue>> { [key] = values };
                Add(dictionary);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<TValue> GetValues(TKey key)
        {
            lock (CountByKey)
            {
                uint count;
                if (!CountByKey.TryGetValue(key, out count)) return null;

                var start = new KeyPair<TKey, uint>(key, 0);
                var end = new KeyPair<TKey, uint>(key, count);

                return ValuesByKeyAndIndex.GetItems().Where(item => item.KeyOne.KeyOne.Equals(key)).Select(item => item.KeyTwo);
            }
        }

        public bool TryGetValue(TKey key, out HashSet<TValue> values)
        {
            var enumerableValues = GetValues(key);
            if (enumerableValues == null)
            {
                values = null;
                return false;
            }

            values = enumerableValues.ToHashSet();
            return true;
        }

        public bool TryGetValues(TKey[] keys, out TValue[] values)
        {
            throw new NotImplementedException();
        }
    }
}