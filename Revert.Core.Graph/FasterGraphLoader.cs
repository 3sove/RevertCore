using Revert.Core.Extensions;
using Revert.Core.IO.Stores;
using System;
using System.Text;
using Revert.Core.Common;

namespace Revert.Core.Graph
{
    public class FasterGraphLoader<TVertex> : IGraphLoader<ulong, TVertex>
    {
        public IKeyValueStore<TKey, TValue> LoadKeyStore<TKey, TValue>(string directoryPath, string fileName, IKeyGenerator<TKey> keyGenerator)
        {
            return new FasterKeyValueStore<TKey, TValue>(directoryPath.AddFilePath(fileName), keyGenerator);
        }

    }

}
