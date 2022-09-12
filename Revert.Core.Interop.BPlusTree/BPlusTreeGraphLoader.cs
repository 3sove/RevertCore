using Revert.Core.Common;
using Revert.Core.Extensions;
using Revert.Core.Graph;
using Revert.Core.IO.Stores;

namespace Revert.Core.Interop.BPlusTree
{
    public class BPlusTreeGraphLoader<TVertex> : IGraphLoader<ulong, TVertex>
    {
        public BPlusTreeGraphLoader()
        {
        }

        public IKeyValueStore<TKey, TValue> LoadKeyStore<TKey, TValue>(string directoryPath, string fileName, IKeyGenerator<TKey> keyGenerator)
        {
            return new BPlusTreeStore<TKey, TValue>(directoryPath.AddFilePath(fileName), keyGenerator);
        }
    }




}
