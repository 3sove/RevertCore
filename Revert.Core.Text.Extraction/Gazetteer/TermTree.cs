using System;
using System.Collections.Generic;

namespace Revert.Core.Text.Extraction.Gazetteer
{
    public class TermTreeModel
    {
        public GazetteerGeneratorModel GazetteerGeneratorModel { get; set; }

        private Action<string> updateMessageAction = str => Console.WriteLine(str);
        public Action<string> UpdateMessageAction
        {
            get { return updateMessageAction; }
            set { updateMessageAction = value; }
        }

        private int stringsToEvaluatePerUpdate = 50000;
        public int StringsToEvaluatePerUpdate
        {
            get { return stringsToEvaluatePerUpdate; }
            set { stringsToEvaluatePerUpdate = value; }
        }
    }

    public class TermTree
    {
        private CharacterNode rootNode = new CharacterNode();
        public CharacterNode RootNode
        {
            get { return rootNode; }
            set { rootNode = value; }
        }

        public TermTreeModel Model { get; private set; }

        public TermTree(TermTreeModel model)
        {
            Model = model;
            PopulateTree();
        }

        public void RePopulateTree(TermTreeModel model)
        {
            Model = model;
            PopulateTree();
        }

        private void PopulateTree()
        {
            var generator = new GazetteerGenerator();
            //var gazetteer = generator.Execute(Model.GazetteerGeneratorModel);

            Model.UpdateMessageAction("Gathering keywords from Gazetteer tree.  Please be patient - this may take a few seconds.");

            var keywords = Model.GazetteerGeneratorModel.GetKeywords();

            var kwCount = 0;
            var linesPerMessage = Model.StringsToEvaluatePerUpdate;

            foreach (var keyword in keywords)
            {
                if ((kwCount++ % linesPerMessage) == 1)
                {
                    Model.UpdateMessageAction(
                        string.Format("Populating Item Tree {0} / {1} ({2}%).", 
                        kwCount.ToString("#,#"), keywords.Count.ToString("#,#"), ((kwCount / (double)keywords.Count) * 100).ToString("##.#")));
                }

                RootNode.Populate(keyword);
            }
        }

        public bool Evaluate(string stringToEvaluate, out List<string> matchingTerms)
        {
            var terms = new List<string>();
            var evaluationResult = false;

            for (var i = 0; i < stringToEvaluate.Length; i++)
                evaluationResult |= RootNode.Evaluate(stringToEvaluate, i, ref terms);

            matchingTerms = terms;
            return evaluationResult;
        }
    }
}
