using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using Revert.Core.Common;
using Revert.Core.Common.Types;

namespace Revert.Core.IO.Stores
{
    public class MongoKeyValueStore<TKey, TValue> : IKeyValueStore<TKey, TValue>
    {
        public string ConnectionString { get; }
        public string DatabaseName { get; }
        public string CollectionName { get; }
        //public KeyGenerator<TKey> KeyGenerator { get; }

        private MongoClient mongoClient = null;
        private MongoClient MongoClient
        {
            get
            {
                return mongoClient ?? (mongoClient = new MongoClient(ConnectionString));
            }
        }

        private IMongoCollection<MongoKeyPair<TKey, TValue>> Collection => db.GetCollection<MongoKeyPair<TKey, TValue>>(CollectionName);

        private IMongoDatabase _db;
        private IMongoDatabase db => _db ?? (_db = MongoClient.GetDatabase(DatabaseName));

        public MongoKeyValueStore(string connectionString, string databaseName, string collectionName) //, KeyGenerator<TKey> keyGenerator)
        {
            ConnectionString = connectionString;
            DatabaseName = databaseName;
            CollectionName = collectionName;
            //KeyGenerator = keyGenerator;
        }

        public long Count => GetCount();

        public void Upsert(TKey key, TValue value)
        {
            Collection.InsertOne(new MongoKeyPair<TKey, TValue>(key, value));
        }

        public void UpsertRange(IEnumerable<IKeyPair<TKey, TValue>> itemsToAdd)
        {
            if (itemsToAdd is IEnumerable<MongoKeyPair<TKey, TValue>> castItems) 
                Collection.InsertMany(castItems);
            else
            {
                var mongoItems = itemsToAdd.Select(item => new MongoKeyPair<TKey, TValue>(item.KeyOne, item.KeyTwo));
                Collection.InsertMany(mongoItems);
            }
        }

        public void Commit()
        {
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            var filter = Builders<MongoKeyPair<TKey, TValue>>.Filter.Eq(pair => pair.KeyOne, key);
            var result = Collection.Find(filter).FirstOrDefault();
            value = result.KeyTwo;
            return true;
        }

        public bool Delete(TKey key)
        {
            var filter = Builders<MongoKeyPair<TKey, TValue>>.Filter.Eq(pair => pair.KeyOne, key);
            var deleteResult = Collection.DeleteOne(filter);

            if (deleteResult.IsAcknowledged == false || deleteResult.DeletedCount == 0) return false;
            return true;
        }
        
        public bool Delete(TKey key, TValue value)
        {
            var keyFilter = Builders<MongoKeyPair<TKey, TValue>>.Filter.Eq(pair => pair.KeyOne, key);
            var valueFilter = Builders<MongoKeyPair<TKey, TValue>>.Filter.Eq(pair => pair.KeyTwo, value);
            //var andFilter = Builders<MongoKeyPair<TKey, TValue>>.Filter.And(keyFilter, valueFilter);
            var deleteResult = Collection.DeleteOne(keyFilter & valueFilter);

            if (deleteResult.IsAcknowledged == false || deleteResult.DeletedCount == 0) return false;
            return true;
        }

        public TValue Get(TKey key)
        {
            var filter = Builders<MongoKeyPair<TKey, TValue>>.Filter.Eq(pair => pair.KeyOne, key);
            return Collection.Find(filter).FirstOrDefault().KeyTwo;
        }

        public void Dispose()
        {
        }

        public long GetCount()
        {
            return Collection.CountDocuments(_ => true);
        }

        public IEnumerable<IKeyPair<TKey, TValue>> GetItems()
        {
            return Collection.Find(_ => true).ToList();

        }

        public IEnumerable<IKeyPair<TKey, TValue>> GetItems(TKey startKey, TKey endKey)
        {
            var startKeyFilter = Builders<MongoKeyPair<TKey, TValue>>.Filter.Gte(pair => pair.KeyOne , startKey);
            var endKeyFilter = Builders<MongoKeyPair<TKey, TValue>>.Filter.Lte(pair => pair.KeyOne, endKey);
            //var andFilter = Builders<MongoKeyPair<TKey, TValue>>.Filter.And(startKeyFilter, endKeyFilter);

            return Collection.Find(startKeyFilter & endKeyFilter).ToList();
        }

        public IEnumerable<TKey> GetKeys()
        {
            return Collection.Find(_ => true).ToList().Select(item => item.KeyOne);
        }

        //public TKey GetNextKey()
        //{
        //    return KeyGenerator.GetNext();
        //}

        public IEnumerable<TValue> GetValues()
        {
            return Collection.Find(_ => true).ToList().Select(item => item.KeyTwo);
        }

        public void Rollback()
        {
            throw new NotImplementedException();
        }

        public bool TryGetValues(TKey[] keys, out TValue[] values)
        {
            var keyFilters = new FilterDefinition<MongoKeyPair<TKey, TValue>>[keys.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                keyFilters[i] = Builders<MongoKeyPair<TKey, TValue>>.Filter.Eq(item => item.KeyOne, keys[i]);
            }
            var orFilter = Builders<MongoKeyPair<TKey, TValue>>.Filter.Or(keyFilters);

            values = Collection.Find(orFilter).ToList().Select(item => item.KeyTwo).ToArray();
            return values.Any();
        }
    }
}