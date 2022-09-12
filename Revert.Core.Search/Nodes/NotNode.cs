using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Revert.Core.Search.ErrorReporting;
using System.Threading;
using System.Collections;
using MongoDB.Bson;
using Revert.Core.Extensions;
using Revert.Core.Common;

namespace Revert.Core.Search.Nodes
{
    internal class NotNode : WizardNode
	{
		public Node SubNode;

		public static readonly NotNode Parser = new NotNode();

		private NotNode() : base() { }

		public NotNode(string nodeOperator, List<LexicalTokenizer> tokens) : base(nodeOperator, tokens) { }

		public override Node TryParse(List<LexicalTokenizer> Tokens, ErrorTrack ErrorTrack)
		{
			//First, make sure the token list is not null, then check if the first token is NOT.
			if (Tokens.Count <= 0 || !(Tokens[0] is NOT))
			{
				return null;
			}

			//Call TryParse(), in the Expression node class to see if an expression follows the NOT.  
            //To do this properly, we need to pass in the list of tokens, BUT without the first (NOT) token.
			NotNode notNode = new NotNode();
			notNode.Operator = "NOT";

			//Push the node onto the Current stack
			ErrorTrack errorTrak = ErrorTrack.Push(notNode);

			//We have a NOT, now make sure there are more tokens in the list
			if (Tokens.Count <= 1)
			{
				ErrorTrack.RegisterErrorNode(notNode, Tokens[0]);
				ErrorTrack.Hint("Missing expression following the NOT term.");
				return null;
			}

			notNode.SubNode = ExpressionNode.Parser.TryParse(Tokens.Skip(1).ToList(), ErrorTrack);
			SubNode = notNode.SubNode;

			//If we did not get an expression back, it is an error; otherwise, everything is OK
			if (notNode.SubNode == null)
			{
				ErrorTrack.RegisterErrorNode(notNode, Tokens[0]);
				ErrorTrack.Hint("Expression following the NOT term is not valid.");
				return null;
			}
			else
			{
				notNode.RemainingTokens = notNode.SubNode.RemainingTokens;
				notNode.Children.Add(notNode.SubNode);
				return notNode;
			}
		}

		public override void Print()
		{
			Console.Write("{0} ", Operator);
			SubNode.Print();
		}

		public override bool Eval(string textToSearch)
		{
			return !SubNode.Eval(textToSearch);
		}

        public override bool Evaluate<TValue>(ISearchable<ObjectId, TValue> searchable, out IEnumerable<TValue> results)
        {
            IEnumerable<TValue> subNodeResults;
            if (!SubNode.Evaluate(searchable, out subNodeResults))
            {
                results = null;
                return false;
            }

            var resultsHash = subNodeResults.ToHashSet();
            results = searchable.AllValues.Where(item => !resultsHash.Contains(item));
            return results.Any();
        }

        public override string GetDisplayText()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("<span class=\"notNode\">&nbsp;NOT&nbsp;");
			sb.Append(SubNode.GetDisplayText());
			sb.Append("</span>");
			return sb.ToString();
		}

		public override List<MatchedCoordinate> GetMatchedCoordinates(string TextData, bool isSoundex, List<long> valueIDs = null)
		{
			return new List<MatchedCoordinate>();
		}

		public override string DisplayNode()
		{
			return " NOT ";
		}
	}
}
