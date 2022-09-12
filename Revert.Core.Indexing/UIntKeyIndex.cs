using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Revert.Core.Common;
using Revert.Core.Extensions;
using Revert.Core.IO.Serialization;
using Revert.Core.IO.Stores;

namespace Revert.Core.Indexing
{
    public class UIntKeyIndex<TValue> : KeyedIndex<uint, TValue>
    {
        public UIntKeyIndex(string connectionString, string databaseName, string collectionName, KeyGenerator<uint> idIssuer) : 
            base(connectionString, databaseName, collectionName, idIssuer)
        {
        }

        protected override byte[] GetKeyBytes(uint key)
        {
            return BitConverter.GetBytes(key);
        }
    }
}
