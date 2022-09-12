using System.Numerics;
using Revert.Core.Common;
using Revert.Core.IO.Serialization;

namespace Revert.Core.Indexing
{
    public class ByteArrayKeyedIndex<TValue> : KeyedIndex<byte[], TValue>
    {
        public ByteArrayKeyedIndex(string connectionString, string databaseName, string collectionName, KeyGenerator<byte[]> keyGenerator) : 
            base(connectionString, databaseName, collectionName, keyGenerator)
        {
        }

        protected override byte[] GetKeyBytes(byte[] key)
        {
            return key;
        }
    }
}