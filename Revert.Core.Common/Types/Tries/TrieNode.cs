using Revert.Core.Extensions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Revert.Core.Common.Types.Tries
{
    [DebuggerDisplay("{DebugString}")]
    public class TrieNode<TKey, TValue> : TrieNode
    {
        private TKey key;
        internal HashSet<TValue> Values { get; } = new HashSet<TValue>();
        public Dictionary<TKey, TrieNode<TKey, TValue>> Children { get; set; }
        public IEqualityComparer<TKey> KeyComparer { get; }

        public TrieNode()
        {
            Children = new Dictionary<TKey, TrieNode<TKey, TValue>>();
        }

        public TrieNode(IEqualityComparer<TKey> keyComparer)
        {
            Children = new Dictionary<TKey, TrieNode<TKey, TValue>>(keyComparer);
            KeyComparer = keyComparer;
        }

        public TrieNode(TKey key) : this()
        {
            this.key = key;
        }

        public TrieNode(TKey key, IEqualityComparer<TKey> keyComparer) : this(keyComparer)
        {
            this.key = key;
        }

        public void Add(TrieNode<TKey, TValue> node)
        {
            TrieNode<TKey, TValue> childNode;
            if (!Children.TryGetValue(node.key, out childNode))
            {
                childNode = this;
                childNode.Children.Add(node.key, node);
            }
            foreach (var item in node.Values)
                childNode.Values.Add(item);
        }

        public List<KeyAggregate<TKey, TValue>> AggregateKeys()
        {
            return AggregateKeys(new KeyAggregate<TKey, TValue>());
        }

        public List<KeyAggregate<TKey, TValue>> AggregateKeys(KeyAggregate<TKey, TValue> keys)
        {
            keys.Add(new KeyTripplet<TKey, List<TValue>, int>(key, Values.ToList(), Count));
            var allTrees = new List<KeyAggregate<TKey, TValue>>();
            if (Children.Count == 0 || keys.Count >= 3)
                allTrees.Add(keys);

            foreach (var child in Children)
            {
                List<KeyAggregate<TKey, TValue>> childTrees = child.Value.AggregateKeys(new KeyAggregate<TKey, TValue>(keys));
                allTrees.AddRange(childTrees);
            }
            return allTrees;
        }

        //public void Add(TKey[] keyArray, TValue value, Trie<TKey, TValue> trie)
        //{
        //    Count++;

        //    TrieNode<TKey, TValue> currentNode = this;
        //    foreach (var key in keyArray)
        //    {
        //        TrieNode<TKey, TValue> nextNode;
        //        if (!currentNode.Children.TryGetValue(key, out nextNode))
        //            currentNode.Children[key] = nextNode = new TrieNode<TKey, TValue>(key);

        //        currentNode = nextNode;
        //    }
        //    currentNode.Values.Add(value);
        //}

        //public void Add(IEnumerable<TKey> keyArray, TValue value, Trie<TKey, TValue> trie)
        //{
        //    Count++;

        //    TrieNode<TKey, TValue> currentNode = this;
        //    foreach (var key in keyArray)
        //    {
        //        TrieNode<TKey, TValue> nextNode;
        //        if (!currentNode.Children.TryGetValue(key, out nextNode))
        //        {
        //            if (KeyComparer == null)
        //                currentNode.Children[key] = nextNode = new TrieNode<TKey, TValue>(key);
        //            else
        //                currentNode.Children[key] = nextNode = new TrieNode<TKey, TValue>(key, KeyComparer);
        //        }


        //        currentNode = nextNode;
        //    }
        //    currentNode.Values.Add(value);
        //}

        public void Add(TKey[] keyArray, TValue value)
        {
            Add(keyArray, value, 0);
        }

        public void Add(TKey[] keyArray, TValue value, int position)
        {
            Count++;
            if (position >= keyArray.Length)
            {
                return;
            }

            var nextKey = keyArray[position];
            TrieNode<TKey, TValue> nextNode;
            if (!Children.TryGetValue(nextKey, out nextNode))
            {
                if (KeyComparer == null)
                    Children[nextKey] = nextNode = new TrieNode<TKey, TValue>(nextKey);
                else
                    Children[nextKey] = nextNode = new TrieNode<TKey, TValue>(nextKey, KeyComparer);
            }

            nextNode.Values.Add(value);
            nextNode.Add(keyArray, value, position + 1);
        }

        public string DebugString
        {
            get
            {
                string keyString = string.Empty;
                var keyArray = key as int[];
                if (keyArray == null) keyString = key.ToString();
                else
                {
                    for (int i = 0; i < keyArray.Length; i++)
                    {
                        if (keyString != string.Empty) keyString += ", ";
                        keyString += keyArray[i].ToString();
                    }
                }

                return $"TrieNode<{typeof(TKey).Name}, {typeof(TValue).Name}> - {keyString} | {Values.DistinctWithCount().Select(item => $"{item.Key} ({item.Value})").Combine(", ")} | {Children.Count} children | Count: {Count}";
            }
        }

        public bool TryEvaluate(TKey[] keyArray, out List<TValue> values)
        {
            return TryEvaluate(keyArray, 0, out values);
        }

        public bool TryEvaluate(TKey[] keyArray, int position, out List<TValue> values)
        {
            if (keyArray.Length <= position)
            {
                values = GetValues();
                return true;
            }

            var currentKey = keyArray[position];
            TrieNode<TKey, TValue> nextTreeNode;

            if (!Children.TryGetValue(currentKey, out nextTreeNode) || nextTreeNode == null)
            {
                values = new List<TValue>(); // Equals(this.values, default(TValue)) ? new List<TValue>() : this.values;
                return false;
            }
            return nextTreeNode.TryEvaluate(keyArray, ++position, out values);
        }

        public bool TryEvaluate(TKey key, out List<TValue> values)
        {
            TrieNode<TKey, TValue> nextTreeNode;
            if (!Children.TryGetValue(key, out nextTreeNode) || nextTreeNode == null)
            {
                values = new List<TValue>(); // Equals(this.values, default(TValue)) ? new List<TValue>() : this.values;
                return false;
            }
            values = nextTreeNode.GetValues();
            return true;
        }

        public List<TValue> GetValues()
        {
            List<TValue> values = new List<TValue>();
            GetValues(ref values);
            return values;
        }

        private void GetValues(ref List<TValue> values)
        {
            foreach (var child in Children) child.Value.GetValues(ref values);
            if (Values.Count != 0) values.AddRange(Values);
        }
    }

    [DebuggerDisplay("{DebugString}")]
    public class TrieNode<TKey> : TrieNode
    {
        private TKey key;
        public IEqualityComparer<TKey> KeyComparer { get; }
        public Dictionary<TKey, TrieNode<TKey>> Children { get; set; }

        public string DebugString
        {
            get
            {
                string keyString = string.Empty;
                var keyArray = key as int[];
                if (keyArray == null) keyString = key.ToString();
                else
                {
                    for (int i = 0; i < keyArray.Length; i++)
                    {
                        if (keyString != string.Empty) keyString += ", ";
                        keyString += keyArray[i].ToString();
                    }
                }

                return $"TrieNode<{typeof(TKey).Name}> - {keyString} | {Children.Count} children | Count: {Count}";
            }
        }

        public TrieNode()
        {
            Children = new Dictionary<TKey, TrieNode<TKey>>();
        }

        public TrieNode(IEqualityComparer<TKey> keyComparer)
        {
            KeyComparer = keyComparer;
            Children = new Dictionary<TKey, TrieNode<TKey>>(KeyComparer);
        }

        public TrieNode(TKey key) : this()
        {
            this.key = key;
        }

        public TrieNode(TKey key, IEqualityComparer<TKey> keyComparer) : this(keyComparer)
        {
            this.key = key;
        }

        public void Add(TKey[] keyArray)
        {
            Add(keyArray, 0);
        }

        protected void Add(TKey[] keyArray, int position)
        {
            Count++;

            if (position >= keyArray.Length)
            {
                return;
            }

            var nextKey = keyArray[position];
            TrieNode<TKey> nextNode;
            if (!Children.TryGetValue(nextKey, out nextNode))
            {
                if (KeyComparer == null)
                    Children[nextKey] = nextNode = new TrieNode<TKey>(nextKey);
                else
                    Children[nextKey] = nextNode = new TrieNode<TKey>(nextKey, KeyComparer);
            }


            nextNode.Add(keyArray, position + 1);
        }

        public bool Evaluate(TKey[] keyArray)
        {
            return TryEvaluate(keyArray, 0);
        }

        protected bool TryEvaluate(TKey[] keyArray, int position)
        {
            if (keyArray.Length <= position) return true;

            var currentKey = keyArray[position];
            TrieNode<TKey> nextTreeNode;

            if (!Children.TryGetValue(currentKey, out nextTreeNode) || nextTreeNode == null) return false;
            return nextTreeNode.TryEvaluate(keyArray, ++position);
        }
    }

    public class TrieNode
    {
        public int Count { get; protected set; }
    }
}