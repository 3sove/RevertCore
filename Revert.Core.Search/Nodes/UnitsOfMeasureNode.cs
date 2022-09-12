using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;

namespace Revert.Core.Search.Nodes
{
	class UnitsOfMeasureNode : Node
	{
		public const string UNITS_OF_MEASURE_ENTITY = "Units of measurement";
		public TokenNode NumericData;
		public TokenNode Units;

		public static readonly UnitsOfMeasureNode Parser = new UnitsOfMeasureNode();

		private UnitsOfMeasureNode() : base() { }

		public UnitsOfMeasureNode(TokenNode numericData, TokenNode units)
		{
			NumericData = numericData;
			Units = units;
		}

		//public override Node TryParse(List<LexicalTokenizer> Tokens, ErrorTrack ErrorTrack)
		public UnitsOfMeasureNode TryParse(Node node, UnitsOfMeasureNode other = null)
		{
			if (node is TokenNode)
			{
                //We have only one node; this node must contain the numeric data and the units (alpha numeric) in one string
                TokenNode TokenNode = (TokenNode)node;
				string numeric = null;
				string alpha = null;

				// separate out the numeric data from the alpha numeric units
				if (!parseNumericAlpha(TokenNode.Value, out numeric, out alpha))
				{
					// if the other part was passed in, then it can use the 'others' units
					if (other == null)
					{
						return null;
					}
					Units = other.Units;
				}
				else
				{
					Units = AlphaTokenNodeBuilder(alpha);
				}
				NumericData = NumericTokenNodeBuilder(numeric);
				return new UnitsOfMeasureNode(NumericData, Units);
			}
			if (!(node is AndNode))
			{
				return null;
			}

			AndNode andNode = (AndNode)node;

			if ( ! (andNode.LeftNode is TokenNode) )
			{
				return null;
			}

            TokenNode leftNode = (TokenNode)andNode.LeftNode;
			if (leftNode.TokenType.Name != "Numeric")
			{
				return null;
			}

			if ( ! (andNode.RightNode is TokenNode) )
			{
				return null;
			}

            TokenNode rightNode = (TokenNode)andNode.RightNode;
			if ( ! CheckAlphaOrAlphaNumeric(rightNode) )
			{
				return null;
			}

			// Validation is complete, now build the nodes
			NumericData = NumericTokenNodeBuilder(leftNode.Value);
			Units = AlphaNumericTokenNodeBuilder(rightNode.Value);

			return new UnitsOfMeasureNode(NumericData, Units);
		}

        // Build a TERM node with a Numeric token
		private TokenNode NumericTokenNodeBuilder(string numericData)
		{
			Numeric numericToken = new Numeric(numericData, 0, numericData.Length - 1, 0);
			return new TokenNode(null, numericToken);
		}

        // Build a TERM node with an Alpha token
		private TokenNode AlphaTokenNodeBuilder(string alphaData)
		{
			Alpha alphaToken = new Alpha(alphaData, 0, alphaData.Length - 1, 0);
			return new TokenNode(null, alphaToken);
		}

        // Build a TERM node with an AlphaNumeric token
		private TokenNode AlphaNumericTokenNodeBuilder(string alphaNumericData)
		{
			AlphaNumeric alphaNumericToken = new AlphaNumeric(alphaNumericData, 0, alphaNumericData.Length - 1, 0);
			return new TokenNode(null, alphaNumericToken);
		}

        //This method will separate out numeric and alpha data from a string the numeric data must be in the first part of the data string and
		//the alpha characters must be in the second part of the data string.
		private bool parseNumericAlpha(string data, out string numeric, out string alpha)
		{
			numeric = null;
			alpha = null;

            //The following regular expression checks that the first part of the string contains only digits and the second part contains only alpha characters
			Regex rx = new Regex(@"(?<numeric>(^[0-9]+))(?<alpha>([a-z]+$))", RegexOptions.IgnoreCase);
			Match match = rx.Match(data);

			if (match.Success)
			{
				numeric = match.Groups["numeric"].ToString();
				alpha = match.Groups["alpha"].ToString();
				return true;
			}
			return false;
		}

		// Check if the TERM node has an Alpha or an AlphaNumeric token
		private bool CheckAlphaOrAlphaNumeric(TokenNode TokenNode)
		{
            return TokenNode.TokenType.Name == "Alpha" || TokenNode.TokenType.Name == "AlphaNumeric";
		}

		public override void DebugPrint()
		{
			Console.WriteLine("Numeric Data: ");
			NumericData.DebugPrint();
			Console.WriteLine("Units: ");
			Units.DebugPrint();
		}

		public override void Print()
		{
			NumericData.Print();
			Console.Write(", ");
			Units.Print();
		}

		public override bool Eval(string textToSearch)
		{
			throw new Exception(EvalError);
		}
	}
}
