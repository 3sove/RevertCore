using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Revert.Core.Search.Nodes;
using Revert.Core.Search.ErrorReporting;
using System.Collections;

namespace Revert.Core.Search.Functions
{
	internal class DistanceFromEndFunction : FunctionParser
	{
		public string FunctionName = "DISTANCEFROMEND";

		public Node Param_1;
		public int Distance;

		public static readonly DistanceFromEndFunction Parser = new DistanceFromEndFunction();

		private DistanceFromEndFunction() : base() { }

		private DistanceFromEndFunction(FunctionNode functionNode, Node param_1, int distance)
			: base(functionNode)
		{
			Param_1 = param_1;
			Distance = distance;
		}

		public override Node TryParse(FunctionNode functionNode, ErrorTrack ErrorTrack)
		{
			// Very first test...does name match?
			if (functionNode.FunctionName.ToUpper().Trim() != FunctionName)
				return null;

			// Must have 2 parameters
			if (functionNode.ParamNodes.Count != 2)
			{
				ErrorTrack.Hint("Incorrect number of parameters for the " + FunctionName + " function.");
				return null;
			}

			// First param must be a TERM
			Node param_1 = functionNode.ParamNodes[0];
			if (!(param_1 is TokenNode))
			{
				ErrorTrack.Hint("The first parameter for the " + FunctionName + " function\n is not of the correct type; it must be a term.");
				return null;
			}

			// Second param must be an integer TERM
			TokenNode distanceNode = (TokenNode)functionNode.ParamNodes[2];
			if (!(distanceNode is TokenNode))
			{
				ErrorTrack.Hint("The second parameter for the " + FunctionName + " function\n is not of the correct type; it must be a term.");
				return null;
			}

			int distance;
			if (int.TryParse(distanceNode.Value, out distance))
			{
				DistanceFromEndFunction distanceFromEndFunction = new DistanceFromEndFunction(functionNode, param_1, distance);

				distanceFromEndFunction.Children.Add(param_1);
				distanceFromEndFunction.Children.Add(distanceNode);

				return distanceFromEndFunction;
			}
			else
			{
				ErrorTrack.Hint("The second parameter for the " + FunctionName + " function\n is not of the correct type; it must be an integer.");
				return null;
			}
		}

		public override void DebugPrint()
		{
			Console.WriteLine("Function: {0}", FunctionName);
			Console.Write("Param 1: ");
			Param_1.DebugPrint();
			Console.Write("Distance: {0}", Distance);
			Console.WriteLine("End of {0} funciton.", FunctionName);
		}

		public override void Print()
		{
			Console.Write("{0} (", FunctionName);
			Param_1.Print();
			Console.Write(", {0}", Distance.ToString());
			Console.WriteLine(")\n");
		}
	}
}
