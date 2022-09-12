using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Frame = CodaForte.Text.NLP.FrameNet.Frame;

namespace ISSO.Tools.LexVizDesktop
{
    /// <summary>
    /// Interaction logic for FrameItem.xaml
    /// </summary>
    public partial class FrameItem : UserControl
    {
        private Frame frame;

        public FrameItem(Frame frame, bool loadRelated = true)
        {
            InitializeComponent();
            this.frame = frame;
            this.lblFrameName.Content = frame.Name;



            //foreach (var lexem in frame.LexicalUnits)
            //{
            //    spLexems.Children.Add(new Label()
            //    {
            //        Content = string.Format("{0} ({1}) - {2}", lexem.Name, lexem.PartOfSpeech, lexem.Definition),
            //        Style = Resources["Lexemes"] as Style
            //    });
            //}

        }


    }
}
