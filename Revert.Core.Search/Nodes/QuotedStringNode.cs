using Revert.Core.Common;
using Revert.Core.Extensions;
using Revert.Core.Search.ErrorReporting;
using Revert.Core.Text.Tokenization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;

namespace Revert.Core.Search.Nodes
{
    public class QuotedStringNode : Node
    {
        public ITokenizer Tokenizer { get; set; }
        public static readonly QuotedStringNode Parser = new QuotedStringNode();
        public string Value;
        public int WildCardNumber;

        private QuotedStringNode()
        {
        }

        public QuotedStringNode(List<LexicalTokenizer> tokens, LexicalTokenizer tokenData, ITokenizer tokenizer)
            : base("\"", tokens)
        {
            Tokenizer = tokenizer;
            Value = tokenData.Token;
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
            if (tokens.Count > 0 && (tokens[0] is Alpha || tokens[0] is Numeric || tokens[0] is AlphaNumeric) || tokens[0] is QuotedString)
            {
                List<LexicalTokenizer> remainingTokens = tokens.Skip(1).ToList();
                var TokenNode = new TokenNode(tokens, tokens[0]);

                //Push the node onto the Current stack
                errorTrack.Push(TokenNode);

                return TokenNode;
            }
            return null;
        }

        public override bool Eval(string textToSearch)
        {
            return textToSearch.Contains(Value, StringComparison.CurrentCultureIgnoreCase);
        }

        public override bool Evaluate<TValue>(ISearchable<ObjectId, TValue> searchable, out IEnumerable<TValue> results)
        {
            //TODO: implement wildcards inside of quoted strings
            results = searchable.GetLiteralMatches(searchable.GetKeysFromString(Value));
            return results?.Any() ?? false;
        }

        public override string GetDisplayText()
        {
            return Value.All(char.IsDigit) ? $"<span class='numericToken'>{Value}</span>" : $"<span class='alphaToken'>{Value}</span>";
        }

        public override List<MatchedCoordinate> GetMatchedCoordinates(string TextData, bool isSoundex, List<long> valueIDs = null)
        {
            return TextData.FindMatchedLocations(GetNonWildToken(), StringComparison.OrdinalIgnoreCase);
        }

        //public override void GetSearchableTokens(ref List<string> tokens)
        //{
        //    tokens = Tokenizer.GetTokens(KeyTwo);
        //}

        public override string ToString()
        {
            return $"Quoted String: '{Value}'";
        }
    }
}
