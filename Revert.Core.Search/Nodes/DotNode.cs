using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Revert.Core.Search.ErrorReporting;
using System.Collections;

namespace Revert.Core.Search.Nodes
{
    internal class DotNode : Node
    {
        public static readonly DotNode Parser = new DotNode();

        private DotNode() : base() { }

        public DotNode(string token, List<LexicalTokenizer> tokens) : base(token, tokens) { }

        public override Node TryParse(List<LexicalTokenizer> Tokens, ErrorTrack ErrorTrack)
        {
            if (Tokens.Count <= 0) return null;

            //We have tokens...so check if they represent a Property node or an Expression node
            Node propertyNode = PropertyNode.Parser.TryParse(Tokens, ErrorTrack);

            //Here we check the results of TryParse for a PropertyNode.  
            //If propertyNode is NOT null, return it; otherwise check if we have an Expression node and return the results which will be either an Expression node or null
            if (propertyNode != null) return propertyNode;

            Node expressionNode = ExpressionNode.Parser.TryParse(Tokens, ErrorTrack);
            return expressionNode;
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
