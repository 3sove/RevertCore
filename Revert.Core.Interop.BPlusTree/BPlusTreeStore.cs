using System;
using CSharpTest.Net.Collections;
using CSharpTest.Net.Serialization;
using Revert.Core.IO.Serialization;
using Revert.Core.IO.Stores;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Revert.Core.Common;
using Revert.Core.Common.Types;

namespace Revert.Core.Interop.BPlusTree
{
    public class BPlusTreeStore<TKey, TVertex> : IKeyValueStore<TKey, TVertex> {
        private readonly IKeyGenerator<TKey> keyGenerator;
        public BPlusTree<TKey, TVertex> Tree { get; set; }

        public long Count => GetCount();

        /// <param name="fileName">Full filename, including path and extension.</param>
        public BPlusTreeStore(string fileName, IKeyGenerator<TKey> keyGenerator,  bool largeVertices = false)
        {
            this.keyGenerator = keyGenerator;
            Tree = new BPlusTree<TKey, TVertex>(new BPlusTree<TKey, TVertex>.OptionsV2(new ProtobufSerializerInterop<TKey>(), new ProtobufSerializerInterop<TVertex>())
            {
                CreateFile = CreatePolicy.IfNeeded,
                FileName = fileName,
                StorageType = StorageType.Disk,
                StoragePerformance = StoragePerformance.Default,
                FileBlockSize = largeVertices ? 16384 : 4096
            });
            Tree.EnableCount();
        }

        public void UpsertRange(IEnumerable<IKeyPair<TKey, TVertex>> itemsToAdd)
        {
            Tree.AddRange(itemsToAdd.Select(item => new KeyValuePair<TKey, TVertex>(item.KeyOne, item.KeyTwo)));
            Commit();
        }

        public void Commit()
        {
            Tree.Commit();
        }

        public bool Delete(TKey key)
        {
            var result = Tree.Remove(key);
            if (result) Commit();
            return result;
        }

        public bool Delete(TKey key, TVertex value)
        {
            if (Tree.TryGetValue(key, out var localVertex))
            {
                if (localVertex.Equals(value))
                {
                    Tree.Remove(key);
                    Commit();
                    return true;
                }
            }
            return false;
        }

        public void Dispose()
        {
            Tree.Dispose();
        }

        public TVertex Get(TKey key)
        {
            return Tree[key];
        }

        public long GetCount()
        {
            return Tree.Count;
        }

        public IEnumerable<IKeyPair<TKey, TVertex>> GetItems()
        {
            return Tree.Select(item => new Common.Types.KeyPair<TKey, TVertex>(item.Key, item.Value));
        }

        public IEnumerable<IKeyPair<TKey, TVertex>> GetItems(TKey startKey, TKey endKey)
        {
            return Tree.EnumerateRange(startKey, endKey).Select(item => new Common.Types.KeyPair<TKey, TVertex>(item.Key, item.Value));
        }

        public IEnumerable<TKey> GetKeys()
        {
            return Tree.Keys;
        }

        public TKey GetNextKey()
        {
            return keyGenerator.GetNext();
        }

        public TKey GetLastKey()
        {
            System.Collections.Generic.KeyValuePair<TKey, TVertex> lastItem;
            return Tree.TryGetLast(out lastItem) ? lastItem.Key : default(TKey);
        }

        public IEnumerable<TVertex> GetValues()
        {
            return Tree.Values;
        }

        public void Rollback()
        {
            Tree.Rollback();
        }

        public bool TryGetValue(TKey key, out TVertex value)
        {
            return Tree.TryGetValue(key, out value);
        }

        public void Upsert(TKey key, TVertex value)
        {
            Tree[key] = value;
            //Commit();
        }

        public bool TryGetValues(TKey[] keys, out TVertex[] values)
        {
            throw new NotImplementedException();
        }
    }
}
