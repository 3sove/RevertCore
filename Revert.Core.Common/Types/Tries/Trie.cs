using System;
using System.Collections.Generic;
using System.Linq;

namespace Revert.Core.Common.Types.Tries
{

    public class Trie<TKey, TValue>
    {
        public IEqualityComparer<TKey> KeyComparer { get; }
        public virtual TrieNode<TKey, TValue> RootNode { get; set; }
        public virtual Action<string> UpdateMessageAction { get; set; } = str => Console.WriteLine(str);
        public virtual int ItemsToEvaluatePerUpdate { get; set; } = 1000;
        public List<TrieNode<TKey, TValue>> Nodes => RootNode.Children.Select(c => c.Value).ToList();


        public Trie()
        {
            RootNode = new TrieNode<TKey, TValue>();
        }

        public Trie(IEqualityComparer<TKey> keyComparer)
        {
            KeyComparer = KeyComparer;
            RootNode = new TrieNode<TKey, TValue>(keyComparer);
        }

        public Trie(Dictionary<TKey[], TValue> values) : this()
        {
            foreach (var item in values)
                Add(item.Key, item.Value);
        }

        public Trie(Dictionary<TKey[], TValue> values, IEqualityComparer<TKey> keyComparer) : this(keyComparer)
        {
            foreach (var item in values)
                Add(item.Key, item.Value);
        }

        public List<KeyAggregate<TKey, TValue>> AggregateKeys()
        {
            List<KeyAggregate<TKey, TValue>> allTrees = new List<KeyAggregate<TKey, TValue>>();

            foreach (var node in Nodes)
            {
                var trees = node.AggregateKeys();
                allTrees.AddRange(trees);
            }
            return allTrees;
        }

        //public void Add(TKey[] keys, TValue value)//, bool includePartialMatches = false)
        //{
        //    RootNode.Add(keys, value, this);

        //    //if (includePartialMatches)
        //    //    for (int i = 1; i < keys.Length; i++)
        //    //        RootNode.Add(keys.Skip(i).ToArray(), value, this);
        //}

        public void Add(TKey[] keys, TValue value)//, bool includePartialMatches = false)
        {
            RootNode.Add(keys, value);

            //if (includePartialMatches)
            //    for (int i = 1; i < keys.Length; i++)
            //        RootNode.Add(keys.Skip(i).ToArray(), value, this);
        }

        public List<TValue> Evaluate(TKey key)
        {
            List<TValue> values;
            RootNode.TryEvaluate(key, out values);
            return values;
        }

        public List<TValue> Evaluate(TKey[] keys)
        {
            List<TValue> values;
            RootNode.TryEvaluate(keys, out values);

            return values;

            //List<TValue> subarrayMatches;
            //for (int i = 1; i < keys.Length; i++)
            //    if (RootNode.TryEvaluate(keys.Skip(i).ToArray(), out subarrayMatches))
            //    {
            //        values.AddRange(subarrayMatches);
            //    }
            //return values;
        }
    }

    public class Trie<TKey>
    {
        public IEqualityComparer<TKey> KeyComparer { get; }
        public virtual TrieNode<TKey> RootNode { get; set; }
        public virtual Action<string> UpdateMessageAction { get; set; } = str => Console.WriteLine(str);
        public virtual int ItemsToEvaluatePerUpdate { get; set; } = 1000;
        public List<TrieNode<TKey>> Nodes => RootNode.Children.Select(c => c.Value).ToList();

        public Trie()
        {
            RootNode = new TrieNode<TKey>();
        }

        public Trie(IEqualityComparer<TKey> keyComparer)
        {
            KeyComparer = keyComparer;
            RootNode = new TrieNode<TKey>(keyComparer);
        }

        public Trie(TKey[] items)
        {
            Add(items);
        }

        public Trie(TKey[] items, IEqualityComparer<TKey> keyComparer) : this(keyComparer)
        {
            Add(items);
        }
        
        public void Add(TKey[] keys)//, bool includePartialMatches = false)
        {
            RootNode.Add(keys);

            //if (includePartialMatches)
            //    for (int i = 1; i < keys.Length; i++)
            //        RootNode.Add(keys.Skip(i).ToArray());
        }

        public void Add(IEnumerable<TKey[]> keyCollection)//, bool includePartialMatches = false)
        {
            foreach (var keys in keyCollection)
                RootNode.Add(keys);

            //if (includePartialMatches)
            //    foreach (var keys in keyCollection)
            //        for (int i = 1; i < keys.Length; i++)
            //            RootNode.Add(keys.Skip(i).ToArray());
        }

        public bool Contains(TKey[] keys)
        {
            return RootNode.Evaluate(keys);
        }
    }
}
