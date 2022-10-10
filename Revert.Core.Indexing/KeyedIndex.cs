using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using MongoDB.Bson;
using MongoDB.Driver;
using Revert.Core.Common;
using Revert.Core.Common.Types;
using Revert.Core.Extensions;
using Revert.Core.IO;
using Revert.Core.IO.Stores;

namespace Revert.Core.Indexing
{
    public class KeyedIndex<TValue> : KeyedIndex<ObjectId, TValue>
    {
        public KeyedIndex(string connectionString, string databaseName, string collectionName) : base(connectionString, databaseName, collectionName, ObjectIdKeyIssuer.Instance)
        {
        }
    }

    public abstract class KeyedIndex<TKey, TValue> : IKeyValueStore<TKey, TValue>
    {
        public KeyedIndex(string connectionString, string databaseName, string collectionName, KeyGenerator<TKey> keyIssuer)
        {
            ConnectionString = connectionString;
            DatabaseName = databaseName;
            CollectionName = collectionName;
            //ValueSerializer = valueSerializer;
            KeyIssuer = keyIssuer;
        }

        public KeyGenerator<TKey> KeyIssuer { get; set; }
        //public ISerializer<TValue> ValueSerializer { get; set; }
        public string CollectionName { get; set; }
        public string ConnectionString { get; }
        public string DatabaseName { get; set; }

        //TODO: Move this mongo stuff out of here
        private MongoClient mongoClient;
        private MongoClient MongoClient => mongoClient ?? (mongoClient = new MongoClient(ConnectionString));

        private IMongoDatabase _db;
        private IMongoDatabase db => _db ?? (_db = MongoClient.GetDatabase(DatabaseName));
        
        private IMongoCollection<MongoKeyPair<TKey,TValue>> Collection => db.GetCollection<MongoKeyPair<TKey, TValue>>(CollectionName);

        public long GetCount()
        {
            return Collection.CountDocuments(pair => true);
        }

        public void Upsert(TKey key, TValue value)
        {
            Collection.InsertOne(new MongoKeyPair<TKey, TValue>(key, value));
        }

        public void UpsertRange(IEnumerable<IKeyPair<TKey, TValue>> itemsToAdd)
        {
            Collection.InsertMany(itemsToAdd.Select(item => item is MongoKeyPair<TKey, TValue> mongoItem
                ? mongoItem : new MongoKeyPair<TKey, TValue>(item.KeyOne, item.KeyTwo)));
        }

        /// <summary>
        /// Return an unpadded array of bytes representing the key
        /// </summary>
        protected virtual byte[] GetKeyBytes(TKey key)
        {
            if (key is ObjectId id)
                return id.ToByteArray();

            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, key);
                return ms.ToArray();
            }
        }

        public void Upsert(Dictionary<TKey, TValue> items)
        {
            var mongoItems = items.Select(pair => new MongoKeyPair<TKey, TValue>(pair.Key, pair.Value)).ToList();
            Collection.InsertMany(mongoItems);
        }

        public void Upsert(Dictionary<TKey, HashSet<TValue>> items)
        {
            var itemsToAdd = new List<MongoKeyPair<TKey, TValue>>();
            
            foreach (var item in items)
            foreach (var value in item.Value)
                itemsToAdd.Add(new MongoKeyPair<TKey, TValue>(item.Key, value));

            Collection.InsertMany(itemsToAdd);
        }

        public bool Delete(TKey key)
        {
            var filter = Builders<MongoKeyPair<TKey, TValue>>.Filter.Eq(pair => pair.KeyOne, key);
            //var result = Collection.Find(filter).FirstOrDefault();
            var deleteResult = Collection.DeleteOne(filter);
            return deleteResult.IsAcknowledged;
        }

        public bool Delete(TKey key, TValue value)
        {
            var filterKey = Builders<MongoKeyPair<TKey, TValue>>.Filter.Eq(pair => pair.KeyOne, key);
            var filterValue = Builders<MongoKeyPair<TKey, TValue>>.Filter.Eq(pair => pair.KeyTwo, value);
            //var andFilter = Builders<MongoKeyPair<TKey, TValue>>.Filter.And(new[] {filterKey, filterValue});
            //var result = Collection.Find(filter).FirstOrDefault();
            var deleteResult = Collection.DeleteOne(filterKey & filterValue);
            return deleteResult.IsAcknowledged;
        }

        public IEnumerable<IKeyPair<TKey, TValue>> GetItems()
        {
            return Collection.Find(_ => true).ToList();
        }

        public bool Delete(TKey key, HashSet<TValue> values)
        {
            var orFilters = new List<FilterDefinition<MongoKeyPair<TKey, TValue>>>();
            foreach (var value in values)
            {
                var filterKey = Builders<MongoKeyPair<TKey, TValue>>.Filter.Eq(pair => pair.KeyOne, key);
                var filterValue = Builders<MongoKeyPair<TKey, TValue>>.Filter.Eq(pair => pair.KeyTwo, value);
                //var andFilter = Builders<MongoKeyPair<TKey, TValue>>.Filter.And(new[] { filterKey, filterValue });
                //var result = Collection.Find(filter).FirstOrDefault();

                //Take special note of the & operator
                orFilters.Add(filterKey & filterValue);
            }
            var orFilter = Builders<MongoKeyPair<TKey, TValue>>.Filter.Or(orFilters);
            var deleteResult = Collection.DeleteMany(orFilter);
            return deleteResult.IsAcknowledged;
        }

        public void Update(TKey key, TValue oldValue, TValue value)
        {
            var filterKey = Builders<MongoKeyPair<TKey, TValue>>.Filter.Eq(pair => pair.KeyOne, key);
            var filterValue = Builders<MongoKeyPair<TKey, TValue>>.Filter.Eq(pair => pair.KeyTwo, oldValue);
            //var andFilter = Builders<MongoKeyPair<TKey, TValue>>.Filter.And(new[] { filterKey, filterValue });
            Collection.UpdateOne(filterKey & filterValue, new ObjectUpdateDefinition<MongoKeyPair<TKey, TValue>>(value));
        }

        public void Update(TKey key, HashSet<TValue> values)
        {
            try
            {
                Delete(key);
                var dictionary = new Dictionary<TKey, HashSet<TValue>> { [key] = values };
                Upsert(dictionary);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<TValue> GetValues(TKey key)
        {
            var filterKey = Builders<MongoKeyPair<TKey, TValue>>.Filter.Eq(pair => pair.KeyOne, key);
            return Collection.Find(filterKey).ToList().Select(item => item.KeyTwo);
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

        public IEnumerable<TKey> Keys => GetKeys();
        public IEnumerable<TKey> GetKeys()
        {
            return Collection.Find(_ => true).ToList().Select(item => item.KeyOne);
        }

        public IEnumerable<TValue> Values => Collection.Find(_ => true).ToList().Select(item => item.KeyTwo);
        public IEnumerable<TValue> GetValues()
        {
            return Values;
        }

        public TValue Get(TKey key)
        {
            var filter = Builders<MongoKeyPair<TKey, TValue>>.Filter.Eq(pair => pair.KeyOne, key);
            return Collection.Find(filter).FirstOrDefault().KeyTwo;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            var filter = Builders<MongoKeyPair<TKey, TValue>>.Filter.Eq(pair => pair.KeyOne, key);
            value = Collection.Find(filter).FirstOrDefault().KeyTwo;
            return !Equals(value, default(TValue));
        }


        public long Count => Collection.CountDocuments(pair => true);

        public void Dispose()
        {
            //TODO: Look into mongo disposal 
        }

        public bool TryGetValues(TKey[] keys, out TValue[] values)
        {
            var inFilter = Builders<MongoKeyPair<TKey, TValue>>.Filter.In(item => item.KeyOne, keys);
            values = Collection.Find(inFilter).ToList().Select(item => item.KeyTwo).ToArray();
            return values.Any();

            //var keyFilters = new FilterDefinition<MongoKeyPair<TKey, TValue>>[keys.Length];
            //for (int i = 0; i < keys.Length; i++)
            //{
            //    keyFilters[i] = Builders<MongoKeyPair<TKey, TValue>>.Filter.Eq(item => item.KeyOne, keys[i]);
            //}
            //var orFilter = Builders<MongoKeyPair<TKey, TValue>>.Filter.Or(keyFilters);

            //values = Collection.Find(orFilter).ToList().Select(item => item.KeyTwo).ToArray();
            //return values.Any();
        }
    }
}