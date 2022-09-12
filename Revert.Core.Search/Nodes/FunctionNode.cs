using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Revert.Core.Search.ErrorReporting;
using System.Collections;
using Revert.Core.Search.Functions;

namespace Revert.Core.Search.Nodes
{
	internal class FunctionNode : Node
	{
		public string FunctionName;
		public List<Node> ParamNodes;

		public static FunctionNode Parser = new FunctionNode();

		private FunctionNode() : base() { }

		public FunctionNode(string token, List<LexicalTokenizer> tokens) : base(token, tokens) { }

		public override Node TryParse(List<LexicalTokenizer> Tokens, ErrorTrack ErrorTrack)
		{
			List<LexicalTokenizer> RemainingTokens = null;

            //The list must NOT be NULL, AND there must be at least a colon (:) following a function name
            //for a total of 2 tokens.  Of course, there must be parameters, too, but we'll check that later.
			if (Tokens.Count < 2)
				return null;

			//First token after the function name must be a colon (:);
			//if it is NOT a token, we don't have a function
			if ( ! (Tokens[1] is Colon) )
				return null;
            
			//At this point, it is quite likely that the user meant to use a function, so start to build a function node.
			FunctionNode functionNode = new FunctionNode("FUNCTION", Tokens.Skip(1).ToList());
			functionNode.FunctionName = Tokens[0].Token.ToUpper();

			//Push the node onto the Current stack
			ErrorTrack.Push(functionNode);

			//Following a colon is a term, OR a left-paren followed by a param-list
			if (Tokens.Count < 3)
			{
				//This is a real error; we have a function name and a colon, but nothing else
				ErrorTrack.Hint("Missing function parameters.");
				ErrorTrack.RegisterErrorNode(functionNode, Tokens[1]);
				return null;
			}

			List<Node> paramList;
			if (Tokens[2] is LeftParenthesis)
			{
				//Here we should have a param-list
				RemainingTokens = Tokens.Skip(3).ToList();
				paramList = GetParamList(ref RemainingTokens, ErrorTrack);
				if (paramList == null)
				{
					ErrorTrack.Hint("Missing or invalid parameters after '('.");
					ErrorTrack.RegisterErrorNode(functionNode, Tokens[2]);
					return null;
				}
			}
			else
			{
				//No parenthesis, so there can only be one argument and it must be a TERM create a RemainingTokens list with the rest of the tokens
				RemainingTokens = Tokens.Skip(2).ToList();
				Node TokenNode = Nodes.TokenNode.Parser.TryParse(RemainingTokens, ErrorTrack);
				if (TokenNode == null)
					return null;

                RemainingTokens = TokenNode.RemainingTokens;
				//Create a paramList with the single param.
				paramList = new List<Node>();
				paramList.Add(TokenNode);
			}

			//What RemainingTokens in both cases?
			functionNode.ParamNodes = paramList;
			functionNode.RemainingTokens = RemainingTokens;

			// Now step through the list of actual functions in order to validate the function we have
			List<FunctionParser> Parsers = new List<FunctionParser>();
			InitializeFunctionParsers(ref Parsers);

			foreach (FunctionParser parser in Parsers)
			{
				Node newFunctionNode = parser.TryParse(functionNode, ErrorTrack);
				if (newFunctionNode != null)
				{
					newFunctionNode.RemainingTokens = RemainingTokens;
					return newFunctionNode;
				}
			}

            //If we are here, the potential function did not match any existing functions, so we don't have a function node
			ErrorTrack.RegisterErrorNode(functionNode, Tokens[2]);
			return null;
		}

		private void InitializeFunctionParsers(ref List<FunctionParser> Parsers)
		{
			Parsers.Add(OrFunction.Parser);
			Parsers.Add(AndFunction.Parser);
			Parsers.Add(DistanceFromBeginningFunction.Parser);
			Parsers.Add(DistanceFromEndFunction.Parser);
		}

		private List<Node> GetParamList(ref List<LexicalTokenizer> Tokens, ErrorTrack ErrorTrack)
		{
			if (Tokens.Count <= 0 || Tokens[0] is RightParenthesis)
			{
				//List is empty
				return null;
			}

			List<Node> paramList = new List<Node>();

			Node paramNode = WildcardOrNode(Tokens, ErrorTrack);

			while (paramNode != null)
			{
				paramList.Add(paramNode);

				if (paramNode.RemainingTokens.Count == 0)
				{
					//If this happens, it is usually because the RightParen is missing
					ErrorTrack.Hint("The right parenthesis is missing.");
					return null;
				}

				if (paramNode.RemainingTokens[0] is RightParenthesis)
				{
					Tokens = paramNode.RemainingTokens.Skip(1).ToList();
					return paramList;
				}

				List<LexicalTokenizer> newTokenList;

				//Next token must be a Comma (since it wasn't a RightParen)
				if ( ! (paramNode.RemainingTokens[0] is Comma) )
				{
					ErrorTrack.Hint("A comma is missing in the list of function arguments.");
					return null;
				}

				newTokenList = paramNode.RemainingTokens.Skip(1).ToList();

				if (newTokenList.Count == 0)
				{
					ErrorTrack.Hint("A parameter is missing after the comma.");
					return null;
				}

				paramNode = WildcardOrNode(newTokenList, ErrorTrack);
			}

            //Shouldn't have fallen out of the loop here.  Must be missing the RightParen
			return null;
		}

		public Node WildcardOrNode(List<LexicalTokenizer> Tokens, ErrorTrack ErrorTrack)
		{
			Node paramNode;

			if (Tokens[0] is Wildcard && Tokens.Count == 1)
            {

            }


			if (Tokens[0] is Wildcard && Tokens[1] is Comma)
			{
				Wildcard wildCardToken = new Wildcard(Tokens[0].Token, 0, 0);
				paramNode = new TokenNode(Tokens.Skip(1).ToList(), wildCardToken);
			}
			else
			{
				paramNode = OrNode.Parser.TryParse(Tokens, ErrorTrack);
			}

			return paramNode;
		}

		public override void DebugPrint()
		{
			Console.WriteLine("Node Operator: {0}", Operator);

			Console.WriteLine("Function Name: {0}", FunctionName);
			Console.WriteLine("\nParam List:");
			if (ParamNodes == null)
			{
				Console.WriteLine("Param list is empty.");
			}
			else
			{
				int l = ParamNodes.Count;
				for (int i = 0; i < ParamNodes.Count; i++)
				{
					Node node = ParamNodes[i];
					node.DebugPrint();
				}
			}
		}

		public override void Print()
		{
			Console.Write("{0}(", FunctionName);
			for (int i = 0; i < ParamNodes.Count; i++)
			{
				ParamNodes[i].Print();
			}
			Console.Write(") ");
		}

		public override bool Eval(string textToSearch)
		{
			throw new SearchException(EvalError, this);
		}

		public override string GetDisplayText()
		{
			return String.Empty;
		}
	}
}
