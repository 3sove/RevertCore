using System.Collections.Generic;

namespace Revert.Core.Text.Extraction.Gazetteer
{
    public class CharacterNode
    {
        public char NodeCharacter { get; set; }

        private Dictionary<char, CharacterNode> children = new Dictionary<char, CharacterNode>();
        public Dictionary<char, CharacterNode> Children
        {
            get { return children; }
            set { children = value; }
        }

        public string TermTerminated { get; set; }

        public void Populate(string termString)
        {
            Populate(termString, 0);
        }

        private void Populate(string termString, int currentPosition)
        {
            if (termString.Length <= currentPosition)
            {
                TermTerminated = termString;
                return;
            }

            var currentChar = termString[currentPosition];
            
            CharacterNode nextNode = null;
            if (!Children.TryGetValue(currentChar, out nextNode))
            {
                nextNode = new CharacterNode() { NodeCharacter = currentChar };
                Children[currentChar] = nextNode;
            }

            nextNode.Populate(termString, ++currentPosition);
        }

        //public static void PopulateTree(CharacterNode node, string termString, int currentPosition)
        //{
        //    if (termString.Length <= currentPosition)
        //    {
        //        node.TermTerminated = termString;
        //        return;
        //    }

        //    var nextCharacter = termString[currentPosition];

        //    CharacterNode nextNode = null;
        //    if (!node.Children.TryGetValue(nextCharacter, out nextNode))
        //    {
        //        nextNode = new CharacterNode() { NodeCharacter = nextCharacter };
        //        node.Children[nextCharacter] = nextNode;
        //    }

        //    PopulateTree(nextNode, termString, ++currentPosition);
        //}

        public bool Evaluate(string itemToEvaluate, int currentPosition, ref List<string> matchingTerms)
        {
            if (itemToEvaluate.Length <= currentPosition) return matchingTerms.Count > 0;
            var currentChar = itemToEvaluate[currentPosition];

            CharacterNode childNode = null;
            if (!this.Children.TryGetValue(currentChar, out childNode)) return false;

            if (TermTerminated != string.Empty) matchingTerms.Add(TermTerminated);

            return childNode.Evaluate(itemToEvaluate, ++currentPosition, ref matchingTerms);
        }
    }
}

