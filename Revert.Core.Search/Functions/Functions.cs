using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Revert.Core.Search.Nodes;
using Revert.Core.Search.ErrorReporting;

namespace Revert.Core.Search.Functions
{
	internal abstract class FunctionParser : Node
	{
		public FunctionNode FunctionNode;

		protected FunctionParser() : base() { }

		public FunctionParser(FunctionNode function) : base(function.FunctionName, function.RemainingTokens)
		{
			FunctionNode = function;
		}

		public abstract Node TryParse(FunctionNode functionNode, ErrorTrack ErrorTrack);
	}
}
