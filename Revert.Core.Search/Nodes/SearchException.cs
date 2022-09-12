using System;

namespace Revert.Core.Search.Nodes
{
    public class SearchException : SystemException
	{
		private Node errorNode;
		public Node ErrorNode
		{
			get { return errorNode; }
			set { errorNode = value; }
		}

		public SearchException(string ErrorString, Node ErrorNode) : base(ErrorString)
		{
			errorNode = ErrorNode;
		}
	}
}
