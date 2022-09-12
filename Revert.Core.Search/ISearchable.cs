using System.Collections.Generic;

namespace Revert.Core.Search
{
    public interface ISearchable<TKey, TValue>
    {
        IEnumerable<TKey> AllKeys { get; }
        IEnumerable<TValue> AllValues { get; }

        List<TKey> GetKeysFromTrailingWildCard(string preWildCardValue);
        List<TKey> GetKeysFromLeadingWildCard(string postWildCardValue);

        IEnumerable<TValue> GetMatches(TKey key);
        IEnumerable<TValue> GetMatches(TKey[] keyVector, bool intersect = true);
        IEnumerable<TValue> GetLiteralMatches(TKey[] keyVector);
        IEnumerable<TValue> GetLiteralMatches(string literalString);
        TKey GetKeyFromString(string value);
        TKey[] GetKeysFromString(string value);
        TKey[] GetKeys(TValue vertexId);
    }
}