using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Revert.Core.Search.Nodes;
using Revert.Core.Search.ErrorReporting;
using System.Collections;

namespace Revert.Core.Search.Functions
{
    internal class OrFunction : FunctionParser
    {
        public string FunctionName = "OR";
        public List<Node> ParamNodes = new List<Node>();

        public static readonly OrFunction Parser = new OrFunction();

        private OrFunction() : base() { }

        private OrFunction(FunctionNode functionNode) : base(functionNode)
        {
            for (int i = 0; i < functionNode.ParamNodes.Count; i++)
            {
                ParamNodes.Add(functionNode.ParamNodes[i]);
            }
        }

        public override Node TryParse(FunctionNode functionNode, ErrorTrack ErrorTrack)
        {
            // Very first test...does the name match?
            if (functionNode.FunctionName.ToUpper().Trim() != FunctionName)
                return null;

            // Must have at least one parameter node
            if (functionNode.ParamNodes.Count < 1)
            {
                ErrorTrack.Hint(string.Format("Incorrect number of parameters for the {0} function.", FunctionName));
                return null;
            }

            return new OrFunction(functionNode);
        }

        public override void Print()
        {
            Console.WriteLine("Print out OR function.");
            Console.Write("{0} (", FunctionName);
            foreach (Node paramNode in ParamNodes)
            {
                paramNode.Print();
                Console.Write(", ");
            }
            Console.WriteLine(")");
        }
    }
}
