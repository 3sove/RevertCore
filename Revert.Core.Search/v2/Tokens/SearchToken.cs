using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Revert.Core.Search.v2.Tokens
{
    [DebuggerDisplay("{GetType().Name} - {Value}")]
    public abstract class SearchToken<TToken> : SearchToken where TToken : SearchToken<TToken>, new()
    {
        private static TToken instance;

        private List<Type> childTokenTypes;

        public static TToken Instance
        {
            get { return instance ?? (instance = new TToken()); }
            set { instance = value; }
        }

        public override List<Type> ChildTokenTypes
        {
            get
            {
                return childTokenTypes ?? (childTokenTypes = new List<Type>
                {
                    typeof (WordToken),
                    typeof (NotToken),
                    typeof (OrToken),
                    typeof (AndToken),
                    typeof (QuoteToken),
                    typeof (LeftParenthesisToken),
                    typeof (RightParenthesisToken)
                });
            }
        }

        protected virtual IEnumerable<string> MatchStrings { get; } = new[] { string.Empty };

        public override bool IsMatch(string value)
        {
            return !string.IsNullOrEmpty(value) && MatchStrings.Any(item => string.Equals(value, item, StringComparison.InvariantCultureIgnoreCase));
        }

        public override SearchToken Create(string token, HashSet<string> stopList)
        {
            return new TToken {Value = token, StopList = stopList};
        }

        public override IEnumerable<TValue> Evaluate<TKey, TValue>(ISearchable<TKey, TValue> searchable)
        {
            return null;
        }
    }


    public abstract class SearchToken
    {
        private List<SearchToken> children;

        public List<SearchToken> Children
        {
            get { return children ?? (children = new List<SearchToken>()); }
            set { children = value; }
        }

        public string Value { get; set; }

        public HashSet<string> StopList { get; set; }

        public abstract List<Type> ChildTokenTypes { get; }

        public abstract bool IsMatch(string value);

        public abstract SearchToken Create(string token, HashSet<string> stopList);

        public bool Push(SearchToken token)
        {
            if (ChildTokenTypes.Contains(token.GetType()))
            {
                Children.Add(token);
                return true;
            }
            return false;
        }

        public abstract IEnumerable<TValue> Evaluate<TKey, TValue>(ISearchable<TKey, TValue> searchable);
    }
}