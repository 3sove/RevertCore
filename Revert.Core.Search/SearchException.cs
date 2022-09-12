using System;
using Revert.Core.Search.Nodes;

namespace Revert.Core.Search
{
    public class SearchException : Exception
    {
        public Node ErrorNode { get; set; }
        public SearchException(string errorString, Node errorNode) : base(errorString)
        {
            ErrorNode = errorNode;
        }
    }
}