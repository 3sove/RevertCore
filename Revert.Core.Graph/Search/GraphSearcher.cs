using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using Revert.Core.Extensions;
using Revert.Core.Indexing;
using Revert.Core.Search;
using Revert.Core.Text.Tokenization;
using Revert.Core.Common.Types.Tries;
using Revert.Core.Graph.Vertices;
using Revert.Core.IO.Stores;

namespace Revert.Core.Graph.Search
{
    public class GraphSearcher<TVertex> : ISearchable<ObjectId, ObjectId> where TVertex : class, IVertex, new()
    {
        //public IKeyMultiValueStore<ObjectId, ObjectId> VerticesByKey { get; set; }
        public string TokenIndexName { get; set; }
        public string NlpDictionaryPath { get; set; }
        public HashSet<string> StopList { get; set; }

        public Trie<char, ObjectId> TrailingWildCardTrie = null;
        public Trie<char, ObjectId> LeadingWildCardTrie = null;
        public Graph<TVertex> Graph { get; set; }

        public GraphSearcher(Graph<TVertex> graph)
        {
            Graph = graph;
        }


        public List<ObjectId> GetKeysFromTrailingWildCard(string preWildCardValue)
        {
            if (TrailingWildCardTrie == null)
            {
                var tokens = Graph.TokenIndex.GetTokens(); // TokenIndex.GetInstance( da databaseName:, NlpDictionaryPath).GetTokens();
                TrailingWildCardTrie = new Trie<char, ObjectId>();
                foreach (var token in tokens)
                {
                    var tokenValue = token.Value.ToLowerInvariant().ToCharArray();
                    TrailingWildCardTrie.Add(tokenValue, token.Id);
                }
            }
            return TrailingWildCardTrie.Evaluate(preWildCardValue.ToCharArray());
        }

        public List<ObjectId> GetKeysFromLeadingWildCard(string postWildCardValue)
        {
            if (LeadingWildCardTrie == null)
            {
                var tokens = Graph.TokenIndex.GetTokens(); // TokenIndex.GetInstance(Graph.connectionString, Graph.databaseName, Graph.tokenIndexName, Graph.NlpDictionaryPath).GetTokens();
                LeadingWildCardTrie = new Trie<char, ObjectId>();
                foreach (var token in tokens)
                {
                    var tokenValue = token.Value.ToLowerInvariant().Reverse().ToArray();
                    LeadingWildCardTrie.Add(tokenValue, token.Id);
                }
            }
            return LeadingWildCardTrie.Evaluate(postWildCardValue.Reverse().ToArray());
        }

        public IEnumerable<ObjectId> GetMatches(ObjectId key)
        {

            if (!Graph.VerticesByTokenId.TryGetValue(key, out var vertexIds)) return null;
            return vertexIds;
        }

        public IEnumerable<ObjectId> GetMatches(ObjectId[] keyVector, bool intersect = true)
        {
            if (keyVector == null || keyVector.Length == 0) return null;

            var distinctMatches = new HashSet<ObjectId>();

            IEnumerable<ObjectId>[] objectIds;
            if (Graph.VerticesByTokenId.TryGetValues(keyVector, out objectIds))
            {
                foreach (var idCollection in objectIds)
                    foreach (var id in idCollection)
                        distinctMatches.Add(id);
            }

            return distinctMatches;
        }

        public IEnumerable<ObjectId> GetLiteralMatches(ObjectId[] keyVector)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ObjectId> GetLiteralMatches(string literalString)
        {
            throw new NotImplementedException();
        }

        public ObjectId[] GetOrderedKeys(ObjectId value)
        {
            return Graph.GetOrderedKeys(value);
        }

        public ObjectId GetKeyFromString(string value)
        {
            return Graph.TokenIndex.GetTokenId(value);
        }

        public ObjectId[] GetKeysFromString(string value)
        {
            return Graph.TokenIndex.GetTokenIds(value, Graph.StopList);
        }

        public ObjectId[] GetKeys(ObjectId vertexId)
        {
            var vertex = Graph.GetVertex(vertexId);
            var vertexTokens = Graph.TokenIndex.GetTokenIds(vertex.Features.GetSearchableText(), Graph.StopList).ToHashSet();
            return vertexTokens.ToArray();
        }

        public List<Token> GetTokens(ObjectId[] tokenIds)
        {
            return Graph.TokenIndex.GetTokens();
        }

        public IEnumerable<ObjectId> AllKeys => Graph.VerticesByTokenId.GetKeys().ToHashSet();

        public IEnumerable<ObjectId> AllValues => Graph.VerticesByTokenId.GetValues().ToHashSet();

        public IEnumerable<ObjectId> GetMatches(string searchString)
        {
            return GetMatches(searchString, TokenIndexName, NlpDictionaryPath);
        }

        public IEnumerable<ObjectId> GetMatches(string searchString, string tokenIndexPath, string nlpDictionaryPath)
        {
            //var booleanSearch = new BooleanSearch<int, ulong>(searchString, this, StopList);
            //return booleanSearch.Execute();

            var search = new TextSearch(searchString);
            search.Evaluate(this, out var results);
            return results;
        }
    }
}
