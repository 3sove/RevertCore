using System;
using System.Collections.Generic;
using System.Text;
using Revert.Core.Common.Types;

namespace Revert.Core.IO.Stores
{
    public interface IKeyValueStore<TKey, TValue>
    {
        //long Count { get; }
        long GetCount();

        void Upsert(TKey key, TValue value);
        void UpsertRange(IEnumerable<IKeyPair<TKey, TValue>> itemsToAdd);

        bool Delete(TKey key);
        bool Delete(TKey key, TValue value);



        //TODO: Finish porting all logic for BPlusTree to an IKeyValueStore class which makes another version available in MS FASTER

        /// <summary>
        /// Returns all items within the store.  Key is not guaranteed to be unique.
        /// </summary>
        /// <returns></returns>
        IEnumerable<IKeyPair<TKey, TValue>> GetItems();

        IEnumerable<TKey> GetKeys();
        IEnumerable<TValue> GetValues();

        TValue Get(TKey key);

        bool TryGetValue(TKey key, out TValue value);

        bool TryGetValues(TKey[] keys, out TValue[] values);

    }
}
