using Revert.Core.Common;
using Revert.Core.IO.Stores;

namespace Revert.Core.Graph
{
    public interface IGraphLoader<TKey, TVertex>
    {
        IKeyValueStore<TK, TV> LoadKeyStore<TK, TV>(string directoryPath, string fileName, IKeyGenerator<TK> keyGenerator);

    }




}
