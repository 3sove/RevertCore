using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MongoDB.Bson;
using Revert.Core.Common;
using Revert.Core.Common.Types;
using Revert.Core.Extensions;
using Revert.Core.IO.Stores;
using Revert.Core.IO.Serialization;

namespace Revert.Core.Indexing.Tries
{
    public class TrieIndex<T>
    {
        public KeyedIndex<ObjectId> NodeIdsByTokenId { get; set; }
        public MongoKeyValueStore<ObjectId, ObjectId> RootNodeIdByTokenId { get; set; }
        public IKeyValueStore<KeyPair<ObjectId, ObjectId>, ObjectId> ChildByNodeIdAndTokenId { get; set; }
        public KeyedIndex<T> ValuesByNodeId { get; set; }

        //public BPlusTree<byte[], uint> ChildCountByNodeIdAndTokneId { get; set; } 

        public TrieIndex(string connectionString, string databaseName, string collectionName)
        {
            NodeIdsByTokenId = new KeyedIndex<ObjectId>(connectionString, databaseName, $"{collectionName}_NodeIdsByTokenId");

            RootNodeIdByTokenId = new MongoKeyValueStore<ObjectId, ObjectId>(connectionString, databaseName, $"{collectionName}_RootNodeIdsByTokenId");

            //ChildByNodeIdAndTokenId = new KeyValueStore<KeyPair<ObjectId, ObjectId>, ObjectId>(connectionString, databaseName, $"{collectionName}_ChildByNodeIdAndTokenId",
            //        new KeyGenerator<KeyPair<ObjectId, ObjectId>>(
            //            new KeyPair<ObjectId, ObjectId>(ObjectId.GenerateNewId(), ObjectId.GenerateNewId()),
            //        pair => new KeyPair<ObjectId, ObjectId>(pair.KeyOne, ObjectId.GenerateNewId())));

            ValuesByNodeId = new KeyedIndex<T>(connectionString, databaseName, $"{collectionName}_ValuesByNodeId");
        }

        public void Remove(ObjectId[] tokenArray, T value)
        {
            if (tokenArray == null || !tokenArray.Any()) return;

            ObjectId rootToken = tokenArray[0];
            ObjectId nodeId = ObjectId.Empty;
            if (!RootNodeIdByTokenId.TryGetValue(rootToken, out nodeId))
                return;

            HashSet<T> nodeValues;

            if (!ValuesByNodeId.TryGetValue(nodeId, out nodeValues)) nodeValues = new HashSet<T>();
            nodeValues.Remove(value);
            ValuesByNodeId.Update(nodeId, nodeValues);

            for (int i = 1; i < tokenArray.Length; i++)
            {
                var token = tokenArray[i];
                var tokenBytes = token.ToByteArray();
                var nodeIdBytes = nodeId.ToByteArray();
                var nodeAndTokenKey = new byte[tokenBytes.Length + nodeIdBytes.Length];
                Array.Copy(nodeIdBytes, 0, nodeAndTokenKey, nodeAndTokenKey.Length - nodeIdBytes.Length, nodeIdBytes.Length);
                Array.Copy(tokenBytes, 0, nodeAndTokenKey, 0, tokenBytes.Length);

                var key = new KeyPair<ObjectId, ObjectId>(nodeId, token);

                if (ChildByNodeIdAndTokenId.TryGetValue(key, out nodeId))
                {
                    if (ValuesByNodeId.TryGetValue(nodeId, out nodeValues))
                    {
                        nodeValues.Remove(value);
                        ValuesByNodeId.Update(nodeId, nodeValues);
                    }

                    if (nodeValues.Count == 0)
                    {
                        NodeIdsByTokenId.Delete(token, nodeId);
                    }
                }
            }
        }


        public void Add(ObjectId[] tokenArray, T value)
        {
            if (tokenArray == null || !tokenArray.Any()) return;

            ObjectId rootToken = tokenArray[0];
            ObjectId nodeId;
            if (!RootNodeIdByTokenId.TryGetValue(rootToken, out nodeId))
            {
                nodeId = ObjectId.GenerateNewId();
                RootNodeIdByTokenId.Upsert(rootToken, nodeId);
                NodeIdsByTokenId.Upsert(rootToken, nodeId);
            }

            ValuesByNodeId.Upsert(nodeId, value);

            //Iterate through the elements of the array and find the corresponding Node for the preceeding array
            for (int i = 1; i < tokenArray.Length; i++)
            {
                var token = tokenArray[i];
                var nodeAndTokenKey = new KeyPair<ObjectId, ObjectId>(nodeId, tokenArray[i]);

                if (!ChildByNodeIdAndTokenId.TryGetValue(nodeAndTokenKey, out nodeId))
                {
                    nodeId = ObjectId.GenerateNewId();
                    ChildByNodeIdAndTokenId.Upsert(nodeAndTokenKey, nodeId);
                    NodeIdsByTokenId.Upsert(token, nodeId);
                }

                ValuesByNodeId.Upsert(nodeId, value);
            }
        }

        public bool TryGetValues(ObjectId[] tokenArray, out HashSet<T> values)
        {
            if (tokenArray == null || !tokenArray.Any())
            {
                values = null;
                return false;
            }

            var rootToken = tokenArray[0];

            HashSet<ObjectId> rootNodeIds;
            if (!NodeIdsByTokenId.TryGetValue(rootToken, out rootNodeIds))
            {
                values = null;
                return false;
            }

            values = new HashSet<T>();
            foreach (var rootNodeId in rootNodeIds)
            {
                ObjectId nodeId = rootNodeId;

                for (int i = 1; i < tokenArray.Length; i++)
                {
                    if (i == tokenArray.Length)
                    {
                        HashSet<T> branchValues;
                        ValuesByNodeId.TryGetValue(nodeId, out branchValues);
                        foreach (var item in branchValues)
                            values.Add(item);
                    }

                    var token = tokenArray[i];
                    var childKey = new KeyPair<ObjectId, ObjectId>(nodeId, token);

                    if (!ChildByNodeIdAndTokenId.TryGetValue(childKey, out nodeId))
                        break;
                }
            }
            return values.Any();
        }
    }
   
}
