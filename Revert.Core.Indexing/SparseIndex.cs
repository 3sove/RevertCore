//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using CSharpTest.Net.Collections;
//using CSharpTest.Net.Serialization;
//using Revert.Core.Extensions;

//namespace Revert.Core.Indexing
//{
//    public class SparseIndex<TKey, TValue> : IIndex<TKey, TValue>
//    {
//        public string FilePath { get; set; }

//        public SparseIndex(string filePath, ISerializer<TKey> keySerializer, ISerializer<HashSet<TValue>> valueSerializer, IComparer<TKey> keyComparer = null)
//        {
//            FilePath = filePath;
//            var fileInfo = new FileInfo(filePath);
//            if (fileInfo.Directory == null) throw new DirectoryNotFoundException($"The file path requires a directory path and file, such as C:\\Files\\myFile1.xml.  The value supplied was {filePath}");
//            if (!fileInfo.Directory.Exists) fileInfo.Directory.FullName.CreateDirectory();

//            Tree = new BPlusTree<TKey, HashSet<TValue>>(keyComparer == null
//                ? new BPlusTree<TKey, HashSet<TValue>>.OptionsV2(keySerializer, valueSerializer)
//                {
//                    CreateFile = CreatePolicy.IfNeeded,
//                    FileName = FilePath,
//                    StorageType = StorageType.Disk,
//                    StoragePerformance = StoragePerformance.Default
//                }
//                : new BPlusTree<TKey, HashSet<TValue>>.OptionsV2(keySerializer, valueSerializer, keyComparer)
//                {
//                    CreateFile = CreatePolicy.IfNeeded,
//                    FileName = FilePath,
//                    StorageType = StorageType.Disk,
//                    StoragePerformance = StoragePerformance.Default
//                });
//        }

//        public BPlusTree<TKey, HashSet<TValue>> Tree { get; set; }

//        /// <summary>
//        /// Calls Commit after insertion
//        /// </summary>
//        /// <param name="key"></param>
//        /// <param name="value"></param>
//        public void Upsert(TKey key, TValue value)
//        {
//            HashSet<TValue> values;
//            if (!Tree.TryGetValue(key, out values)) values = new HashSet<TValue>();
//            values.Upsert(value);
//            Tree[key] = values;
//            Tree.Commit();
//        }

//        public void Commit()
//        {
//            Tree.Commit();
//        }

//        /// <summary>
//        /// Calls Commit after bulk insertion
//        /// </summary>
//        /// <param name="items"></param>
//        public void Upsert(Dictionary<TKey, HashSet<TValue>> items)
//        {
//            Tree.UpsertRange(items);
//            //Tree.BulkInsert(items.Select(item => new KeyPair<TKey, HashSet<TValue>>(item.KeyOne, item.KeyTwo)), new BulkInsertOptions
//            //{
//            //    CommitOnCompletion = true,
//            //    DuplicateHandling = DuplicateHandling.LastValueWins,
//            //    InputIsSorted = false,
//            //});
//        }

//        public void Delete(TKey key)
//        {
//            Tree.Delete(key);
//        }

//        public void Delete(TKey key, HashSet<TValue> values)
//        {
//            HashSet<TValue> previousValues;
//            if (!Tree.TryGetValue(key, out previousValues)) return;
//            Tree[key] = previousValues.Except(values).ToHashSet();
//        }

//        public void Update(TKey key, HashSet<TValue> values)
//        {
//            Tree[key] = values;
//        }

//        public IEnumerable<TValue> GetValues(TKey key)
//        {
//            HashSet<TValue> items;
//            if (!Tree.TryGetValue(key, out items)) return null;
//            return items;
//        }

//        public bool TryGetValue(TKey key, out HashSet<TValue> items)
//        {
//            return Tree.TryGetValue(key, out items);
//        }

//        public IEnumerable<TKey> Keys => Tree.Keys;
//        public IEnumerable<TValue> Values { get { return Tree.Values.SelectMany(item => item); } }
//    }
//}
