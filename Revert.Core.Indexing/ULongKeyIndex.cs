using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Revert.Core.Common;
using Revert.Core.Extensions;
using Revert.Core.IO.Serialization;
using Revert.Core.IO.Stores;

namespace Revert.Core.Indexing
{
    public class ULongKeyIndex<TValue> : KeyedIndex<ulong, TValue>
    {
        public ULongKeyIndex(string connectionString, string databaseName, string collectionName, KeyGenerator<ulong> idIssuer = null) : base(connectionString, databaseName, collectionName, idIssuer ?? new KeyGenerator<ulong>(0, arg => ++arg))
        {
        }

        protected override byte[] GetKeyBytes(ulong key)
        {
            return BitConverter.GetBytes(key);
        }
    }
}
