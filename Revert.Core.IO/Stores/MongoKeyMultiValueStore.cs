using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using Revert.Core.Common;
using Revert.Core.Common.Types;
using Revert.Core.Extensions;

namespace Revert.Core.IO.Stores
{
    public class MongoKeyMultiValueStore<TKey, TValue> : IKeyMultiValueStore<TKey, TValue>
    {
        public string ConnectionString { get; }
        public string DatabaseName { get; }
        public string CollectionName { get; }
        public KeyGenerator<TKey> KeyGenerator { get; }

        private MongoClient mongoClient;
        private MongoClient MongoClient => mongoClient ?? (mongoClient = new MongoClient(ConnectionString));

        private IMongoDatabase _db;
        private IMongoDatabase db => _db ?? (_db = MongoClient.GetDatabase(DatabaseName));
        private IMongoCollection<MongoKeyPair<TKey, TValue>> Collection => db.GetCollection<MongoKeyPair<TKey, TValue>>(CollectionName);

        public MongoKeyMultiValueStore(string connectionString, string databaseName, string collectionName, KeyGenerator<TKey> keyGenerator)
        {
            ConnectionString = connectionString;
            DatabaseName = databaseName;
            CollectionName = collectionName;
            KeyGenerator = keyGenerator;
        }

        public void Dispose()
        {
        }

        public long Count => GetCount();

        public long GetCount()
        {
            return Collection.CountDocuments(_ => true);
        }

        public void Upsert(TKey key, TValue value)
        {
            Collection.InsertOne(new MongoKeyPair<TKey, TValue>(key, value));
        }


        IEnumerable<IEnumerable<TValue>> IKeyValueStore<TKey, IEnumerable<TValue>>.GetValues()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TValue> Get(TKey key)
        {
            var filter = Filter.Eq(pair => pair.KeyOne, key);
            return Collection.Find(filter).ToList().Select(item => item.KeyTwo);
        }

        public bool Delete(TKey key, IEnumerable<TValue> value)
        {
            throw new NotImplementedException();
        }

        IEnumerable<IKeyPair<TKey, IEnumerable<TValue>>> IKeyValueStore<TKey, IEnumerable<TValue>>.GetItems()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IKeyPair<TKey, TValue>> GetItems()
        {
            return Collection.Find(_ => true).ToList();

        }

        private FilterDefinitionBuilder<MongoKeyPair<TKey,TValue>> Filter { get; set; } = Builders<MongoKeyPair<TKey, TValue>>.Filter;

        public IEnumerable<IKeyPair<TKey, TValue>> GetItems(TKey startKey, TKey endKey)
        {
            var startKeyFilter = Filter.Gte(pair => pair.KeyOne, startKey);
            var endKeyFilter = Filter.Lte(pair => pair.KeyOne, endKey);
            //var andFilter = Filter.And(startKeyFilter, endKeyFilter);

            return Collection.Find(startKeyFilter & endKeyFilter).ToList();
        }


        public IEnumerable<TKey> GetKeys()
        {
            return Collection.Find(_ => true).ToList().Select(item => item.KeyOne);
        }

        public IEnumerable<TValue> GetValues()
        {
            return Collection.Find(_ => true).ToList().Select(item => item.KeyTwo);
        }

        public bool TryGetValue(TKey key, out IEnumerable<TValue> value)
        {
            var filter = Filter.Eq(pair => pair.KeyOne, key);
            value = Collection.Find(filter).ToList().Select(item => item.KeyTwo).ToArray();
            return value.Any();
        }

        public void Upsert(TKey key, IEnumerable<TValue> value)
        {
            Collection.InsertMany(value.Select(item => new MongoKeyPair<TKey, TValue>(key, item)));
        }

        public void UpsertRange(IEnumerable<IKeyPair<TKey, TValue>> itemsToAdd)
        {
            if (itemsToAdd is IEnumerable<MongoKeyPair<TKey, TValue>> castItems)
                Collection.InsertMany(castItems);
            else
            {
                var mongoItems = itemsToAdd.Select(item => new MongoKeyPair<TKey, TValue>(item.KeyOne, item.KeyTwo));
                if (mongoItems.Any()) Collection.InsertMany(mongoItems);
            }
        }

        public void UpsertRange(IEnumerable<IKeyPair<TKey, IEnumerable<TValue>>> itemsToAdd)
        {
            itemsToAdd = itemsToAdd as IKeyPair<TKey, IEnumerable<TValue>>[] ?? itemsToAdd.ToArray();
            var upsertItems = new List<MongoKeyPair<TKey, TValue>>();

            foreach (var keyPair in itemsToAdd)
            foreach (var value in keyPair.KeyTwo)
                upsertItems.Add(new MongoKeyPair<TKey, TValue>(keyPair.KeyOne, value));

            if (upsertItems.Any()) UpsertRange(upsertItems);
        }

        public bool Delete(TKey key)
        {
            var filter = Filter.Eq(pair => pair.KeyOne, key);
            var deleteResult = Collection.DeleteOne(filter);

            if (deleteResult.IsAcknowledged == false || deleteResult.DeletedCount == 0) return false;
            return true;
        }

        public bool Delete(TKey key, TValue value)
        {
            var keyFilter = Filter.Eq(pair => pair.KeyOne, key);
            var valueFilter = Filter.Eq(pair => pair.KeyTwo, value);
            //var andFilter = Filter.And(keyFilter, valueFilter);
            var deleteResult = Collection.DeleteOne(keyFilter & valueFilter);

            if (deleteResult.IsAcknowledged == false || deleteResult.DeletedCount == 0) return false;
            return true;
        }

        public bool TryGetValues(TKey[] keys, out MongoKeyPair<TKey, TValue>[] values)
        {
            var inFilter = Filter.In(item => item.KeyOne, keys);
            values = Collection.Find(inFilter).ToList().ToArray();
            return values.Any();

            //var filters = new FilterDefinition<MongoKeyPair<TKey, TValue>>[keys.Length];
            //for (int i = 0; i < keys.Length; i++)
            //{
            //    filters[i] = Filter.Eq(record => record.KeyOne, keys[i]);
            //}
            //var orFilter = Filter.Or(filters);

            //values = Collection.Find(orFilter).ToList().ToArray();
            //return values.Any();
        }

        public bool TryGetValues(TKey[] keys, out IEnumerable<TValue>[] values)
        {
            MongoKeyPair<TKey, TValue>[] mongoValues;
            if (!TryGetValues(keys, out mongoValues))
            {
                values = null;
                return false;
            }

            var valueEnumerable = mongoValues.Select(value => value.KeyTwo).ToList();
            values = new IEnumerable<TValue>[1];
            values[0] = valueEnumerable;
            return values.Any();
        }
    }
}