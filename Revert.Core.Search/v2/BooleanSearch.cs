using System;
using System.Collections.Generic;
using System.Linq;
using Revert.Core.Search.v2.Tokens;
using Revert.Core.Text.Tokenization;

namespace Revert.Core.Search
{
    public class BooleanSearch<TKey, TValue>
    {
        public string SearchString { get; set; }
        public ISearchable<TKey, TValue> Searchable { get; set; }
        public HashSet<string> StopList { get; set; }

        private static List<SearchToken> searchTokens;
        public static List<SearchToken> SearchTokens
        {
            get
            {
                return searchTokens ?? (searchTokens = new List<SearchToken>
                {
                    QuoteToken.Instance,
                    LeftParenthesisToken.Instance,
                    RightParenthesisToken.Instance,
                    SpaceToken.Instance,
                    AndToken.Instance,
                    OrToken.Instance,
                    WordToken.Instance
                });
            }
            set { searchTokens = value; }
        }

        public BooleanSearch(string searchString, ISearchable<TKey, TValue> searchable, HashSet<string> stopList = null)
        {
            SearchString = searchString;
            Searchable = searchable;
            StopList = stopList ?? new HashSet<string>();
            SearchTokens.ForEach(token => token.StopList = StopList);
        }



        public IEnumerable<TValue> Execute()
        {
            //TODO: Unit tests
            if (SearchString.Trim() == "*")
                return Searchable.AllValues;

            var tokenizer = new SimpleTokenizer();
            List<string> rawTokens = tokenizer.GetTokens(SearchString);

            var cleanTokens = GetSearchTokens(rawTokens, StopList);
            var tokenStack = new Stack<SearchToken>();

            foreach (var token in cleanTokens)
            {
                if (tokenStack.Count == 0)
                {
                    tokenStack.Push(token);
                    continue;
                }

                var previousToken = tokenStack.Peek();
                if (previousToken == null) throw new Exception("Expected a token on the top of the stack, instead found nothing.");

                if (EvaluateQuotations(token, previousToken, tokenStack)) continue;
                if (EvaluateParenthesis(token, tokenStack, previousToken)) continue;

                if (previousToken.ChildTokenTypes != null && previousToken.ChildTokenTypes.Contains(token.GetType()))
                {
                    previousToken.Push(token);
                    continue;
                }

                if (token.ChildTokenTypes != null && token.ChildTokenTypes.Contains(previousToken.GetType()))
                {
                    token.Push(tokenStack.Pop());
                    tokenStack.Push(token);
                    continue;
                }

                var andToken = new AndToken();
                andToken.Push(tokenStack.Pop());
                andToken.Push(token);
                tokenStack.Push(andToken);
            }

            if (tokenStack.Count == 0 || tokenStack.Count > 1) throw new Exception("0 items in token stack, must be an error");

            return tokenStack.Peek().Evaluate(Searchable) ?? new List<TValue>();
        }

        private static bool EvaluateParenthesis(SearchToken token, Stack<SearchToken> tokenStack, SearchToken previousToken)
        {
            var leftParenthesis = token as LeftParenthesisToken;
            if (leftParenthesis != null)
            {
                tokenStack.Push(leftParenthesis);
                return true;
            }

            if (!(token is RightParenthesisToken)) return false;
            if (!(previousToken is LeftParenthesisToken)) return true;  //This is for poorly formed parethesis

            tokenStack.Pop();
            var nextLowestToken = tokenStack.Peek();
            if (nextLowestToken == null)
                tokenStack.Push(previousToken);
            else if (nextLowestToken.ChildTokenTypes != null && nextLowestToken.ChildTokenTypes.Contains(typeof(LeftParenthesisToken)))
                nextLowestToken.Push(previousToken);
            else
            {
                var andToken = new AndToken();
                andToken.Push(tokenStack.Pop());
                andToken.Push(previousToken);
                tokenStack.Push(andToken);
            }
            return true;
        }

        private static bool EvaluateQuotations(SearchToken token, SearchToken previousToken, Stack<SearchToken> tokenStack)
        {
            var quotationToken = token as QuoteToken;
            if (quotationToken == null) return false;

            if (!(previousToken is QuoteToken))
            {
                tokenStack.Push(quotationToken);
                return true;
            }

            tokenStack.Pop();
            var nextLowestToken = tokenStack.Peek();
            if (nextLowestToken == null)
                tokenStack.Push(previousToken);
            else if (nextLowestToken.ChildTokenTypes.Contains(typeof(QuoteToken)))
                nextLowestToken.Push(previousToken);
            else
            {
                var andToken = new AndToken();
                andToken.Push(tokenStack.Pop());
                tokenStack.Push(andToken);
            }

            return true;
        }

        public static List<SearchToken> GetSearchTokens(List<string> tokenStrings, HashSet<string> stopList = null)
        {
            return tokenStrings.Select(tokenString =>
            {
                SearchToken token = null;
                foreach (var searchToken in SearchTokens)
                {
                    if (searchToken.IsMatch(tokenString))
                    {
                        token = searchToken.Create(tokenString, stopList);
                        break;
                    }
                }
                return token;
            }).Where(t => !Equals(t, null)).ToList();
        }

    }
}
