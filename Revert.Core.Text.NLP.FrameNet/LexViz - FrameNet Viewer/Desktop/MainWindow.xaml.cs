using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CodaForte.Extensions;
using CodaForte.Text.NLP.FrameNet;
using Frame = CodaForte.Text.NLP.FrameNet.Frame;

namespace ISSO.Tools.LexVizDesktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FrameNetEngine frameNetEngine;

        public MainWindow()
        {
            InitializeComponent();
            this.txtInput.KeyDown += txtInput_KeyDown;
            this.txtInput.Focus();

            var frameNetPath = System.Configuration.ConfigurationManager.AppSettings["FrameNetPath"];
            frameNetEngine = new FrameNetEngine(frameNetPath);
        }

        void txtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                Search();
            }
        }

        private void BtnSearch_OnClick(object sender, RoutedEventArgs e)
        {
            Search();
        }

        private void Search()
        {
            if (txtInput.Text == string.Empty)
            {
                MessageBox.Show("Please enter a lexical term you're interested in");
                return;
            }

            var textSearch = new CodaForte.Search.TextSearch(txtInput.Text.ToLower());
            

            tvFrame.Items.Clear();

            var term = txtInput.Text.Trim().ToLower();
            HashSet<Frame> frames;
            if (!frameNetEngine.ContainsLexeme(term))
            {
                frames = frameNetEngine.Frames.Where(frame => textSearch.Evaluate(frame.Definition.ToLower())).ToHashSet();
                if (frames.Count == 0)
                {
                    tvFrame.Items.Add(new Label
                    {
                        Content = "No results were found for the given search"
                    });
                }
                foreach (var frame in frames) PopulateFrameData(frame, tvFrame, false);
            }
            else
            {
                frames = frameNetEngine.GetFramesForLexeme(term);
                foreach (var frame in frames) PopulateFrameData(frame, tvFrame, true);
            }


        }

        private void PopulateFrameData(Frame frame, ItemsControl parent, bool includeRelations, bool exactMatch = false)
        {

            var frameViewItem = new TreeViewItem() { Header = frame.Name, FontSize = 22, Style = (exactMatch ? Resources["ExactMatchFrames"] : Resources["TextSearchFrames"]) as Style };

            if (frame.LexicalUnits != null && frame.LexicalUnits.Count != 0)
            {
                var lexemsTreeViewItem = new TreeViewItem() {Header = "Lexical Units", FontSize = 16};
                foreach (var lu in frame.LexicalUnits)
                {
                    
                    var luTreeViewItem = new TreeViewItem() {Header = string.Format("{0} ({1}) - {2}", lu.Name, lu.PartOfSpeech, lu.Definition), FontSize = 12};
                    lexemsTreeViewItem.Items.Add(luTreeViewItem);
                }
                frameViewItem.Items.Add(lexemsTreeViewItem);
            }

            if (includeRelations)
            {

                var attestations = frameNetEngine.GetAttestationsForFrame(frame);

                if (attestations != null && attestations.Count != 0)
                {
                    var attestationsTreeViewItem = new TreeViewItem() { Header = "Attestations", FontSize = 16 };

                    foreach (var attestation in attestations)
                    {
                        var attestationItem = new TreeViewItem() { Header = attestation.Sentence, FontSize = 14 };
                        attestationsTreeViewItem.Items.Add(attestationItem);

                        foreach (var annotatedSpan in attestation.AnnotatedSpansByFrameElement)
                        {
                            var annotatedSpanItem = new TreeViewItem() { Header = annotatedSpan.Key.Name + " - " + 
                                annotatedSpan.Value.Select(value => value.Value).Combine(", "), FontSize = 14 };

                            attestationItem.Items.Add(annotatedSpanItem);
                        }
                    }
                    frameViewItem.Items.Add(attestationsTreeViewItem);

                }
                          
                if (frame.RelationSuperFramesByFrameRelation.Count != 0)
                {
                    var parentFrames = new TreeViewItem() { Header = "Parent Frames", FontSize = 16};
                    foreach (var item in frame.RelationSuperFramesByFrameRelation)
                    {
                        var relationItem = new TreeViewItem() { Header = item.Key.ToString(), FontSize = 12};
                        item.Value.ToList().ForEach(relatedframe => PopulateFrameData(relatedframe, relationItem, false));
                        parentFrames.Items.Add(relationItem);
                    }
                    frameViewItem.Items.Add(parentFrames);
                }

                if (frame.RelationSubFramesByFrameRelation.Count != 0)
                {
                    var childFrames = new TreeViewItem() { Header = "Child Frames", FontSize = 16};
                    foreach (var item in frame.RelationSubFramesByFrameRelation)
                    {
                        var relationItem = new TreeViewItem() { Header = item.Key.ToString(), FontSize = 14};
                        item.Value.ToList().ForEach(relatedframe => PopulateFrameData(relatedframe, childFrames, false));
                        childFrames.Items.Add(relationItem);
                    }
                    frameViewItem.Items.Add(childFrames);
                }
            }

            parent.Items.Add(frameViewItem);
            frameViewItem.IsExpanded = true;
        }
    }
}
