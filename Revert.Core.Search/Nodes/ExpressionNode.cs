using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Revert.Core.Search.ErrorReporting;
using System.Collections;

namespace Revert.Core.Search.Nodes
{
    internal class ExpressionNode : Node
    {
        public static readonly ExpressionNode Parser = new ExpressionNode();

        private ExpressionNode() : base() { }

        public ExpressionNode(string token, List<LexicalTokenizer> tokens) : base(token, tokens) { }

        public override Node TryParse(List<LexicalTokenizer> Tokens, ErrorTrack ErrorTrack)
        {
            // Check for parenthesis first
            if (Tokens[0] is LeftParenthesis)
            {
                // After a LeftParen comes an OR node
                List<LexicalTokenizer> newTokenList = Tokens.Skip(1).ToList();
                Node orNode = OrNode.Parser.TryParse(newTokenList, ErrorTrack);
                if (orNode == null)
                {
                    // Didn't get an OR node...this is an error
                    ErrorTrack.Hint("The expression after the left parenthesis is missing or incorrect.");
                    return null;
                }

                // The first token in the list of remaining tokens should be the right paren
                if (orNode.RemainingTokens.Count > 0 && orNode.RemainingTokens[0] is RightParenthesis)
                {
                    List<LexicalTokenizer> newOrNodeRemainingTokens = orNode.RemainingTokens.Skip(1).ToList();
                    orNode.RemainingTokens = newOrNodeRemainingTokens;

                    //return an expression?
                    return orNode;
                }
                else
                {
                    ErrorTrack.Hint("Missing end parenthesis.");
                    return null;
                }
            }

            // Check or-expression node
            Node notNode = NotNode.Parser.TryParse(Tokens, ErrorTrack);
            if (notNode != null)
                return notNode;

            //Check for a function
            Node functionNode = FunctionNode.Parser.TryParse(Tokens, ErrorTrack);

            //If functionNode is NOT null, return it; otherwise return a new TokenNode based on the tokens.
            //Note: if we don't have a TERM, the call to TryParse() will return null, which is what we want in that case.



            return functionNode ?? TokenNode.Parser.TryParse(Tokens, ErrorTrack);

        }

        public override bool Eval(string textToSearch)
        {
            throw new SearchException(EvalError, this);
        }
    }
}
