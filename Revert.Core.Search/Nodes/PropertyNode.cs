using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Revert.Core.Search.ErrorReporting;
using System.Collections;
using Revert.Core.Common;

namespace Revert.Core.Search.Nodes
{
    internal class PropertyNode : Node
	{
		public Node EntityType;
		public Node EntityProperty;

		public static readonly PropertyNode Parser = new PropertyNode();

		private PropertyNode() : base() { }

		public PropertyNode(string nodeOperator, List<LexicalTokenizer> tokens) : base(nodeOperator, tokens) { }

		public override Node TryParse(List<LexicalTokenizer> Tokens, ErrorTrack ErrorTrack)
		{
			// Make sure there are at least 3 tokens in the list
			if (Tokens.Count <= 2)
			{
				// Nothing here
				return null;
			}

			// Second token must be a Dot
			if ( ! (Tokens[1] is Dot) )
			{
				return null;
			}

			// At this point, if we have a Wildcard as the EntityType, then we must also have a Wildcard as the EntityProperty
			if (Tokens[0] is Wildcard)
			{
				if (!(Tokens[2] is Wildcard))
				{
					return null;
				}
			}

			// Now, the first token can be only an alpha or alphanumeric
			if (Tokens[0] is Alpha || Tokens[0] is AlphaNumeric)
			{
				/* Check that the 2nd token is a dot and the third is an alpha or alphanumeric
				if ( ! CheckDotAlphaNumeric(Tokens.Skip(1).ToList()))
				{
					return null;
				}
				 */
				if (!(Tokens[2] is Alpha || Tokens[2] is AlphaNumeric))
				{
					return null;
				}
			}

			BuildTypeAndPropertyNodes(Tokens);

			PropertyNode propertyNode = new PropertyNode();
			propertyNode.EntityType = EntityType;
			propertyNode.EntityProperty = EntityProperty;
			propertyNode.RemainingTokens = Tokens.Skip(3).ToList();     // Skip type, dot, property, AND comma(,)

			// Push the node onto the Current stack
			ErrorTrack.Push(propertyNode);

			return propertyNode;
		}

		//Check that the second token (from list passed into TryParse()) is a dot (.) and that the third token is an AlphaNumeric
		//Note: in this method, it is the first and second tokens, as the first token in the list was stripped off when passed here
		private bool CheckDotAlphaNumeric(List<LexicalTokenizer> Tokens)
		{
            // First token must be a dot - Second token must be an alpha or alphanumeric
            if (!(Tokens[0] is Dot) || !(Tokens[1] is Alpha || Tokens[1] is AlphaNumeric)) return false;

            return true;
		}

		//Build the EntityType and EntityProperty nodes
		private void BuildTypeAndPropertyNodes(List<LexicalTokenizer> Tokens)
		{
			if (Tokens[0] is Wildcard)
			{
				Wildcard wildCardToken = new Wildcard(Tokens[0].Token, 0, 0);
				EntityType = new TokenNode(Tokens.Skip(2).ToList(), wildCardToken);
			}
			else
			{
				AlphaNumeric alphaNumericTokenEntity = new AlphaNumeric(Tokens[0].Token, 0, 0, 0);
				EntityType = new TokenNode(Tokens.Skip(2).ToList(), alphaNumericTokenEntity);
			}

			AlphaNumeric alphaNumericTokenProperty = new AlphaNumeric(Tokens[2].Token, 0, 0, 0);

            //For the RemainingToken list, skip the EntityType, the dot, the EntityProperty, AND the comma which is before the rest of the arguments
			EntityProperty = new TokenNode(Tokens.Skip(3).ToList(), alphaNumericTokenProperty);
		}

		public override bool Eval(string textToSearch)
		{
			throw new SearchException(EvalError, this);
		}

		public override string GetDisplayText()
		{
			return String.Empty;
		}

		public override List<MatchedCoordinate> GetMatchedCoordinates(string TextData, bool isSoundex, List<long> valueIDs = null)
        {
			return new List<MatchedCoordinate>();
		}
	}
}
