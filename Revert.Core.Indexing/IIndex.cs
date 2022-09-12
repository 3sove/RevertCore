using System.Collections.Generic;
using Revert.Core.IO.Stores;

namespace Revert.Core.Indexing
{
    //DEPRECATED - use Revert.Core.IO.IKeyValueStore instead

    //public interface IIndex<TKey, TValue> 
    //{
    //    //BPlusTree<TKey, HashSet<TValue>> Tree { get; set; }
    //    void Upsert(TKey key, TValue value);
    //    void Upsert(Dictionary<TKey, HashSet<TValue>> items);
    //    void Delete(TKey key);
    //    void Delete(TKey key, TValue value);
    //    void Delete(TKey key, HashSet<TValue> values);
    //    void Update(TKey key, TValue oldValue, TValue value);
    //    void Update(TKey key, HashSet<TValue> values);
    //    IEnumerable<TValue> GetValues(TKey key);
    //    bool TryGetValue(TKey key, out HashSet<TValue> values);
    //    IEnumerable<TKey> Keys { get; }
    //    IEnumerable<TValue> Values { get; }
    //}
}