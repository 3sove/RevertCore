using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using Revert.Core.Common.Types;
using Revert.Core.Extensions;
using Revert.Core.IO;
using Revert.Core.IO.Serialization;
using Revert.Core.IO.Stores;

namespace Revert.Core.Indexing
{
    public class MongoRecordIndex<T> : IKeyValueStore<ObjectId, T> where T : IMongoRecord
    {
        public MongoRecordIndex(string connectionString, string databaseName, string collectionName, ISerializer<T> valueSerializer = null)
        {
            ConnectionString = connectionString;
            DatabaseName = databaseName;
            CollectionName = collectionName;
            ValueSerializer = valueSerializer ?? new ProtobufSerializer<T>();
        }

        public ISerializer<T> ValueSerializer { get; set; }
        public string CollectionName { get; set; }
        public string ConnectionString { get; }
        public string DatabaseName { get; set; }

        private MongoClient mongoClient;
        private MongoClient MongoClient => mongoClient ?? (mongoClient = new MongoClient(ConnectionString));

        private IMongoDatabase _db;
        private IMongoDatabase db => _db ?? (_db = MongoClient.GetDatabase(DatabaseName));

        private IMongoCollection<T> Collection => db.GetCollection<T>(CollectionName);

        public void Add(T item)
        {
            if (!Collection.Find(Builders<T>.Filter.Eq(record => record.Id, item.Id)).Any()) Collection.InsertOne(item);
        }

        public void Add(IEnumerable<T> items)
        {
            var itemsArray = items as T[] ?? items.ToArray();
            var idFilter = Builders<T>.Filter.Or(itemsArray.Select(item => Builders<T>.Filter.Eq(record => record.Id, item.Id)));
            var existingItems = Collection.Find(idFilter).ToList().Select(item => item.Id).ToHashSet();

            Collection.InsertMany(itemsArray.Where(item => !existingItems.Contains(item.Id)));
        }

        public bool Remove(ObjectId id)
        {
            var filter = Builders<T>.Filter.Eq(record => record.Id, id);
            var result = Collection.DeleteOne(filter);
            return result.IsAcknowledged;
        }

        public bool Remove(T item)
        {
            var filter = Builders<T>.Filter.Eq(record => record.Id, item.Id);
            //var result = Collection.Find(filter).FirstOrDefault();
            var result = Collection.DeleteOne(filter);
            return result.IsAcknowledged;
        }

        public bool Remove(IEnumerable<T> items)
        {
            var orFilters = new List<FilterDefinition<T>>();
            foreach (var item in items)
                orFilters.Add(Builders<T>.Filter.Eq(record => record.Id, item.Id));

            var orFilter = Builders<T>.Filter.Or(orFilters);
            var result = Collection.DeleteMany(orFilter);
            return result.IsAcknowledged;
        }

        public void Update(T value)
        {
            var filter = Builders<T>.Filter.Eq(record => record.Id, value.Id);
            Collection.ReplaceOne(filter, value);
        }

        public void Update(IEnumerable<T> values)
        {
            try
            {
                foreach (var value in values)
                {
                    var filter = Builders<T>.Filter.Eq(record => record.Id, value.Id);
                    Collection.ReplaceOne(filter, value);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public T GetValue(ObjectId id)
        {
            var filter = Builders<T>.Filter.Eq(record => record.Id, id);
            return Collection.Find(filter).FirstOrDefault();
        }

        public T Get(ObjectId key)
        {
            var filter = Builders<T>.Filter.Eq(record => record.Id, key);
            return Collection.Find(filter).FirstOrDefault();
        }

        public bool TryGetValues(ObjectId[] ids, out T[] values)
        {
            var filters = new FilterDefinition<T>[ids.Length];
            for (int i = 0; i < ids.Length; i++)
                filters[i] = Builders<T>.Filter.Eq(record => record.Id, ids[i]);

            var inFilter = Builders<T>.Filter.In(item => item.Id, ids);
            var returnValues = Collection.Find(inFilter).ToList();
            //var returnValues = Collection.Find(orKey).ToList();
            values = returnValues.ToArray();
            return values.Any();
        }


        public bool TryGetValue(ObjectId id, out T value)
        {
            var filterKey = Builders<T>.Filter.Eq(record => record.Id, id);
            value = Collection.Find(filterKey).FirstOrDefault();
            return value != null;
        }

        public IEnumerable<ObjectId> Keys => GetKeys();

        public IEnumerable<IKeyPair<ObjectId, T>> GetItems()
        {
            return Values.Select(v => new KeyPair<ObjectId, T>(v.Id, v));
        }

        IEnumerable<ObjectId> IKeyValueStore<ObjectId, T>.GetKeys()
        {
            return GetKeys();
        }

        public IEnumerable<T> GetValues()
        {
            return Values;
        }

        private IEnumerable<ObjectId> GetKeys()
        {
            return Collection.Find(_ => true).ToList().Select(item => item.Id);
        }

        public IEnumerable<T> Values => Collection.Find(_ => true).ToEnumerable();

        public long GetCount()
        {
            return Collection.CountDocuments(_ => true);
        }

        public void Upsert(ObjectId key, T value)
        {
            if (key != value.Id) throw new Exception("MongoRecordIndex Upsert exception.  ObjectIds should match.");

            var filter = Builders<T>.Filter.Eq(record => record.Id, key);
            var existingRecord = Collection.Find(filter).FirstOrDefault();

            if (existingRecord == null)
            {
                Add(value);
            }
            else
            {
                Update(value);
            }
        }

        public void UpsertRange(IEnumerable<IKeyPair<ObjectId, T>> itemsToAdd)
        {
            var idFilters = new List<FilterDefinition<T>>();
            var upsertItems = itemsToAdd as IKeyPair<ObjectId, T>[] ?? itemsToAdd.ToArray();
            upsertItems.ForEach(item => idFilters.Add(Builders<T>.Filter.Eq(record => record.Id, item.KeyOne)));
            var orFilter = Builders<T>.Filter.Or(idFilters);

            var existingRecords = Collection.Find(orFilter).ToList().Select(item => item.Id).ToHashSet();
            
            Add(upsertItems.Where(item => !existingRecords.Contains(item.KeyOne)).Select(item => item.KeyTwo));
            Update(upsertItems.Where(item => existingRecords.Contains(item.KeyOne)).Select(item => item.KeyTwo));
        }

        public bool Delete(ObjectId key)
        {
            return Remove(key);
        }

        public bool Delete(ObjectId key, T value)
        {
            var keyFilter = Builders<T>.Filter.Eq(record => record.Id, key);
            var valueFilter = Builders<T>.Filter.Eq(record => record, value);
            //var andFilter = Builders<T>.Filter.And(keyFilter, valueFilter);
            var result = Collection.DeleteOne(keyFilter & valueFilter);
            return result.IsAcknowledged;
        }
    }
}
