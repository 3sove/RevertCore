using System;
using System.Collections.Generic;
using System.Linq;
using Revert.Core.Common.Types.Tries;
using Revert.Core.Extensions;
using Revert.Core.Text.Tokenization;

namespace Revert.Core.Search.Testing
{
    public class StringTestingSearchable : ISearchable<string, int>
    {
        private static Dictionary<string, List<int>> recordSet = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<int, string> keysByPosition = new Dictionary<int, string>();
        public IEnumerable<string> AllKeys => recordSet.Keys.ToHashSet();
        public IEnumerable<int> AllValues => keysByPosition.Keys.ToHashSet();

        private static Dictionary<int, string> keyByValue = new Dictionary<int, string>();

        private Trie<char, string> trailingTokenTrie = new Trie<char, string>();
        private Trie<char, string> leadingTokenTrie = new Trie<char, string>();

        public StringTestingSearchable(string pathToTextFile)
        {
            FillRecordSet(pathToTextFile);
        }

        private static int tokenId = 0;
        private void FillRecordSet(string pathToTextFile)
        {
            var text = System.IO.File.ReadAllText(pathToTextFile);
            var tokenizer = new SimpleTokenizer();
            var tokens = tokenizer.GetTokensWithIndex(text);

            foreach (var token in tokens)
            {
                List<int> positions;
                if (!recordSet.TryGetValue(token.Value, out positions))
                {
                    recordSet[token.Value] = positions = new List<int>();
                }
                positions.Add(token.Key);
                keysByPosition[token.Key] = token.Value;
                var charArray = token.Value.ToCharArray();
                trailingTokenTrie.Add(charArray, token.Value);
                Array.Reverse(charArray);
                leadingTokenTrie.Add(charArray, token.Value);
            }
        }

        public List<string> GetKeysFromTrailingWildCard(string preWildCardValue)
        {
            return trailingTokenTrie.Evaluate(preWildCardValue.ToCharArray());
        }

        public List<string> GetKeysFromLeadingWildCard(string postWildCardValue)
        {
            return leadingTokenTrie.Evaluate(postWildCardValue.ToCharArray());
        }

        public IEnumerable<int> GetMatches(string key)
        {
            List<int> matches;
            if (!recordSet.TryGetValue(key, out matches)) matches = new List<int>();
            return matches;
        }

        public IEnumerable<int> GetMatches(string[] keyVector, bool intersect = true)
        {
            IEnumerable<int> allMatches = new List<int>();

            foreach (var item in keyVector)
            {
                List<int> ids;
                if (!recordSet.TryGetValue(item, out ids))
                    if (intersect) return new List<int>();
                    else continue;

                if (intersect)
                {
                    if (!allMatches.Any()) allMatches = ids;
                    else allMatches = allMatches.Intersect(ids);

                    if (!allMatches.Any()) return allMatches;
                }
                else allMatches = allMatches.Union(ids);
            }

            return allMatches;
        }

        public IEnumerable<int> GetLiteralMatches(string[] keyVector)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<int> GetLiteralMatches(string literalString)
        {
            throw new NotImplementedException();
        }

        public string GetKeyFromString(string value)
        {
            return value;
        }

        public string[] GetKeysFromString(string value)
        {
            var simpleTokenizer = new SimpleTokenizer();
            return simpleTokenizer.GetTokens(value).ToArray();
        }

        public string[] GetKeys(int vertexId)
        {
            string key;
            if (!keyByValue.TryGetValue(vertexId, out key)) return new string[0];
            return new[] { key };
        }

        public string[] GetOrderedKeys(int value)
        {
            throw new NotImplementedException();
        }

        public string GetKey(string value)
        {
            return value;
        }
    }
}