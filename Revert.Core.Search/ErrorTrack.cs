using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Revert.Core.Search.Nodes;

namespace Revert.Core.Search.ErrorReporting
{
    public class Context
    {
        public LexicalTokenizer Token;
        public Node Node;

        public Context(LexicalTokenizer token, Node node)
        {
            Token = token;
            Node = node;
        }
    }

    public class ErrorTrack
    {
        public Stack<Node> Current;
        public Stack<Node> WorstSoFar;
        public Node ErrorNode;
        public LexicalTokenizer ErrorToken;

        public ErrorTrack() 
        {
            Reset();
        }

        public void Reset()
        {
            Current = new Stack<Node>();
            WorstSoFar = new Stack<Node>();
            ErrorNode = null;
            ErrorToken = null;
        }

        public ErrorTrack Push(Node node)
        {
            Current.Push(node);
            return null;
        }

        public void RegisterErrorNode(Node node, LexicalTokenizer token)
        {
            Node tempNode = null;

            // If we have already come in here with a failed node, then don't bother
            if (ErrorNode == null)
            {
                ErrorNode = node;
                ErrorToken = token;
            }
            else
            {
                return;
            }

            //If Current is the bigger stack, clear out WorstSoFar and copy Current to WorstSoFar
            //Use a temporary stack because the copy inverts the stack
            if (Current.Count > WorstSoFar.Count)
            {
                // Use a temporary stack because the copy inverts the stack
                Stack<Node> tempStack = new Stack<Node>(Current);
                WorstSoFar = new Stack<Node>(tempStack);
            }

            // This should always be > 0, but just in case...
            if (Current.Count > 0)
            {
                // pop nodes off of Current until you get the one you passed in
                while (tempNode != node)
                {
                    tempNode = Current.Pop();
                }
            }
        }

        public void Hint(string hint)
        {
            if (Current.Count > 0)
            {
                Node tempNode = Current.Pop();
                tempNode.Hint = hint;
                Current.Push(tempNode);
            }
        }

        public void ErrorNodePrint()
        {
            Console.WriteLine("Error Node:");

            if (WorstSoFar.Count == 0)
            {
                Console.WriteLine("No nodes have been created;\nthe error is at the beginning of the input string.");
            }
            else
            {
                Console.WriteLine("Hint: {0}", WorstSoFar.Peek().Hint);
                WorstSoFar.Peek().DebugPrint();
            }
        }

        public string GetError()
        {
            if (WorstSoFar.Count == 0)
            {
                return "No nodes have been created; the error is at the beginning of the input string.";
            }
            else
            {
                return (WorstSoFar.Peek()).Hint;
            }
        }

        public void ErrorTokenPrint()
        {
            if (WorstSoFar.Count == 0)
            {
                Console.WriteLine("\nNo tokens have been saved;\nthe error is at the beginning of the input string.");
            }
            else
            {
                Console.WriteLine("\nError follows token: {0}", ErrorToken.Token);
                Console.WriteLine("Beginning at character position: {0}, ending at: {1}", (ErrorToken.Begin + 1).ToString(), (ErrorToken.End + 1).ToString());
            }
        }
    }
}