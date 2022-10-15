using FASTER.core;
using Revert.Core.Extensions;
using Revert.Core.IO.Stores;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Revert.Core.Common;
using Revert.Core.Common.Types;
using Revert.Core.Mathematics;

namespace Revert.Core.IO.Stores
{
    public partial class FasterKeyValueStore<TKey, TValue> : IKeyValueStore<TKey, TValue>
    {
        private readonly IKeyGenerator<TKey> keyGenerator;

        public FasterKeyValueStore(string directory, IKeyGenerator<TKey> keyGenerator)
        {
            this.keyGenerator = keyGenerator;
            Directory = directory;

            if (!System.IO.Directory.Exists(directory)) System.IO.Directory.CreateDirectory(directory);
            MainLog = Devices.CreateLogDevice(directory.AddFilePath("main.log"), preallocateFile: true);
            ObjectLog = Devices.CreateLogDevice(directory.AddFilePath("object.log"), preallocateFile: true);

            SerializerSettings = new SerializerSettings<TKey, TValue>
            {
                keySerializer = () => new FasterProtobufSerializer<TKey>(),
                valueSerializer = () => new FasterProtobufSerializer<TValue>()
            };

            var logSettings = new LogSettings
            {
                LogDevice = MainLog,
                ObjectLogDevice = ObjectLog
            };

            var checkpointSettings = new CheckpointSettings
            {
                
                CheckpointDir = directory
                //CheckPointType = CheckpointType.FoldOver
            };

            Store = new FasterKV<TKey, TValue>(1L << 20, logSettings, serializerSettings: SerializerSettings, checkpointSettings: checkpointSettings);
            try
            {
                Store.Recover();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error recovering Store - {Directory}. {ex.GetBaseException().Message}");
            }
        }

        public void ClearSession()
        {
            session = null;
        }

        private ClientSession<TKey, TValue, TValue, TValue, Empty, SimpleFunctions<TKey, TValue>> session;
        public ClientSession<TKey, TValue, TValue, TValue, Empty, SimpleFunctions<TKey, TValue>> GetSession()
        {
            return session ?? (session = Store.For(new SimpleFunctions<TKey, TValue>())
                .NewSession<SimpleFunctions<TKey, TValue>>());
        }

        public string Directory { get; }
        public IDevice MainLog { get; private set; }
        public IDevice ObjectLog { get; private set; }
        public SerializerSettings<TKey, TValue> SerializerSettings { get; private set; }
        public FasterKV<TKey, TValue> Store { get; private set; }

        public long Count => GetCount();

        public void Upsert(TKey key, TValue value)
        {
            using (var session = GetSession())
            {
                var status = session.Upsert(key, value);
                if (status.IsPending) session.CompletePending(true, true);
            }

            //Store.Log.Flush(true);
        }

        public void UpsertRange(IEnumerable<IKeyPair<TKey, TValue>> itemsToAdd)
        {
            using (var session = GetSession())
            {
                foreach (var item in itemsToAdd)
                {
                    var upsertResult = session.Upsert(item.KeyOne, item.KeyTwo);
                    if (!upsertResult.IsCompleted) throw new Exception("Couldn't upsert item.");

                    if (upsertResult.IsPending)
                        session.CompletePending(true, true);
                }
            }
        }

        public void Commit()
        {
            using (var session = GetSession())
            {
                session.CompletePending(true, true);
                Store.TakeHybridLogCheckpointAsync(CheckpointType.FoldOver);
            }
            ClearSession();
        }


        public bool TryGetValue(TKey key, out TValue value)
        {
            using (var session = GetSession())
            {
                var readResult = session.Read(key);
                value = readResult.Item2;
                return readResult.Item1.IsCompletedSuccessfully;
            }
        }

        public bool Delete(TKey key)
        {
            using (var session = GetSession())
            {
                var deleteStatus = session.Delete(key);

                session.CompletePending(true);
                Store.Log.FlushAndEvict(true);
                return deleteStatus.IsCompletedSuccessfully;
            }
            //TODO: Verify this status code is correct for FASTER Deletes
        }

        public bool Delete(TKey key, TValue value)
        {
            using (var session = GetSession())
            {
                var deleteStatus = session.Delete(key);

                session.CompletePending(true);
                Store.Log.FlushAndEvict(true);
                return deleteStatus.IsCompletedSuccessfully;
            }
            //TODO: Verify this status code is correct for FASTER Deletes
        }



        public TValue Get(TKey key)
        {
            using (var session = GetSession())
            {
                var readResults = session.Read(key);
                if (readResults.Item1.IsCompletedSuccessfully)
                    return readResults.Item2;
                return default;
            }
        }

        public void Dispose()
        {
            Store.Dispose();
            MainLog.Dispose();
            ObjectLog.Dispose();
        }

        public long GetCount()
        {
            long i = 0;
            using (var session = GetSession())
            {
                using (var iterator = session.Iterate())
                {
                    while (iterator.GetNext(out var recordInfo))
                    {
                        i++;
                    }
                }
            }
            return i;
            //TODO: Find more efficient count than iteration
        }

        public IEnumerable<IKeyPair<TKey, TValue>> GetItems()
        {
            List<IKeyPair<TKey, TValue>> items = new List<IKeyPair<TKey, TValue>>();
            using (var session = GetSession())
            {
                using (var iterator = session.Iterate())
                {
                    while (iterator.GetNext(out var recordInfo))
                    {
                        items.Add(new Common.Types.KeyPair<TKey, TValue>(iterator.GetKey(), iterator.GetValue()));
                    }
                }
            }

            return items;
        }

        public IEnumerable<IKeyPair<TKey, TValue>> GetItems(TKey startKey, TKey endKey)
        {
            List<Common.Types.KeyPair<TKey, TValue>> items = new List<Common.Types.KeyPair<TKey, TValue>>();
            using (var session = GetSession())
            {
                using (var iterator = session.Iterate())
                {
                    bool startFound = false;

                    while (iterator.GetNext(out var recordInfo))
                    {
                        if (Equals(startKey, iterator.GetKey())) startFound = true;
                        if (Equals(endKey, iterator.GetKey())) break;

                        if (startFound == true) items.Add(new Common.Types.KeyPair<TKey, TValue>(iterator.GetKey(), iterator.GetValue()));
                    }
                }
            }
            return items;
            //TODO: Verify order and sorting of items in Faster iterator, create equality comparors
        }

        public IEnumerable<TKey> GetKeys()
        {
            List<TKey> keys = new List<TKey>();
            using (var session = GetSession())
            {
                using (var iterator = session.Iterate())
                {
                    while (iterator.GetNext(out var recordInfo))
                    {
                        keys.Add(iterator.GetKey());
                    }
                }
            }
            return keys;
        }

        public TKey GetNextKey()
        {
            return keyGenerator.GetNext();
        }

        public TKey GetLastKey()
        {
            using (var session = GetSession())
            {
                using (var iterator = session.Iterate())
                {
                    TKey lastKey = default(TKey);
                    while (iterator.GetNext(out var recordInfo))
                    {
                        lastKey = iterator.GetKey();
                    }
                    return lastKey;
                }
            }
        }

        public IEnumerable<TValue> GetValues()
        {
            List<TValue> items = new List<TValue>();
            using (var session = GetSession())
            {
                using (var iterator = session.Iterate())
                {
                    while (iterator.GetNext(out var recordInfo))
                    {
                        items.Add(iterator.GetValue());
                    }
                }
            }
            return items;
        }

        public void Rollback()
        {
            throw new NotImplementedException();
        }

        public bool TryGetValues(TKey[] key, out TValue[] value)
        {
            throw new NotImplementedException();
        }
    }
}