using System;
using System.Collections.Generic;
using System.Linq;
using Revert.Core.Common.Types.Tries;
using Revert.Core.Extensions;
using Revert.Core.Text.Tokenization;

namespace Revert.Core.Search.Testing
{
    /// <summary>
    /// TokenId = key, position(s) within text = values
    /// </summary>
    public class UIntTestingSearchable : ISearchable<uint, int>
    {
        private static uint currentTokenId = 0;
        public static Dictionary<string, uint> TokenIds = new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<uint, List<int>> valuesByKey = new Dictionary<uint, List<int>>();
        private static Dictionary<int, List<uint>> keysByValue = new Dictionary<int, List<uint>>();
        private static Trie<uint, int> recordTrie = new Trie<uint, int>();
        public IEnumerable<uint> AllKeys => valuesByKey.Keys.ToHashSet();
        public IEnumerable<int> AllValues => keysByValue.Keys.ToHashSet();

        private Trie<char, string> trailingTokenTrie = new Trie<char, string>();
        private Trie<char, string> leadingTokenTrie = new Trie<char, string>();

        public UIntTestingSearchable(string pathToTextFile)
        {
            FillRecordSet(pathToTextFile);
        }

        private void FillRecordSet(string pathToTextFile)
        {
            var text = System.IO.File.ReadAllText(pathToTextFile);
            var tokenizer = new SimpleTokenizer();
            var tokens = tokenizer.GetTokensWithIndex(text);

            uint[] tokenIds = new uint[tokens.Count];
            int i = 0;

            foreach (var token in tokens)
            {
                uint tokenId;
                if (!TokenIds.TryGetValue(token.Value, out tokenId))
                {
                    tokenId = ++currentTokenId;
                    TokenIds[token.Value] = tokenId;
                }
                tokenIds[i++] = tokenId;
                List<int> locations;
                if (!valuesByKey.TryGetValue(tokenId, out locations))
                {
                    locations = new List<int>();
                    valuesByKey[tokenId] = locations;
                    var charArray = token.Value.ToCharArray();
                    trailingTokenTrie.Add(charArray, token.Value);
                    Array.Reverse(charArray);
                    leadingTokenTrie.Add(charArray, token.Value);
                }

                keysByValue.AddToCollection(i, tokenId);
                locations.Add(token.Key);
            }
            recordTrie.Add(tokenIds, i);
        }

        public List<uint> GetKeysFromTrailingWildCard(string preWildCardValue)
        {
            return trailingTokenTrie.Evaluate(preWildCardValue.ToCharArray()).Select(t => TokenIds[t]).ToList();
        }

        public List<uint> GetKeysFromLeadingWildCard(string postWildCardValue)
        {
            return leadingTokenTrie.Evaluate(postWildCardValue.ToCharArray()).Select(t => TokenIds[t]).ToList();
        }

        public IEnumerable<int> GetMatches(uint key)
        {
            List<int> matches;
            if (!valuesByKey.TryGetValue(key, out matches)) matches = new List<int>();
            return matches;
        }

        public IEnumerable<int> GetMatches(uint[] keyVector, bool intersect = true)
        {
            var allValues = new HashSet<int>();
            var allMatches = new List<List<int>>();

            foreach (var item in keyVector)
            {
                List<int> ids;
                if (!valuesByKey.TryGetValue(item, out ids))
                    if (intersect) return new List<int>();
                    else continue;

                allMatches.Add(ids);
            }

            foreach (var matches in allMatches)
            {
                if (allValues.Count == 0 || !intersect) allValues.UnionWith(matches);
                else
                {
                    allValues.IntersectWith(matches);
                    if (allValues.Count == 0) return allValues;
                }
            }
            return allValues;
        }

        public IEnumerable<int> GetLiteralMatches(uint[] keyVector)
        {
            return recordTrie.Evaluate(keyVector);
        }

        public IEnumerable<int> GetLiteralMatches(string literalString)
        {
            throw new NotImplementedException();
        }

        public uint[] GetKeysFromString(string value)
        {
            var simpleTokenizer = new SimpleTokenizer();
            var tokens = simpleTokenizer.GetTokens(value);

            uint[] tokenIds = new uint[tokens.Count];

            for (int i = 0; i < tokens.Count; i++)
            {
                tokenIds[i] = GetKeyFromString(tokens[i]);
            }
            return tokenIds;
        }

        public uint[] GetKeys(int vertexId)
        {
            List<uint> positions;
            if (!keysByValue.TryGetValue(vertexId, out positions)) throw new Exception($"The vertexId with id {vertexId} was not found.");
            return positions.ToArray();
        }

        public uint GetKeyFromString(string key)
        {
            uint id;
            TokenIds.TryGetValue(key, out id);
            return id;
        }

        public IEnumerable<int> GetMatches(string[] keyVector, bool intersect = true)
        {
            var allValues = new HashSet<int>();
            var allMatches = new List<List<int>>();

            foreach (var item in keyVector)
            {
                uint tokenId;
                if (!TokenIds.TryGetValue(item, out tokenId))
                    if (intersect) return new List<int>();
                    else continue;

                List<int> ids;
                if (!valuesByKey.TryGetValue(tokenId, out ids))
                    if (intersect) return new List<int>();
                    else continue;

                allMatches.Add(ids);
            }

            foreach (var matches in allMatches)
            {
                if (allValues.Count == 0 || !intersect)
                    allValues.UnionWith(matches);
                else
                {
                    allValues.IntersectWith(matches);
                    if (allValues.Count == 0) return allValues;
                }
            }
            return allValues;
        }

        public uint GetKey(string value)
        {
            return TokenIds.TryReturnValue(value);
        }
    }
}