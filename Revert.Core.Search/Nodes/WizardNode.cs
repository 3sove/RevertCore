using System.Collections.Generic;

namespace Revert.Core.Search.Nodes
{
    public abstract class WizardNode : Node
	{
		protected WizardNode() { }

		protected WizardNode(string nodeOperator, List<LexicalTokenizer> tokenList) : base(nodeOperator, tokenList)
		{
		}
	}
}
