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
    internal class OrNode : WizardNode
    {
        public Node LeftNode;
        public Node RightNode;

        public static readonly OrNode Parser = new OrNode();

        private OrNode() : base() { }

        public OrNode(string nodeOperator, List<LexicalTokenizer> tokens) : base(nodeOperator, tokens) { }

        public override Node TryParse(List<LexicalTokenizer> Tokens, ErrorTrack ErrorTrack)
        {
            //First, check that there are tokens in the list.  Next, check for an AND expression.  If there is NOT an AND expression, just return NULL. 
            //If we get an AND expression, check if there are more tokens.  If there are NO MORE tokens, just return the AND expression node.  If there ARE MORE nodes, build the OR node.
            if (Tokens.Count <= 0)
            {
                return null;
            }

            Node andNode = AndNode.Parser.TryParse(Tokens, ErrorTrack);
            if (andNode == null) return null; //This is an error.  The first part must be an and-expression

            // We have an andNode
            if (andNode.RemainingTokens.Count == 0) return andNode;

            //OK, we have an andNode and there are more tokens after the andNode, so check for an OR token; 
            //if there is one, simply remove it by stripping it from the list of tokens returned
            if (!(andNode.RemainingTokens[0] is OR)) return andNode;

            //We have an OR token, so strip the OR token from the token list
            List<LexicalTokenizer> newTokenList = andNode.RemainingTokens.Skip(1).ToList();

            OrNode orNode = new OrNode("OR", newTokenList);

            //Push the node onto the Current stack
            ErrorTrack errorTrak = ErrorTrack.Push(orNode);

            orNode.LeftNode = andNode;

            //Make sure something exists after OR
            if (newTokenList.Count > 0)
            {
                orNode.RightNode = OrNode.Parser.TryParse(orNode.RemainingTokens, ErrorTrack);
                RightNode = orNode.RightNode;
                orNode.Children.Add(RightNode);
                orNode.RemainingTokens = orNode.RightNode.RemainingTokens;
            }

            LeftNode = orNode.LeftNode;

            orNode.Children.Add(LeftNode);
            return orNode;
        }

        public override void DebugPrint()
        {
            Console.WriteLine("Node Operator: {0}", Operator);
            Console.WriteLine("\nOR Node-Left Node:");

            if (LeftNode == null)
            {
                Console.WriteLine("NULL");
            }
            else
            {
                LeftNode.DebugPrint();
            }

            Console.WriteLine("\nOR Node-Right Node:");
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
            Console.WriteLine("Or Node Left:");
            LeftNode.PrintContext();
            Console.WriteLine("\nOr Node Right:");
            RightNode.PrintContext();
        }

        public override bool Eval(string textToSearch)
        {
            return LeftNode.Eval(textToSearch) || RightNode.Eval(textToSearch);
        }
        
        public override bool Evaluate<TValue>(ISearchable<ObjectId, TValue> searchable, out IEnumerable<TValue> results)
        {
            IEnumerable<TValue> leftResults;
            IEnumerable<TValue> rightResults = null;

            var leftSuccess = LeftNode.Evaluate(searchable, out leftResults);
            var rightSuccess = RightNode.Evaluate(searchable, out rightResults);

            if (!leftSuccess && !rightSuccess)
            {
                results = null;
                return false;
            }

            if (leftResults == null)
                results = rightResults;
            else if (rightResults == null) results = leftResults;
            else results = leftResults.Union(rightResults);
            return results != null && results.Any();
        }

        public override string GetDisplayText()
        {
            return string.Format(@"<span class=""orNode"">{0}&nbsp;OR&nbsp;{1}</span>", LeftNode.GetDisplayText(), RightNode.GetDisplayText());
        }

        public override List<MatchedCoordinate> GetMatchedCoordinates(string TextData, bool isSoundex, List<long> valueIDs = null)
        {
            List<MatchedCoordinate> coordinates = LeftNode.GetMatchedCoordinates(TextData, isSoundex);
            if (RightNode != null) coordinates.AddRange(RightNode.GetMatchedCoordinates(TextData, isSoundex));
            return coordinates;
        }
        
        public override string DisplayNode()
        {
            return " OR ";
        }
    }
}
