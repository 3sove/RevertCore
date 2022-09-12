using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using MongoDB.Bson;
using Revert.Core.Search.ErrorReporting;
using Revert.Core.Common;

namespace Revert.Core.Search.Nodes
{
    // Base class
    public abstract class Node
	{
		public string Operator;
		public List<LexicalTokenizer> RemainingTokens;
		public string Hint = null;
		public string EvalError = "Eval() should not have been called on this node.";
		protected int Position = 0;
		public List<Node> Children = new List<Node>();

		protected Node() { }

		protected Node(string nodeOperator, List<LexicalTokenizer> tokenList)
		{
			Operator = nodeOperator;
			RemainingTokens = tokenList;
		}

		public virtual Node TryParse(List<LexicalTokenizer> Tokens, ErrorTrack ErrorTrack)
		{
			return null;
		}

        public virtual bool Eval(string textToSearch)
        {
            throw new Exception(EvalError);
        }

        public virtual bool Evaluate<TValue>(ISearchable<ObjectId, TValue> searchable, out IEnumerable<TValue> results)
        {
            throw new Exception(EvalError);
        }

        public virtual string GetDisplayText()
        {
            return string.Empty;
        }

        public virtual List<MatchedCoordinate> GetMatchedCoordinates(string TextData, bool isSoundex, List<long> valueIDs = null)
        {
            return new List<MatchedCoordinate>();
        }

		public virtual void Print() { }

		public virtual void DebugPrint()
		{
			Console.Write("Remaining Tokens: ");
			if (RemainingTokens == null)
			{
				Console.WriteLine("No remaining tokens.");
			}
			else
			{
				for (int i = 0; i < RemainingTokens.Count; i++)
				{
					Console.Write(RemainingTokens[i].Token + " ");
				}
			}

			Console.WriteLine("\n");
		}

		public virtual void PrintContext()
		{
			string tokenStr = (RemainingTokens.Count == 0) ? "Null" : RemainingTokens[0].Token;

			Console.WriteLine("Node Context:");
			Console.WriteLine("Token: {0}", tokenStr);
			Console.WriteLine("Node: {0}", Operator);
		}

		public virtual string DisplayNode()
		{
			return null;
		}
	}
}
