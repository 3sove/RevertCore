using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Revert.Core.Common;
using Revert.Core.Common.Types;
using Revert.Core.Common.Types.Tries;
using Revert.Core.Extensions;
using Revert.Core.Indexing;
using Revert.Core.IO;
using Revert.Core.IO.Serialization;
using Revert.Core.Text.NLP;
using Revert.Core.IO.Stores;

namespace Revert.Core.Text.Tokenization
{
    public class TokenIndex : IDisposable
    {
        private readonly string connectionString;
        private readonly string databaseName;
        private readonly string collectionName;
        private readonly string nlpDataDirectoryPath;
        public string FileName { get; set; }

        private MongoRecordIndex<Token> TokenByIdIndex { get; set; }
        //private BPlusTree<uint, Token> TokenByIdIndex { get; set; }
        private Dictionary<string, ObjectId> TokenIdByStringIndex { get; set; }

        private Trie<char, ObjectId> forwardTokenTrie = new Trie<char, ObjectId>();
        private Trie<char, ObjectId> backwardTokenTrie = new Trie<char, ObjectId>();

        private Dictionary<ObjectId, Token> TokenByIdCache { get; set; }

        public bool CreateIfNotFound { get; set; } = true;

        public TokenIndex()
        {
        }

        private TokenIndex(string connectionString, string databaseName, string collectionName, string nlpDataDirectoryPath)
        {
            this.connectionString = connectionString;
            this.databaseName = databaseName;
            this.collectionName = collectionName;
            this.nlpDataDirectoryPath = nlpDataDirectoryPath;

            TokenByIdIndex = new MongoRecordIndex<Token>(connectionString, databaseName, collectionName);

            if (TokenByIdIndex == null || TokenByIdIndex.GetCount() == 0)
            {
                TokenByIdIndex = GenerateFromWordNet(connectionString, nlpDataDirectoryPath, databaseName, collectionName);
            }

            var items = TokenByIdIndex.GetItems().ToArray();
            TokenByIdCache = new Dictionary<ObjectId, Token>();
            foreach (var item in items)
            {
                TokenByIdCache[item.KeyOne] = item.KeyTwo;
            }

            TokenIdByStringIndex = new Dictionary<string, ObjectId>(StringComparer.OrdinalIgnoreCase);
            Console.WriteLine("Caching token index");

            var tokensById = TokenByIdIndex.GetCount() > 0
                ? TokenByIdIndex.GetItems().ToDictionary(item => item.KeyOne, item => item.KeyTwo)
                : new Dictionary<ObjectId, Token>();

            foreach (var tokenById in tokensById)
            {
                var tokenValue = tokenById.Value.Value;
                if (TokenIdByStringIndex.TryGetValue(tokenValue, out var duplicateTokenId))
                {
                    Console.WriteLine($"{tokenValue} is a duplicate with {duplicateTokenId}");
                    continue;
                }
                TokenIdByStringIndex[tokenValue] = tokenById.Key;

                //var lowerTokenValue = tokenById.KeyTwo.KeyTwo.ToLowerInvariant().ToCharArray();
                //forwardTokenTrie.Add(lowerTokenValue, tokenById.KeyOne);
                //backwardTokenTrie.Add(lowerTokenValue.Reverse().ToArray(), tokenById.KeyOne);
            }
            Console.WriteLine("Done caching token index - found {0:#,#} tokens", TokenIdByStringIndex.Count);
        }

        private static readonly Dictionary<string, TokenIndex> tokenIndices = new Dictionary<string, TokenIndex>(StringComparer.OrdinalIgnoreCase);
        public static TokenIndex GetInstance(string connectionString, string databaseName, string collectionName, string nlpDictionaryPath)
        {
            if (!tokenIndices.TryGetValue(databaseName + collectionName, out var index))
                tokenIndices[databaseName + collectionName] = index = new TokenIndex(connectionString, databaseName, collectionName, nlpDictionaryPath);
            return index;
        }

        public static bool TryGetInstance(out TokenIndex tokenIndex)
        {
            if (!tokenIndices.Any())
            {
                tokenIndex = null;
                return false;
            }
            tokenIndex = tokenIndices.First().Value;
            return true;
        }

        public static void DisposeInstance(string path)
        {
            if (tokenIndices.ContainsKey(path)) tokenIndices.Remove(path);
        }

        private ITokenizer tokenizer;
        public ITokenizer Tokenizer
        {
            get => tokenizer ?? (tokenizer = new SimpleTokenizer());
            set => tokenizer = value;
        }

        public Dictionary<ObjectId, int> GetTokenIdsWithCount(string value, bool createNewTokens = false, HashSet<string> stopList = null)
        {
            var tokenStrings = Tokenizer.GetTokens(value);
            var countById = new Dictionary<ObjectId, int>();
            var newTokens = new Dictionary<string, Token>(StringComparer.OrdinalIgnoreCase);

            int skippedTokenCount = 0;

            foreach (var tokenString in tokenStrings)
            {
                if (stopList != null && stopList.Contains(tokenString, StringComparer.OrdinalIgnoreCase))
                {
                    skippedTokenCount++;
                    continue;
                }

                if (TokenIdByStringIndex.TryGetValue(tokenString, out var id))
                {
                    countById.IncrementValue(id);
                }
                else
                {
                    if (newTokens.TryGetValue(tokenString, out var newToken))
                    {
                        countById.IncrementValue(newToken.Id);
                    }
                    else if (createNewTokens)
                    {
                        var token = new Token { Id = ObjectId.GenerateNewId(), Value = tokenString };
                        newTokens[token.Value] = token;
                    }
                }
            }

            if (createNewTokens && newTokens.Any())
            {
                foreach (var item in newTokens)
                {
                    countById.IncrementValue(item.Value.Id);
                }

                BulkInsert(newTokens.Values);
            }

            return countById;
        }

        public List<Token> GetTokens(ObjectId[] tokenIds)
        {
            Token[] tokens;
            TokenByIdIndex.TryGetValues(tokenIds, out tokens);
            return tokens.ToList();
        }

        readonly Dictionary<int, Token> emptyTokensByLocation = new Dictionary<int, Token>();
        public Dictionary<int, Token> GetTokensByLocation(string value, bool createNewTokens = false, HashSet<string> stopList = null)
        {
            if (string.IsNullOrWhiteSpace(value)) return emptyTokensByLocation;

            var tokenStrings = Tokenizer.GetTokens(value).Distinct(StringComparer.OrdinalIgnoreCase);
            var values = new Dictionary<int, Token>();

            var newTokens = new List<Token>();

            int i = 0;
            foreach (var tokenString in tokenStrings)
            {
                i++;
                if (stopList != null && stopList.Contains(tokenString)) continue;

                Token token = null;
                if (TokenIdByStringIndex.TryGetValue(tokenString, out var id))
                    token = TokenByIdIndex.Get(id);
                else if (createNewTokens)
                    newTokens.Add(token = new Token { Value = tokenString });
                values[i] = token;
            }

            if (createNewTokens && newTokens.Any()) BulkInsert(newTokens);
            return values;
        }

        readonly ObjectId[] emptyTokenArray = Array.Empty<ObjectId>();
        public ObjectId[] GetTokenIds(string value, HashSet<string> stopList = null)
        {
            if (string.IsNullOrWhiteSpace(value)) return emptyTokenArray;
            var tokenStrings = Tokenizer.GetTokens(value).Distinct(StringComparer.OrdinalIgnoreCase);
            var tokenIds = new List<ObjectId>();
            var newTokens = new List<Token>();

            foreach (var tokenString in tokenStrings)
            {
                if (string.IsNullOrWhiteSpace(tokenString) || stopList != null && stopList.Contains(tokenString)) continue;

                if (TokenIdByStringIndex.TryGetValue(tokenString, out var id)) tokenIds.Add(id);
                else newTokens.Add(new Token { Value = tokenString });
            }

            if (newTokens.Any())
            {
                BulkInsert(newTokens);
                tokenIds.AddRange(newTokens.Select(t => t.Id));
            }

            return tokenIds.ToArray();
        }

        public ObjectId GetTokenId(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return ObjectId.Empty;

            foreach (var tokenString in Tokenizer.GetTokens(value))
            {
                TokenIdByStringIndex.TryGetValue(tokenString, out var id);
                return id;
            }
            return ObjectId.Empty;
        }

        private readonly object bulkInsertLock = new object();
        private void BulkInsert(IEnumerable<Token> tokens)
        {
            lock (TokenIdByStringIndex)
            {
                var tokenArray = tokens as Token[] ?? tokens.ToArray();

                foreach (var token in tokenArray.Where(token => token.Id == ObjectId.Empty))
                    token.Id = ObjectId.GenerateNewId();

                var tokensById = tokenArray.ToKeyPairs(t => new KeyPair<ObjectId, Token>(t.Id, t));

                foreach (var token in tokenArray)
                    TokenIdByStringIndex[token.Value] = token.Id;

                TokenByIdIndex.UpsertRange(tokensById);
            }
        }

        public void DeleteRange(int from, int to)
        {
            throw new NotImplementedException();
        }

        private void Insert(Token token)
        {
            //lock (TokenByIdIndex)
            //{
            if (token.Id == ObjectId.Empty)
                token.Id = ObjectId.GenerateNewId();

            TokenByIdIndex.Upsert(token.Id, token);
            TokenIdByStringIndex[token.Value] = token.Id;
            //}
        }

        public List<Token> GetTokens()
        {
            List<Token> tokens = new List<Token>();
            foreach (var token in TokenByIdIndex.GetValues())
                tokens.Add(token);
            return tokens;
        }

        public IEnumerable<Token> GetTokens(string value, HashSet<string> stopList = null)
        {
            var tokenIds = GetTokenIds(value, stopList);

            List<Token> tokens = new List<Token>();
            foreach (var tokenId in tokenIds)
            {
                if (!TokenByIdCache.TryGetValue(tokenId, out var token))
                    token = TokenByIdCache[tokenId] = TokenByIdIndex.Get(tokenId);

                tokens.Add(token);
            }
            return tokens;
        }

        public bool TryGetToken(ObjectId id, out Token token)
        {
            return TokenByIdIndex.TryGetValue(id, out token);
        }

        public bool TryGetToken(string value, out Token token)
        {
            return TokenByIdIndex.TryGetValue(GetTokenId(value), out token);
        }

        public static MongoRecordIndex<Token> GenerateFromWordNet(string connectionString, string nlpDataDirectoryPath, string databaseName, string collectionName)
        {
            var englishDictionary = new EnglishDictionary(new EnglishDictionaryModel());

            Console.WriteLine("Completed populating english dictionary - proceeding to serialization.");

            //For some reason Wordnet has duplicates in it... /sigh
            var words = new Dictionary<string, Word>();
            foreach (var word in englishDictionary.Words)
            {
                if (words.TryGetValue(word.Value.ToLower(), out var indexedWord))
                {
                    indexedWord.Merge(word);
                    continue;
                }

                words[word.Value.ToLower()] = word;
            }

            //var directoryInfo = new DirectoryInfo(outputDirectory);
            //if (!directoryInfo.Exists) directoryInfo.Create();

            Console.WriteLine("Serializing Token by Id index.");

            var tokenIndex = new MongoRecordIndex<Token>(connectionString, databaseName, collectionName);

            var tokensToInsert = new List<MongoKeyPair<ObjectId, Token>>();
            foreach (var word in words)
            {
                var token = new Token
                {
                    Id = ObjectId.GenerateNewId(),
                    //Definition = w.KeyTwo.Definition,
                    PartOfSpeech = word.Value.PartOfSpeech,
                    //RelatedWords = w.KeyTwo.RelatedWords,
                    //Synonyms = w.KeyTwo.Synonyms,
                    Value = word.Value.Value
                };
                tokensToInsert.Add(new MongoKeyPair<ObjectId, Token>(token.Id, token));
            }

            tokenIndex.UpsertRange(tokensToInsert);
            return tokenIndex;
        }

        public void Dispose()
        {
        }
    }
}


