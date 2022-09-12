using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using MongoDB.Bson;
using Revert.Core.Search.ErrorReporting;
using Revert.Core.Common;

namespace Revert.Core.Search.Nodes
{
    internal class AndNode : WizardNode
	{
		public Node LeftNode;
		public Node RightNode;

		public static readonly AndNode Parser = new AndNode();

		private AndNode() : base() { }

		public AndNode(string nodeOperator, List<LexicalTokenizer> tokens) : base(nodeOperator, tokens) { }

		public override Node TryParse(List<LexicalTokenizer> Tokens, ErrorTrack ErrorTrack)
		{
			/*
			 * First, check that there are tokens in the list.
			 * 
			 * Next, check for an expression.  If there is NOT an expression, just return NULL.
			 * If we get an expression, check if there are more tokens.  If there are NO MORE
			 * tokens, just return the expression node.  If there ARE MORE nodes, build the
			 * AND node.
			 */
			if (Tokens.Count <= 0)
			{
				return null;
			}

			Node dotNode = DotNode.Parser.TryParse(Tokens, ErrorTrack);
			if (dotNode == null)
			{
				// This is an error...the first part must be an expression...
				return null;
			}

			if (dotNode.RemainingTokens.Count < 0)
			{
				// Error...should never happen
				return null;
			}

			if (dotNode.RemainingTokens.Count == 0)
			{
				// This is not an error...this simply isn't an AND node
				return dotNode;
			}

			/*
			 * Tokens remaining...check for an AND token; if there is one, simply remove it
			 * by stripping it from the list of tokens returned
			 */
			if (dotNode.RemainingTokens[0] is AND)
			{
				// Strip the AND token from the token list
				List<LexicalTokenizer> newTokenList = dotNode.RemainingTokens.Skip(1).ToList();

				AndNode andNode = new AndNode("AND", newTokenList.Skip(1).ToList());

				// Push the node onto the Current stack
				ErrorTrack errorTrak = ErrorTrack.Push(andNode);

				// Make sure something exists after AND
				if (newTokenList.Count <= 0)
				{
					// This is an error as there should be something following "AND"
					ErrorTrack.RegisterErrorNode(andNode, dotNode.RemainingTokens[0]);
					return null;
				}

				andNode.LeftNode = dotNode;
				andNode.RightNode = AndNode.Parser.TryParse(newTokenList, ErrorTrack);
				LeftNode = andNode.LeftNode;
				RightNode = andNode.RightNode;
				
				// Make sure there was no error
				if (andNode.RightNode == null)
				{
					// error
					ErrorTrack.RegisterErrorNode(andNode, dotNode.RemainingTokens[0]);
					//Node lastSuccessful = ErrorTrak.

					return null;
				}

				andNode.RemainingTokens = andNode.RightNode.RemainingTokens;

				andNode.Children.Add(LeftNode);
				andNode.Children.Add(RightNode);
				return andNode;
			}
			else
			{
				/*
				 * Here, we have more tokens, but the next one is not AND.  So check if the
				 * next one is a special character, like a ')' or '}'
				 * If the next token IS a special character, return the expression node,
				 * otherwise, build an AND expression
				if (HelperFunctions.CheckForSpecialChar(dotNode.RemainingTokens[0].Token[0]))
				 */
				if (dotNode.RemainingTokens[0] is RightParenthesis || dotNode.RemainingTokens[0] is Colon)
				{
					return dotNode;
				}
				else
				{
					AndNode andNode = new AndNode("AND", dotNode.RemainingTokens.Skip(1).ToList());

					// Push the node onto the Current stack
					ErrorTrack errorTrak = ErrorTrack.Push(andNode);

					andNode.LeftNode = dotNode;

					/*
					 * Check for an AND expression - if it is an AND expression, return an AndNode,
					 * otherwise, return the expression node
					 */
					andNode.RightNode = AndNode.Parser.TryParse(dotNode.RemainingTokens, ErrorTrack);

					if (andNode.RightNode != null)
					{
						andNode.RemainingTokens = andNode.RightNode.RemainingTokens;
						andNode.Children.Add(andNode.LeftNode);
						andNode.Children.Add(andNode.RightNode);
						return andNode;
					}
					else
					{
						// This is an error...but it may not be...
						//ErrorTrak.Instance.NeverMind(andNode, dotNode.RemainingTokens[0]);

						if ((ErrorTrack.Current.Count > 0 && 
							ErrorTrack.Current.Peek().Hint != null) ||
							ErrorTrack.WorstSoFar.Count > 0)
						{
							ErrorTrack.RegisterErrorNode(andNode, dotNode.RemainingTokens[0]);
							return null;
						}
						else
						{
							return dotNode;
						}
					}
				}
			}
		}

		public override void DebugPrint()
		{
			Console.WriteLine("Node Operator: {0}", Operator);

			Console.WriteLine("\nAND Node-Left Node:");
			if (LeftNode == null)
			{
				Console.WriteLine("NULL");
			}
			else
			{
				LeftNode.DebugPrint();
			}

			Console.WriteLine("\nAND Node-Right Node:");
			if (RightNode == null)
			{
				Console.WriteLine("NULL");
			}
			else
			{
				RightNode.DebugPrint();
			}

			if (Hint != null)
			{
				Console.WriteLine("Hint: {0}", Hint);
			}
		}

		public override void Print()
		{
			LeftNode.Print();
			Console.Write("{0} ", Operator);
			RightNode.Print();
		}

		public override void PrintContext()
		{
			//Console.WriteLine("Printing AndNode Context");
			Console.WriteLine("And Node Left: ");
			LeftNode.PrintContext();
			Console.WriteLine("\nAnd Node Right: ");
			RightNode.PrintContext();
		}

		public override bool Eval(string textToSearch)
		{
			if (LeftNode.Eval(textToSearch))
			{
				return RightNode.Eval(textToSearch);
			}
			else
			{
				// Left node failed...doesn't matter about right one
				return false;
			}
		}

        public override bool Evaluate<TValue>(ISearchable<ObjectId, TValue> searchable, out IEnumerable<TValue> results)
        {
            IEnumerable<TValue> leftResults;
            IEnumerable<TValue> rightResults = null;
            bool result = LeftNode.Evaluate(searchable, out leftResults) && RightNode.Evaluate(searchable, out rightResults);
            results = leftResults?.Intersect(rightResults ?? new List<TValue>());
            return result;
        }

        public override string GetDisplayText()
		{
            return string.Format(@"<span class=""andNode"">{0}&nbsp;AND&nbsp;</span>", LeftNode.GetDisplayText(), RightNode.GetDisplayText());
		}

		public override List<MatchedCoordinate> GetMatchedCoordinates(string TextData, bool isSoundex, List<long> valueIDs = null)
		{
			List<MatchedCoordinate> coordinates = LeftNode.GetMatchedCoordinates(TextData, isSoundex);
			coordinates.AddRange(RightNode.GetMatchedCoordinates(TextData, isSoundex));
			return coordinates;
		}
        
		public override string DisplayNode()
		{
			return " AND ";
		}
	}
}
