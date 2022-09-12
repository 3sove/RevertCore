using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Revert.Core.Search.Functions;
using Revert.Core.Search.ErrorReporting;
using System.Collections;
using MongoDB.Bson;
using Revert.Core.Extensions;
using Revert.Core.Text.Tokenization;
using Revert.Core.Common;

namespace Revert.Core.Search.Nodes
{
    internal class TokenNode : WizardNode
	{
        public static readonly TokenNode Parser = new TokenNode();
        public string Value;
        public Type TokenType;
        public int WildCardNumber;

        private TokenNode()
        {
        }

        public TokenNode(List<LexicalTokenizer> tokens, LexicalTokenizer tokenData)
            : base(tokenData.Token, tokens)
        {
            Value = tokenData.Token;
            TokenType = tokenData.GetType();
            var data = tokenData as WildCardToken;
            WildCardNumber = data?.WildCard ?? 0;
        }

        public string GetNonWildToken()
        {
            if (WildCardNumber == 0) return Value;

            string[] possible = Value.Split('*');

            string longestString = string.Empty;

            foreach (string item in possible)
                if (item.Length > longestString.Length) longestString = item;

            return longestString;
        }

        public override Node TryParse(List<LexicalTokenizer> tokens, ErrorTrack errorTrack)
        {
            //If the list is NOT NULL AND then first token in the list is a Term (Alpha, Numeric, AlphaNumeric, or Quoted String) then 
            //return a new instance of a Term node containing the token and the list; otherwise, return null

            if (tokens.Count == 0) return null;
            var token = tokens[0];
            var remainingTokens = tokens.Skip(1).ToList();

            Node node = null;

            if (token is QuotedString)
                node = new QuotedStringNode(remainingTokens, token, new SimpleTokenizer());

            if (token is Alpha || token is Numeric || token is AlphaNumeric)
                node = new TokenNode(remainingTokens, token);

            if (node != null) errorTrack.Push(node);
            return node;
        }

        public override bool Eval(string textToSearch)
        {
            return textToSearch.Contains(Value, StringComparison.CurrentCultureIgnoreCase);
        }

        public override bool Evaluate<TValue>(ISearchable<ObjectId, TValue> searchable, out IEnumerable<TValue> results)
        {
            if (WildCardNumber == 0)
            {
                results = searchable.GetMatches(searchable.GetKeyFromString(Value));
                return results?.Any() ?? false;
            }

            int wildCardIndex = Value.IndexOf("*", StringComparison.Ordinal);

            results = new List<TValue>();

            var value = Value;

            while (wildCardIndex != -1)
            {
                if (value == string.Empty) break;
                var wildCardKeys = new List<ObjectId>();

                var stopLocation = wildCardIndex >= value.Length - 1 ? value.Length : value.IndexOf("*", wildCardIndex + 1, StringComparison.Ordinal);
                if (stopLocation == -1) stopLocation = value.Length;

                //if the search is nothing but wildcards
                if (Value.Replace(Wildcard.ELEMENT, ' ').Trim() == string.Empty)
                {
                    results = searchable.AllValues;
                    return results.Any();
                }

                if (wildCardIndex == 0)
                {
                    var leadingWildCardKeys = searchable.GetKeysFromLeadingWildCard(value.Substring(1, stopLocation - 1));
                    wildCardKeys.AddRange(leadingWildCardKeys);
                }
                else
                {
                    var trailingWildCardKeys = searchable.GetKeysFromTrailingWildCard(value.Substring(0, wildCardIndex));
                    wildCardKeys.AddRange(trailingWildCardKeys);
                }

                TokenIndex tokenIndex;
                if (!TokenIndex.TryGetInstance(out tokenIndex) || tokenIndex == null) return false;


                List<string> tokens = tokenIndex.GetTokens(wildCardKeys.ToArray()).Select(t => t.Value).ToList();

                if (wildCardTokens == null) wildCardTokens = new HashSet<string>();
                tokens.ForEach(t => wildCardTokens.Add(t));

                var matches = searchable.GetMatches(wildCardKeys.ToArray(), false);
                if (matches != null) results = results.Union(matches);
                wildCardIndex = value.IndexOf("*", wildCardIndex + 1, StringComparison.Ordinal);

                if (stopLocation != value.Length) value = value.Substring(stopLocation + 1);
            }

            return results.Any();
        }

        HashSet<string> wildCardTokens;

        public override string GetDisplayText()
        {
            bool isNumeric = Value.All(char.IsDigit);
            if (isNumeric) return $"<span class='numericToken'>{Value}</span>";

            if (WildCardNumber != 0 && wildCardTokens == null) return $"{Value} - No Wildcard Matches";

            if (WildCardNumber == 0) return $"<span class='alphaToken'>{Value}</span>";
            return $"<span class='alphaToken'>{Value}</span> <br /> Wildcard Matches(<span class='wildcardToken'>{wildCardTokens.Combine(", ")}</span>)<br />";
        }

        public override List<MatchedCoordinate> GetMatchedCoordinates(string textData, bool isSoundex, List<long> valueIDs = null)
        {
            //soundex is ignored for now
            if (WildCardNumber == 0) return textData.FindMatchedLocations(GetNonWildToken(), StringComparison.OrdinalIgnoreCase);

            var results = new List<MatchedCoordinate>();

            if (wildCardTokens != null)
            {
                if (wildCardTokens.Count > 100)
                {
                    var token = GetNonWildToken();
                    return textData.FindMatchedLocations(token, StringComparison.OrdinalIgnoreCase, Value.StartsWith("*"));
                }

                foreach (var token in wildCardTokens)
                    if (token.Length > 2)
                        results.AddRange(textData.FindMatchedLocations(token, StringComparison.OrdinalIgnoreCase));
            }
            return results;
        }

        public void GetSearchableTokens(ref List<string> tokens)
        {
            if (WildCardNumber == 0) tokens.Add(GetNonWildToken());
            else if (wildCardTokens != null) tokens.AddRange(wildCardTokens);
        }

        public override string ToString()
        {
            return $"Token: {Value}";
        }
    }
}
