using System;
using System.Collections.Generic;
using System.Text;
using Revert.Core.Common.Types;

namespace Revert.Core.IO.Stores
{
    public interface IKeyMultiValueStore<TKey, TValue> : IKeyValueStore<TKey, IEnumerable<TValue>>, IDisposable
    {
        void Upsert(TKey key, TValue value);
        void UpsertRange(IEnumerable<IKeyPair<TKey, TValue>> itemsToAdd);
        bool Delete(TKey key, TValue value);
        new IEnumerable<TValue> GetValues();


    }
}
