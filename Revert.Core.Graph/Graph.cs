using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using MongoDB.Bson;
using Revert.Core.Extensions;
using Revert.Core.Graph.Edges;
using Revert.Core.Graph.MetaData;
using Revert.Core.Graph.Search;
using Revert.Core.Graph.Vertices;
using Revert.Core.Indexing;
using Revert.Core.Search;
using Revert.Core.Text.Tokenization;
using Revert.Core.Common.Types;
using Revert.Core.Graph.MetaData.DataPoints;
using Revert.Core.IO;
using Revert.Core.IO.Stores;

namespace Revert.Core.Graph
{
    public class Graph<TVertex> : IGraph<TVertex> where TVertex : class, IVertex, IMongoRecord, new()
    {
        public readonly string connectionString;
        public readonly string databaseName;
        public readonly string graphName;
        public readonly string tokenIndexName;

        public GraphSearcher<TVertex> searcher;

        public MongoRecordIndex<TVertex> Vertices { get; set; }
        public IKeyValueStore<ObjectId, TVertex> GetVertices()
        {
            return Vertices;
        }

        public MongoRecordIndex<Clique> Cliques { get; set; }
        public IKeyValueStore<ObjectId, Clique> GetCliques()
        {
            return Cliques;
        }

        public IKeyMultiValueStore<ObjectId, ObjectId> VerticesByTokenId { get; set; }
        //internal TrieIndex<ObjectId> VerticesByTokenTrie { get; set; }


        private TokenIndex tokenIndex;
        public TokenIndex TokenIndex
        {
            get => tokenIndex ?? (tokenIndex = TokenIndex.GetInstance(connectionString, databaseName, tokenIndexName, NlpDictionaryPath));
            set => tokenIndex = value;
        }

        public string NlpDictionaryPath { get; set; }

        private HashSet<string> stopList;
        public HashSet<string> StopList
        {
            get => stopList ?? (stopList = new HashSet<string>()); //  (stopList = Text.NLP.StopList.Words);
            set => stopList = value;
        }

        public bool IsLoaded { get; set; }
        protected ReaderWriterLockSlim RwLock { get; set; } = new ReaderWriterLockSlim();

        public Graph(string connectionString, string databaseName, string graphName, string tokenIndexName)
        {
            this.databaseName = databaseName;
            this.graphName = graphName;
            this.connectionString = connectionString;
            this.tokenIndexName = tokenIndexName;

            LoadData();
        }

        private void LoadData()
        {
            while (!RwLock.TryEnterWriteLock(1000)) Debug.WriteLine("Graph Load lock tick");
            try
            {
                if (IsLoaded) return;

                Console.WriteLine("Initializing indices");

                VerticesByTokenId = new MongoKeyMultiValueStore<ObjectId, ObjectId>(connectionString, databaseName,
                    $"{graphName}_VerticesByKey", ObjectIdKeyIssuer.Instance);
                //VerticesByTokenTrie = new TrieIndex<ObjectId>(connectionString, databaseName, $"{graphName}_VerticesByKeyTrie");

                Vertices = new MongoRecordIndex<TVertex>(connectionString, databaseName, $"{graphName}_Vertices");
                Cliques = new MongoRecordIndex<Clique>(connectionString, databaseName, $"{graphName}_Cliques");

                Console.WriteLine("Graph loading completed");
                IsLoaded = true;
            }
            finally
            {
                RwLock.ExitWriteLock();
            }
        }

        public IEnumerable<TVertex> GetRecentEntities(int count)
        {
            return Vertices.Values.OrderByDescending(item => item.Features?.LastUpdated).Take(count);
        }

        public long GetCount()
        {
            return Vertices.GetCount();
        }

        TVertex IGraph<TVertex>.GetVertex(ObjectId id)
        {
            return GetVertex(id);
        }
        
        public List<ObjectId> GetNeighborIds(TVertex vertex, bool includeEdges = true, bool includeCliques = true)
        {
            var entityIds = new List<ObjectId>();
            var cliques = GetCliques();
            if (includeCliques) vertex.CliqueIds.ForEach(cliqueId => entityIds.AddRange(cliques.Get(cliqueId).VertexIds));
            if (includeEdges) entityIds.AddRange(vertex.Edges.Select(edge => edge.VertexId));
            return entityIds;
        }

        public HashSet<TVertex> GetNeighbors(TVertex vertex, bool includeEdges = true, bool includeCliques = true)
        {
            var neighbors = new HashSet<TVertex>(new VertexEqualityComparer());

            if (includeCliques)
                foreach (var id in vertex.CliqueIds)
                {
                    var clique = GetCliques().Get(id);
                    foreach (var vertexId in clique.VertexIds)
                    {
                        neighbors.Add(GetVertex(vertexId));
                    }
                }

            if (includeEdges)
                foreach (var edge in vertex.Edges)
                {
                    neighbors.Add(GetVertex(edge.VertexId));
                }
            return neighbors.Where(n => n != null).ToHashSet();
        }


        private void DeleteSearchTokens(TVertex vertex)
        {
            var vertexTokens = GetVertexTokens(vertex);

            var verticesByTokenId = new Dictionary<ObjectId, HashSet<ObjectId>>();

            foreach (var tokenId in vertexTokens)
                verticesByTokenId.AddToCollection(tokenId.Key, vertex.Id);

            var orderedKeysAndVertexIds = new List<KeyValuePair<ObjectId[], ObjectId>>();
            var orderedTokens = vertex.Features.GetOrderedSearchableTokens(TokenIndex, StopList);
            orderedKeysAndVertexIds.Add(new KeyValuePair<ObjectId[], ObjectId>(orderedTokens, vertex.Id));

            //TODO: Delete search tokens
        }


        private void UpsertSearchTokens(IEnumerable<TVertex> vertices)
        {
            var bulkVerticesByTokenId = new Dictionary<ObjectId, HashSet<ObjectId>>();
            foreach (var vertex in vertices)
            {
                Dictionary<ObjectId, int> newTokensWithCount = vertex.Features.GetSearchableTokensWithCount(TokenIndex, StopList);
                IEnumerable<ObjectId> tokensToAdd;

                if (Vertices.TryGetValue(vertex.Id, out var oldVertex))
                {
                    var oldTokensByCount = oldVertex.Features.GetSearchableTokensWithCount(TokenIndex, StopList);
                    var tokensToRemove = oldTokensByCount.Keys.Where(k => !newTokensWithCount.ContainsKey(k));

                    foreach (var token in tokensToRemove)
                    {
                        if (VerticesByTokenId.TryGetValue(token, out var values))
                        {
                            VerticesByTokenId.Upsert(token, values.Where(v => v != oldVertex.Id));
                        }

                        //TODO: Upsert removing by token tries
                    }

                    tokensToAdd = newTokensWithCount.Keys.Where(k => !oldTokensByCount.ContainsKey(k)).ToArray();
                }
                else
                {
                    tokensToAdd = newTokensWithCount.Keys.ToArray();
                }

                foreach (var token in tokensToAdd)
                    bulkVerticesByTokenId.AddToCollection(token, vertex.Id);
            }

            var upsertItems = new List<KeyPair<ObjectId, ObjectId>>();
            foreach (var item in bulkVerticesByTokenId)
                foreach (var vertexId in item.Value)
                    upsertItems.Add(new KeyPair<ObjectId, ObjectId>(item.Key, vertexId));

            VerticesByTokenId.UpsertRange(upsertItems);

            //foreach (var token in tokensToAdd)
            //    VerticesByTokenId.Upsert(token, vertex.Id);

            //VerticesByTokenId.Upsert();
        }

        private void UpsertSearchTokens(TVertex vertex)
        {
            var newTokensWithCount = vertex.Features.GetSearchableTokensWithCount(TokenIndex, StopList);
            IEnumerable<ObjectId> tokensToAdd;

            if (Vertices.TryGetValue(vertex.Id, out var oldVertex))
            {
                var oldTokensByCount = oldVertex.Features.GetSearchableTokensWithCount(TokenIndex, StopList);
                var tokensToRemove = oldTokensByCount.Keys.Where(k => !newTokensWithCount.ContainsKey(k));

                foreach (var token in tokensToRemove)
                {
                    if (VerticesByTokenId.TryGetValue(token, out var vertices))
                    {
                        VerticesByTokenId.Upsert(token, vertices.Where(v => v != oldVertex.Id));
                    }

                    //TODO: Upsert removing by token tries
                }

                tokensToAdd = newTokensWithCount.Keys.Where(k => !oldTokensByCount.ContainsKey(k)).ToArray();

                foreach (var token in tokensToAdd)
                {
                    VerticesByTokenId.Upsert(token, vertex.Id);
                }
            }
            else
            {
                tokensToAdd = newTokensWithCount.Keys.ToArray();
            }

            var bulkVerticesByTokenId = new Dictionary<ObjectId, HashSet<ObjectId>>();

            foreach (var token in tokensToAdd)
                bulkVerticesByTokenId.AddToCollection(token, vertex.Id);

            var upsertItems = new List<KeyPair<ObjectId, ObjectId>>();

            foreach (var tokenId in tokensToAdd)
                upsertItems.Add(new MongoKeyPair<ObjectId, ObjectId>(tokenId, vertex.Id));

            if (upsertItems.Any()) VerticesByTokenId.UpsertRange(upsertItems);

            //foreach (var token in tokensToAdd)
            //    VerticesByTokenId.Upsert(token, vertex.Id);

            //VerticesByTokenId.Upsert();
        }

        public bool Add(TVertex vertex, bool resolveVertex = true)
        {
            lock (Vertices)
            {
                bool isNew = true;
                TVertex resolvedVertex = resolveVertex ? ResolveVertex(vertex, out isNew) : vertex;

                if (resolvedVertex == null) throw new Exception("Vertex resolution resulted in a null value.");

                UpsertSearchTokens(resolvedVertex);
                Vertices.Add(resolvedVertex);
                return isNew;
            }
        }

        public bool DeleteVertex(IVertex vertex)
        {
            //TODO: delete all traces of vertex

            return Vertices.Remove(vertex.Id);
        }

        private TVertex ResolveVertex(TVertex vertex, out bool isNew)
        {
            //TODO: improve performance
            isNew = true;
            if (!IsDuplicate(vertex, out var potentialVertices)) return vertex;
            if (!potentialVertices.Any()) return vertex;

            isNew = false;

            TVertex cleanVertex = potentialVertices.OrderByDescending(item =>
                item.Features.CalculateFeatureSimilarity(vertex, item, TokenIndex, StopList)).First();
            cleanVertex.Merge(vertex);
            return cleanVertex;
        }

        private List<TVertex> ResolveVertices(IEnumerable<TVertex> vertices)
        {
            //TODO: improve performance
            var cleanVertices = new List<TVertex>();

            int newVertexCount = 0;

            var references = new Dictionary<ObjectId, List<TVertex>>();

            var unresolvedVertices = vertices as TVertex[] ?? vertices.ToArray();

            Console.WriteLine($"Getting neighbors for {unresolvedVertices.Length} vertices.");

            foreach (var vertex in unresolvedVertices)
            {
                var neighbors = GetNeighbors(vertex);
                foreach (var neighbor in neighbors) references.AddToCollection(neighbor.Id, vertex);
            }

            var duplicates = new List<TVertex>();

            Console.WriteLine($"Resolving {unresolvedVertices.Length} vertices within {references.Count} references.");

            foreach (var vertex in unresolvedVertices)
            {
                Console.WriteLine($"Resolving {vertex.Name}");
                if (!IsDuplicate(vertex, out var potentialVertices) || potentialVertices == null || !potentialVertices.Any())
                {
                    cleanVertices.Add(vertex);
                    newVertexCount++;
                    continue;
                }

                duplicates.Add(vertex);

                TVertex cleanVertex = potentialVertices.OrderBy(item => item.Features.CalculateFeatureSimilarity(vertex, item, TokenIndex, StopList)).First();

                if (vertex.Id != cleanVertex.Id)
                {
                    cleanVertex.Merge(vertex);
                    if (references.TryGetValue(vertex.Id, out var vertexPointers))
                    {
                        foreach (var pointingVertex in vertexPointers)
                        {
                            pointingVertex.UpdatePointers(vertex.Id, cleanVertex.Id);
                        }
                    }
                }

                cleanVertices.Add(cleanVertex);
            }

            Console.WriteLine($"Resolved {cleanVertices.Count:#,#} vertices - {newVertexCount} new, and {duplicates.Count} existing: {duplicates.Select(d => d.Name).Combine(", ")}");
            return cleanVertices;
        }

        public Clique GetClique(ObjectId cliqueId)
        {
            return Cliques.Get(cliqueId);
        }

        public TVertex AddReflectedObject(object item, string name)
        {
            List<TVertex> entities = new List<TVertex>();
            HashSet<object> objectStack = new HashSet<object>() { };

            Dictionary<KeyValuePair<string, string>, float> frequencyByPropertyAndValue = new Dictionary<KeyValuePair<string, string>, float>();

            var entity = AddReflectedObject(item, name, ref entities, ref objectStack, ref frequencyByPropertyAndValue);
            Add(entities);
            UpsertSearchTokens(entity);
            return entity;
        }

        private TVertex AddReflectedObject(object item, string name, ref List<TVertex> vertices, ref HashSet<object> objectStack, ref Dictionary<KeyValuePair<string, string>, float> frequencyByPropertyAndValue)
        {
            if (item is Type) return null;
            if (!objectStack.Add(item)) return null; //recursion

            var type = item.GetType();

            var vertex = new TVertex { Name = name };

            bool propertyAdded = false;

            foreach (var typeProperty in type.GetProperties())
            {
                //var propertyValue = property.GetValue(property);
                propertyAdded |= AddReflectedProperty(typeProperty, item, ref vertex, ref vertices, ref objectStack, ref frequencyByPropertyAndValue);
            }

            if (!propertyAdded) return null;

            vertices.Add(vertex);
            return vertex;
        }

        public bool AddReflectedProperty(PropertyInfo property, object containingObject, ref TVertex vertex, ref List<TVertex> vertices, ref HashSet<object> objectStack, ref Dictionary<KeyValuePair<string, string>, float> frequencyByPropertyAndValue)
        {
            if (property.Name == "SyncRoot") return false;
            if (property.PropertyType == typeof(Type) || property.PropertyType.FullName.StartsWith("System.Reflection")) return false;
            //Ignore XmlIgnore items
            if (property.GetCustomAttributes(false).Any(attribute => attribute is XmlIgnoreAttribute)) return false;

            var type = property.PropertyType;


            if (type == typeof(string))
            {
                var value = (string)property.GetValue(containingObject);
                if (!string.IsNullOrWhiteSpace(value))
                    vertex.Features.TextData.AddIgnoreEmpty(property.Name, value, frequencyByPropertyAndValue.TryReturnValue(new KeyValuePair<string, string>(property.Name, value)) < .1f);
                return true;
            }

            if (type == typeof(bool))
            {
                var value = (bool)property.GetValue(containingObject);
                vertex.Features.BooleanData.AddIgnoreEmpty(property.Name, value);
                return true;
            }

            if (type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(uint) || type == typeof(ulong) || type == typeof(ushort))
            {
                if (!long.TryParse(property.GetValue(containingObject).ToString(), out var longValue)) return false;

                vertex.Features.DiscreteData.Add(property.Name, longValue);
                return true;
            }

            if (type == typeof(float) || type == typeof(double) || type == typeof(decimal))
            {
                if (!double.TryParse(property.GetValue(containingObject).ToString(), out var doubleValue)) return false;

                vertex.Features.ContinuousData.Add(property.Name, doubleValue);
                return true;
            }

            if (type == typeof(DateTime))
            {
                vertex.Features.DateData.AddIgnoreEmpty(property.Name, (DateTime)property.GetValue(containingObject));
                return true;
            }

            if (property.IsNullable())
            {
                if (type == typeof(bool?))
                {
                    var value = (bool?)property.GetValue(containingObject);
                    if (value == null) return false;
                    vertex.Features.BooleanData.AddIgnoreEmpty(property.Name, (bool)value);
                    return true;
                }

                if (type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte) || type == typeof(uint) || type == typeof(ulong) || type == typeof(ushort))
                {
                    if (!long.TryParse(property.GetValue(containingObject)?.ToString() ?? "", out var longValue)) return false;
                    vertex.Features.DiscreteData.Add(property.Name, longValue);
                    return true;
                }

                if (type == typeof(float) || type == typeof(double))
                {
                    var value = (double?)property.GetValue(containingObject);
                    if (value == null) return false;
                    vertex.Features.ContinuousData.Add(property.Name, (double)value);
                    return true;
                }

                if (type == typeof(DateTime))
                {
                    var value = (DateTime?)property.GetValue(containingObject);
                    if (value == null) return false;
                    vertex.Features.DateData.AddIgnoreEmpty(property.Name, (DateTime)value);
                    return true;
                }
            }

            if (type == typeof(IEnumerable))
            {
                var enumerableItems = property.GetValue(containingObject) as IEnumerable;
                if (enumerableItems == null) return false;
                var containerEntity = new TVertex();

                foreach (var enumerableItem in enumerableItems)
                {
                    var enumerableEntity = AddReflectedObject(enumerableItem, property.Name, ref vertices, ref objectStack, ref frequencyByPropertyAndValue);
                    if (enumerableEntity == null) continue;
                    containerEntity.CreateEdge(enumerableEntity);
                }
                //TODO: Record collection based data
            }

            var propertyValue = property.GetValue(containingObject);
            if (propertyValue == null) return false;

            if (propertyValue.GetType().IsClass)
                if (objectStack.Contains(propertyValue)) return false;


            MethodInfo methodInfo = propertyValue.GetType().GetMethod("ToString", Type.EmptyTypes);
            var name = (!methodInfo.IsDefault() && methodInfo.DeclaringType == propertyValue.GetType()) ? propertyValue.ToString() : property.Name;

            var reflectedEntity = AddReflectedObject(propertyValue, name, ref vertices, ref objectStack, ref frequencyByPropertyAndValue);
            if (reflectedEntity == null) return false;

            vertex.CreateEdge(reflectedEntity, property.Name);

            return true;
        }

        public bool Add(IEnumerable<TVertex> vertices, bool createClique = false, bool resolveEntities = true)
        {
            lock (Vertices)
            {
                //var cleanVertices = new List<Entity>();
                //Console.WriteLine("Ensuring tokens are indexed for {0} vertices", vertices.Count.ToString("#,#"));
                //EnsureTokensAreIndexed(vertices);

                if (resolveEntities)
                {
                    var resolvedVertices = ResolveVertices(vertices);
                    vertices = resolvedVertices;
                }

                vertices = vertices.ToArray();

                if (createClique)
                {
                    //Console.WriteLine($"Creating a clique for {vertices.Count()} vertices");
                    var clique = new Clique(vertices);
                    Cliques.Upsert(clique.Id, clique);
                    vertices.ForEach(e => e.CliqueIds.Add(clique.Id));
                    //Console.WriteLine("Done creating a clique");
                }

                //var itemsToUpdate = vertices.ToDictionary(v => v.Id, v => v);
                //Vertices.UpsertRange(itemsToUpdate);

                UpsertSearchTokens(vertices);
                Vertices.Add(vertices);

                //TODO: Ensure Edge vertices are updated

                return true;
            }
        }

        private void EnsureTokensAreIndexed(IEnumerable<TVertex> vertices)
        {
            //Console.WriteLine("Ensuring vertices are indexed");
            var text = new StringBuilder();
            vertices.ForEach(vertex => text.AppendLine(vertex.Features.GetSearchableText()));
            var tokenIds = TokenIndex.GetTokenIds(text.ToString(), StopList);
            Console.WriteLine($"Got Ids for {tokenIds.Length} tokens.");
            //Console.WriteLine("Done ensuring vertices are indexed");
        }

        public bool IsDuplicate(TVertex vertex, float similarityThreshold = .9f)
        {
            var vertices = GetVertices(vertex.Features);

            foreach (var item in vertices)
                if (item.Features.CalculateFeatureSimilarity(vertex, item, TokenIndex, StopList) > similarityThreshold)
                    return true;

            return false;
        }

        private bool IsDuplicate(TVertex vertex, out List<TVertex> potentialVertices, float similarityThreshold = .9f)
        {
            var vertices = GetVertices(vertex.Features);
            potentialVertices = null;

            foreach (var item in vertices)
            {
                //TODO: move similarity limit to UI
                var similarity = item.Features.CalculateFeatureSimilarity(vertex, item, TokenIndex, StopList);
                if (similarity > similarityThreshold)
                {
                    if (potentialVertices == null) potentialVertices = new List<TVertex>();
                    potentialVertices.Add(item);
                }
            }

            return potentialVertices != null && potentialVertices.Any();
        }

        /// <summary>
        /// Gets Vertex Tokens with a count by Id
        /// </summary>
        private Dictionary<ObjectId, int> GetVertexTokens(TVertex vertex)
        {
            return vertex.Features.GetSearchableTokensWithCount(TokenIndex, StopList);
        }

        public GraphSearcher<TVertex> Searcher => searcher ?? (searcher = new GraphSearcher<TVertex>(this));

        public IEnumerable<TVertex> GetVertices(TextSearch textSearch)
        {
            var items = new HashSet<TVertex>();

            if (textSearch.Evaluate(Searcher, out var ids))
            {
                foreach (var id in ids)
                {
                    if (Vertices.TryGetValue(id, out var vertex)) items.Add(vertex);
                }
            }
            return items;
        }


        public List<TVertex> GetVertices(IEnumerable<ObjectId> ids)
        {
            var vertices = new List<TVertex>();
            ids.ForEach(id =>
            {
                if (Vertices.TryGetValue(id, out var vertex)) vertices.Add(vertex);
            });
            return vertices;
        }

        public TVertex GetVertex(ObjectId id)
        {
            Vertices.TryGetValue(id, out var vertex);
            return vertex;
        }

        public TVertex GetVertexById(Features features)
        {
            var id = features.DiscreteData.FirstOrDefault(data => data.Key.Contains("Id"));
            if (Equals(id, default(DiscreteDataPoint))) return default;

            var textSearch = new TextSearch(id.Value.ToString());
            var vertices = GetVertices(textSearch);
            return vertices.FirstOrDefault(v => v.Features.DiscreteData.Any(d => d.Key == id.Key && d.Value == id.Value));
        }

        public IEnumerable<TVertex> GetVertices(Features features)
        {
            List<TVertex> vertices = new List<TVertex>();
            HashSet<ObjectId> vertexIds = new HashSet<ObjectId>();

            var tokens = features.GetResolvableTokens(TokenIndex, StopList).ToArray();
            var meaningfulTokens = tokens.Where(t => t.IsMeaningful).Select(t => t.Id).ToArray();
            if (!meaningfulTokens.Any()) meaningfulTokens = tokens.Select(t => t.Id).ToArray();

            var matches = Searcher.GetMatches(meaningfulTokens);
            //TODO: Soundex

            if (matches != null)
            {
                foreach (var match in matches)
                {
                    if (!vertexIds.Add(match)) continue;

                    if (Vertices.TryGetValue(match, out var vertex))
                    {
                        vertices.Add(vertex);
                    }
                }
            }
            return vertices;
        }

        public bool Update(TVertex vertex)
        {
            Console.WriteLine("Updating vertex: {0}", vertex.Name);

            UpsertSearchTokens(vertex);
            Vertices.Update(vertex);
            return true;
        }


        public void AddClique(Clique clique)
        {
            Cliques.Upsert(clique.Id, clique);

            foreach (var vertexId in clique.VertexIds)
            {
                var vertex = GetVertex(vertexId);
                if (vertex == null) continue; //THIS SHOULD NEVER HAPPEN - WHY IS THIS HAPPENING MONGO?
                vertex.CliqueIds.Add(clique.Id);

                Update(vertex);
            }

        }

        public void CalculateBetweennessCentrality(TVertex value, IEnumerable<TVertex> neighbors, Dictionary<ObjectId[], List<ObjectId>> betweennessMatrix)
        {
            foreach (var neighbor in neighbors)
            {
                var nextNeighbors = GetNeighbors(neighbor);
                CalculateBetweennessCentrality(new[] { value.Id }, neighbor, nextNeighbors, betweennessMatrix);
            }
        }

        public void CalculateBetweennessCentrality(ObjectId[] path, TVertex value, IEnumerable<TVertex> neighbors, Dictionary<ObjectId[], List<ObjectId>> betweennessMatrix)
        {
            foreach (var neighbor in neighbors)
            {
                var nextNeighbors = GetNeighbors(neighbor);

                var neighborPath = new ObjectId[path.Length + 1];
                path.CopyTo(neighborPath, 0);
                neighborPath[path.Length - 1] = value.Id;

                if (!betweennessMatrix.TryGetValue(neighborPath, out var matrixIds))
                    matrixIds = new List<ObjectId>();
                matrixIds.Add(value.Id);
                betweennessMatrix[neighborPath] = matrixIds;

                CalculateBetweennessCentrality(neighborPath, neighbor, nextNeighbors, betweennessMatrix);
            }
        }

        public int GetDegreeCentrality(TVertex vertex)
        {
            return vertex.Edges.Count + vertex.CliqueIds.Select(id => Cliques.Get(id).VertexIds.Count).Sum();
        }


        public ObjectId[] GetOrderedKeys(ObjectId value)
        {
            TVertex vertex = GetVertex(value);
            return TokenIndex.GetTokenIds(vertex.Features.GetSearchableText());
        }
    }
}
