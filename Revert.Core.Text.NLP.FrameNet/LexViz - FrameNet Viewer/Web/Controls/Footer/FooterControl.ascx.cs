using System;

namespace LexViz.Controls.Footer
{
    public partial class FooterControl : AbstractUserControl<FooterControl, FooterViewModel>
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        public override string ControlPath
        {
            get { return "~/Controls/Footer/FooterControl.ascx"; }
        }

        protected override void LoadData(FooterViewModel viewModel)
        {            
        }
    }
}